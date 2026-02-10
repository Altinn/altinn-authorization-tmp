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
/// Resolves the target (to) UUID for AddAssignment based on either:
/// - Provided 'to' UUID referencing a non-person entity resolved via <see cref="IEntityService"/>
/// - PersonInput (identifier + last name) resolved via <see cref="IUserProfileLookupService"/>
/// Internal implementation detail of the Enduser API.
/// </summary>
public sealed class ToUuidResolver(
    IEntityService entityService,
    IUserProfileLookupService userProfileLookupService,
    IHttpContextAccessor httpContextAccessor,
    ConnectionQuery connectionQuery)
{
    public async Task<Result<Entity>> Resolve(PersonInput person, Guid? to, Guid party, bool acceptAnyConnectionForPerson = true, CancellationToken cancellationToken = default)
    {
        if (to is { } partyuuid && partyuuid != Guid.Empty)
        {
            var entity = await entityService.GetEntity(partyuuid, cancellationToken);
            if (entity is null)
            {
                return Problems.PartyNotFound;
            }

            if (entity.Type.Id == EntityTypeConstants.Person)
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
                        return entity;
                    }
                }
            }
            else
            {
                return entity;
            }
        }

        return await ResolveWithPersonInputAsync(person, cancellationToken);
    }

    /// <summary>
    /// Resolve target UUID for a person entity from <see cref="PersonInput"/> via <see cref="UserProfileLookupService"/>.
    /// </summary>
    internal async Task<Result<Entity>> ResolveWithPersonInputAsync(PersonInput person, CancellationToken cancellationToken = default)
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

    private async Task<Result<Guid?>> LookupPerson(PersonInput person, CancellationToken cancellationToken)
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
            var profile = await userProfileLookupService.GetUserProfile(authUserId, lookup, lastName, cancellationToken);
            if (profile is null)
            {
                errorBuilder.Add(ValidationErrors.InvalidQueryParameter, ["/personIdentifier", "/lastName"], [new("personInput", ValidationErrorMessageTexts.PersonIdentifierLastNameInvalid)]);
            }
            else
            {
                return profile.UserUuid != Guid.Empty ? profile.UserUuid : profile.Party?.PartyUuid;
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
