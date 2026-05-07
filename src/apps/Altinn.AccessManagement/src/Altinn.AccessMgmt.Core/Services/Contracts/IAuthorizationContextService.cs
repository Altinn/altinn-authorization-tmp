using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection.Models;

namespace Altinn.AccessMgmt.Core.Services.Contracts;

/// <summary>
/// Service responsible for resolving authorization context information
/// by looking up entities, roles, access packages, and delegations from the local database.
/// Replaces the old ContextHandler / DelegationContextHandler that relied on external API calls.
/// </summary>
public interface IAuthorizationContextService
{
    /// <summary>
    /// Resolves an entity from an organization number.
    /// </summary>
    Task<Entity> ResolveEntityByOrgNo(string orgNo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves an entity from a person identifier (SSN).
    /// </summary>
    Task<Entity> ResolveEntityByPersonId(string personId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves an entity from a party ID.
    /// </summary>
    Task<Entity> ResolveEntityByPartyId(int partyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves an entity from a user ID.
    /// </summary>
    Task<Entity> ResolveEntityByUserId(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves an entity from a party UUID.
    /// </summary>
    Task<Entity> ResolveEntityByUuid(Guid partyUuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all connections (roles, access packages, delegations) from a resource party to a subject party.
    /// The resource party is the "From" (grantor) and the subject party is the "To" (recipient).
    /// Includes roles, packages, resources, and instances in a single query.
    /// </summary>
    /// <param name="resourcePartyUuid">The resource party UUID (From/grantor).</param>
    /// <param name="subjectPartyUuids">The subject party UUID(s) (To/recipient), including keyrole party UUIDs.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>List of connection records with roles, packages, resources, and instances.</returns>
    Task<List<ConnectionQueryExtendedRecord>> GetConnections(
        Guid resourcePartyUuid,
        IReadOnlyCollection<Guid> subjectPartyUuids,
        CancellationToken cancellationToken = default);
}
