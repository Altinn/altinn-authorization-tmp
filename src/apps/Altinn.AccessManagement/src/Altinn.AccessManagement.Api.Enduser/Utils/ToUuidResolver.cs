using System.Diagnostics;
using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Api.Enduser.Validation;
using Altinn.AccessManagement.Core;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models.Profile;
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Api.Enduser.Utils;

/// <summary>
/// Resolves the target (<c>to</c>) party UUID for AddAssignment operations.
///
/// Resolution follows this order:
/// <list type="number">
/// <item>
/// If <c>"toPartyUuid"</c> is provided and refers to a non-person entity,
/// it is resolved directly via <see cref="IEntityService"/>.
/// </item>
/// <item>
/// If <c>toPartyUuid</c> refers to a person entity, the resolver first
/// checks whether the person has an existing relationship with the calling
/// <c>party</c>.
/// <list type="bullet">
/// <item>
/// If a relationship exists, the person entity is accepted.
/// </item>
/// <item>
/// If no relationship exists, the resolver falls back to validating
/// <see cref="PersonInput"/> to ensure the caller knows the correct identifier
/// and last name.
/// </item>
/// </list>
/// </item>
/// <item>
/// If <c>toPartyUuid</c> is not provided or cannot be accepted, the
/// resolver uses <see cref="PersonInput"/> to resolve the person via
/// <see cref="IUserProfileLookupService"/>.
/// </item>
/// </list>
///
/// This pattern allows both people and organizations to reference other people
/// securely, while preventing enumeration of person identifiers.
///
/// Person lookups are rate-limited. Repeated failed attempts will eventually
/// trigger a cooldown (for example, after three incorrect attempts).
/// </summary>
public sealed class ToUuidResolver(
    IEntityService entityService,
    IUserProfileLookupService userProfileLookupService,
    IHttpContextAccessor httpContextAccessor,
    ConnectionQuery connectionQuery)
{
    /// <summary>
    /// Resolves the target entity for an assignment.
    ///
    /// If <paramref name="toPartyUuid"/> is provided:
    /// <list type="bullet">
    /// <item>
    /// Non-person entities are resolved directly.
    /// </item>
    /// <item>
    /// Person entities are only accepted if a relationship exists with the
    /// requesting <paramref name="party"/>.
    /// </item>
    /// <item>
    /// If no relationship exists, <see cref="PersonInput"/> is required and
    /// validated to confirm the target person.
    /// </item>
    /// </list>
    ///
    /// If <paramref name="toPartyUuid"/> is not provided, resolution is performed
    /// exclusively using <see cref="PersonInput"/>.
    ///
    /// This ensures that callers cannot reference unrelated persons without
    /// supplying correct identifying information.
    /// </summary>
    /// <param name="person">
    /// Identifying information for a person (identifier and last name). Required
    /// when referencing a person without an existing relationship.
    /// </param>
    /// <param name="toPartyUuid">
    /// Optional UUID of the target party. May refer to a person or non-person entity.
    /// </param>
    /// <param name="party">
    /// UUID of the calling party initiating the assignment.
    /// </param>
    /// <param name="acceptAnyConnectionForPerson">
    /// Indicates whether any existing connection to a person entity is sufficient
    /// to accept <paramref name="toPartyUuid"/> without validating
    /// <see cref="PersonInput"/>.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{Entity}"/> containing the resolved entity, or a problem
    /// result if resolution fails.
    /// </returns>
    public async Task<Result<Entity>> Resolve(
        PersonInput person,
        Guid? toPartyUuid,
        Guid party,
        bool acceptAnyConnectionForPerson = true,
        CancellationToken cancellationToken = default)
    {
        if (toPartyUuid is { } partyuuid && partyuuid != Guid.Empty)
        {
            var toEntity = await entityService.GetEntity(partyuuid, cancellationToken);
            if (toEntity is null)
            {
                return Problems.PartyNotFound;
            }

            if (toEntity.Type.Id == EntityTypeConstants.Person)
            {
                if (acceptAnyConnectionForPerson)
                {
                    var connections = await connectionQuery.GetConnectionsFromOthersAsync(
                        new()
                        {
                            FromIds = [party],
                            ToIds = [partyuuid],
                            IncludeDelegation = false,
                            EnrichPackageResources = false,
                            EnrichEntities = false
                        },
                        ct: cancellationToken
                    );

                    if (connections.Count > 0)
                    {
                        return toEntity;
                    }
                }
            }
            else
            {
                return toEntity;
            }
        }

        return await ResolveWithPersonInputAsync(person, cancellationToken);
    }

    /// <summary>
    /// Resolves a person entity using <see cref="PersonInput"/>.
    ///
    /// This method validates the provided identifier and last name via
    /// <see cref="IUserProfileLookupService"/>. It is used when:
    /// <list type="bullet">
    /// <item>
    /// No <c>toPartyUuid</c> is supplied.
    /// </item>
    /// <item>
    /// A person UUID is supplied but no relationship exists with the caller.
    /// </item>
    /// </list>
    ///
    /// Failed lookups are tracked and may contribute to rate limiting and
    /// temporary cooldowns.
    /// </summary>
    /// <param name="person">Person identifying input.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{Entity}"/> containing the resolved person entity,
    /// or a problem result if validation fails.
    /// </returns>
    internal async Task<Result<Entity>> ResolveWithPersonInputAsync(
        PersonInput person,
        CancellationToken cancellationToken = default)
    {
        var result = await LookupPerson(person, cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem;
        }

        if (result.Value is { } uuid)
        {
            return await entityService.GetEntity(uuid, cancellationToken);
        }

        throw new UnreachableException();
    }

    /// <summary>
    /// Performs a guarded lookup of a person profile based on identifier and last name.
    ///
    /// The identifier may be either a national identifier (SSN) or a username.
    /// Failed lookups are monitored, and repeated failures may result in a
    /// temporary cooldown to prevent brute-force or enumeration attacks.
    /// </summary>
    /// <param name="person">Person identifying input.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{Guid}"/> containing the resolved user or party UUID,
    /// or validation errors if the lookup fails.
    /// </returns>
    private async Task<Result<Guid?>> LookupPerson(
        PersonInput person,
        CancellationToken cancellationToken)
    {
        int authUserId = AuthenticationHelper.GetUserId(httpContextAccessor.HttpContext);
        ValidationErrorBuilder errorBuilder = default;

        string identifier = person.PersonIdentifier;
        string lastName = person.LastName;

        bool treatAsSsn = identifier.Length == 11 && identifier.All(char.IsDigit);

        UserProfileLookup lookup = new();
        if (treatAsSsn)
        {
            lookup.Ssn = identifier;
        }
        else
        {
            lookup.Username = identifier;
        }

        try
        {
            var profile = await userProfileLookupService.GetUserProfile(
                authUserId,
                lookup,
                lastName,
                cancellationToken);

            if (profile is null)
            {
                errorBuilder.Add(
                    ValidationErrors.InvalidQueryParameter,
                    ["/personIdentifier", "/lastName"],
                    [new("personInput", ValidationErrorMessageTexts.PersonIdentifierLastNameInvalid)]);
            }
            else
            {
                return profile.UserUuid != Guid.Empty
                    ? profile.UserUuid
                    : profile.Party?.PartyUuid;
            }
        }
        catch (TooManyFailedLookupsException)
        {
            return Problems.PersonLookupFailedToManyErrors;
        }

        errorBuilder.TryBuild(out var errors);
        return errors;
    }
}
