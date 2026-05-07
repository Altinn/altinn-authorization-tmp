using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection.Models;

namespace Altinn.AccessMgmt.Core.Services;

/// <summary>
/// Implementation of <see cref="IAuthorizationContextService"/> that resolves authorization context
/// using local Entity Framework database lookups via <see cref="IEntityService"/> and <see cref="ConnectionQuery"/>.
/// </summary>
public class AuthorizationContextService : IAuthorizationContextService
{
    private readonly IEntityService _entityService;
    private readonly ConnectionQuery _connectionQuery;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationContextService"/> class.
    /// </summary>
    public AuthorizationContextService(IEntityService entityService, ConnectionQuery connectionQuery)
    {
        _entityService = entityService;
        _connectionQuery = connectionQuery;
    }

    /// <inheritdoc/>
    public async Task<Entity> ResolveEntityByOrgNo(string orgNo, CancellationToken cancellationToken = default)
    {
        return await _entityService.GetByOrgNo(orgNo, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Entity> ResolveEntityByPersonId(string personId, CancellationToken cancellationToken = default)
    {
        return await _entityService.GetByPersNo(personId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Entity> ResolveEntityByPartyId(int partyId, CancellationToken cancellationToken = default)
    {
        return await _entityService.GetByPartyId(partyId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Entity> ResolveEntityByUserId(int userId, CancellationToken cancellationToken = default)
    {
        return await _entityService.GetByUserId(userId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Entity> ResolveEntityByUuid(Guid partyUuid, CancellationToken cancellationToken = default)
    {
        return await _entityService.GetEntity(partyUuid, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<ConnectionQueryExtendedRecord>> GetConnections(
        Guid resourcePartyUuid,
        IReadOnlyCollection<Guid> subjectPartyUuids,
        CancellationToken cancellationToken = default)
    {
        var queryFilter = new ConnectionQueryFilter
        {
            FromIds = [resourcePartyUuid],
            ToIds = subjectPartyUuids,
            IncludePackages = true,
            IncludeResources = true,
            IncludeInstances = true,
            IncludeDelegation = true,
            IncludeKeyRole = true,
            EnrichEntities = false,
            ExcludeDeleted = true,
        };

        return await _connectionQuery.GetConnectionsFromOthersAsync(queryFilter, ct: cancellationToken);
    }
}
