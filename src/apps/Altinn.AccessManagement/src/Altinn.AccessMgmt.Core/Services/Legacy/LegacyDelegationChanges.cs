using System.Linq.Expressions;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Legacy;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services.Legacy;

/// <summary>
/// HUSK Å SKRIVE OM DE SOM BRUKER DETTE! Byttet rekkefølge på metodene...
/// </summary>
public static class EfLatestPerGroupExtensions
{
    /// <summary>
    /// Latest per group ved å bruke EF.Property på id-kolonnen
    /// </summary>
    public static IQueryable<T> LatestPerGroup<T, TKey, TId>(
        this IQueryable<T> source,
        Expression<Func<T, TId>> idSelector,
        Expression<Func<IGrouping<TKey, T>, TId>> maxIdSelector,
        Expression<Func<T, TKey>> groupBy
        )
        where T : class
    {
        var latestIds = source.GroupBy(groupBy).Select(maxIdSelector);
        return source.Join(latestIds, idSelector, id => id, (row, _) => row);
    }

    /// <summary>
    /// Latest per group ved å bruke EF.Property på id-kolonnen
    /// </summary>
    public static IQueryable<T> LatestPerGroup<T, TKey, TId>(
        this IQueryable<T> source,
        Expression<Func<T, TId>> idSelector,
        Expression<Func<T, TKey>> groupBy
        )
        where T : class
    {
        var groupParameters = Expression.Parameter(typeof(IGrouping<TKey, T>), "g");
        var xParam = Expression.Parameter(typeof(T), "x");

        var idBodyForX = new ReplaceParameterVisitor(idSelector.Parameters[0], xParam).Visit(idSelector.Body)!;

        var maxCall = Expression.Call(
            typeof(Enumerable),
            nameof(Enumerable.Max),
            [typeof(T), typeof(TId)],
            groupParameters,
            Expression.Lambda<Func<T, TId>>(idBodyForX, xParam));

        var maxIdSelector = Expression.Lambda<Func<IGrouping<TKey, T>, TId>>(maxCall, groupParameters);

        var latestIds = source.GroupBy(groupBy).Select(maxIdSelector);

        return source.Join(latestIds, idSelector, id => id, (row, _) => row);
    }

    private sealed class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly ParameterExpression _to;

        public ReplaceParameterVisitor(ParameterExpression from, ParameterExpression to) => (_from, _to) = (from, to);

        protected override Expression VisitParameter(ParameterExpression node) => node == _from ? _to : base.VisitParameter(node);
    }
}

public class LegacyDelegationChanges(AppDbContext dbContext) : IDelegationChangesService
{
    public async Task<DelegationChanges> InsertDelegation(DelegationChanges delegationChange, CancellationToken cancellationToken = default)
    {
        // Split to avoid ResourceAttributeMatchType
        return await InsertAppDelegation(delegationChange, cancellationToken);
    }
    public async Task<DelegationChanges> InsertDelegationAltinnApp(DelegationChanges delegationChange, CancellationToken cancellationToken = default)
    {
        // Split to avoid ResourceAttributeMatchType
        return await InsertResourceRegistryDelegation(delegationChange, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InstanceDelegationChanges>> GetActiveInstanceDelegations(List<string> resourceIds, Guid from, List<Guid> to, CancellationToken cancellationToken = default)
    {
        var baseQuery = dbContext.LegacyInstanceDelegationChanges
        .AsNoTracking()
        .Where(dc => resourceIds.Contains(dc.ResourceId))
        .Where(dc => dc.FromUuid == from)
        .Where(dc => dc.ToUuid.HasValue && to.Contains(dc.ToUuid.Value));

        return await baseQuery
           .LatestPerGroup(
               t => new
               {
                   t.InstanceDelegationMode,
                   t.ResourceId,
                   t.InstanceId,
                   t.FromUuid,
                   t.FromType,
                   t.ToUuid,
                   t.ToType
               },
               t => t.InstanceDelegationChangeId)
           .Where(t => t.DelegationChangeType != "revoke_last")
           .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChanges>> GetAllAppDelegationChanges(string altinnAppId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, CancellationToken cancellationToken = default)
    {
        // DONE
        return await dbContext.LegacyDelegationChanges.AsNoTracking()
           .Where(t => t.AltinnAppId == altinnAppId)
           .Where(t => t.OfferedByPartyId == offeredByPartyId)
           .WhereIf(coveredByPartyId != null, t => t.CoveredByPartyId == coveredByPartyId)
           .WhereIf(coveredByUserId != null, t => t.CoveredByUserId == coveredByUserId)
           .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChanges>> GetAllCurrentAppDelegationChanges(List<int> offeredByPartyIds, List<string> altinnAppIds, List<int> coveredByPartyIds = null, List<int> coveredByUserIds = null, CancellationToken cancellationToken = default)
    {
        var baseQuery = dbContext.LegacyDelegationChanges.AsNoTracking()
            .Where(t => offeredByPartyIds.Contains(t.OfferedByPartyId) && altinnAppIds.Contains(t.AltinnAppId))
            .WhereIf(coveredByPartyIds is { Count: > 0 }, t => t.CoveredByPartyId.HasValue && coveredByPartyIds!.Contains((int)t.CoveredByPartyId))
            .WhereIf(coveredByUserIds is { Count: > 0 }, t => t.CoveredByUserId.HasValue && coveredByUserIds!.Contains((int)t.CoveredByUserId));

        return await baseQuery
            .LatestPerGroup(
            t => t.DelegationChangeId,
            t => new
                { 
                    t.AltinnAppId, 
                    t.OfferedByPartyId, 
                    t.CoveredByPartyId, 
                    t.CoveredByUserId 
                }
            )
            .Where(t => t.DelegationChangeType != "revoke_last")
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<DelegationChanges>> GetAllCurrentAppDelegationChanges(List<string> altinnAppIds, List<int> fromPartyIds, string toUuidType, Guid toUuid, CancellationToken cancellationToken = default)
    {
        // DONE
        var baseQuery = dbContext.LegacyDelegationChanges.AsNoTracking()
            .Where(t => altinnAppIds.Contains(t.AltinnAppId))
            .Where(t => fromPartyIds.Contains(t.OfferedByPartyId))
            .Where(t => t.ToUuidType == toUuidType)
            .Where(t => t.ToUuid == toUuid);

        return await baseQuery
            .LatestPerGroup(
                t => new { t.AltinnAppId, t.OfferedByPartyId, t.ToUuidType, t.ToUuid },
                t => t.DelegationChangeId)
            .Where(t => t.DelegationChangeType != "revoke_last")
            .ToListAsync(cancellationToken);
    }

    public async Task<List<InstanceDelegationChanges>> GetAllCurrentReceivedInstanceDelegations(List<Guid> toUuid, CancellationToken cancellationToken = default)
    {
        var baseQuery = dbContext.LegacyInstanceDelegationChanges.AsNoTracking()
            .Where(t => toUuid.Contains(t.ToUuid.Value));

        return await baseQuery
            .LatestPerGroup(
                t => new 
                {
                    t.ToUuid,
                    t.FromUuid,
                    t.ResourceId,
                    t.InstanceId
                },
                t => t.InstanceDelegationChangeId)
            .Where(t => t.DelegationChangeType != "revoke_last")
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DelegationChanges>> GetAllCurrentResourceRegistryDelegationChanges(List<int> offeredByPartyIds, List<string> resourceRegistryIds, List<int> coveredByPartyIds = null, int? coveredByUserId = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<List<ResourceRegistryDelegationChanges>> GetAllCurrentResourceRegistryDelegationChanges(List<string> resourceRegistryIds, List<int> fromPartyIds, string toUuidType, Guid toUuid, CancellationToken cancellationToken = default)
    {
        var baseQuery = dbContext.LegacyResourceRegistryDelegationChanges.AsNoTracking()
           .Where(t => t.Resource.Type.Name != "maskinportenschema")
           .WhereIf(resourceRegistryIds is { Count: > 0 }, t => resourceRegistryIds!.Contains(t.ResourceId));

        return await baseQuery
            .LatestPerGroup(
                t => t.ResourceRegistryDelegationChangeId,
                t => new
                {
                    t.ResourceId,
                    t.OfferedByPartyId,
                    t.ToUuidType,
                    t.ToUuid
                }
                )
            .Where(t => t.DelegationChangeType != "revoke_last")
            .ToListAsync(cancellationToken);


    }
    public async Task<List<DelegationChanges>> GetAllDelegationChangesForAuthorizedParties(List<int> coveredByUserIds, List<int> coveredByPartyIds, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public async Task<List<DelegationChanges>> GetAllDelegationChangesForAuthorizedParties(List<Guid> toPartyUuids, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public async Task<List<InstanceDelegationChanges>> GetAllLatestInstanceDelegationChanges(InstanceDelegationSource source, string resourceID, string instanceID, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task<ResourceRegistryDelegationChanges> GetCurrentDelegationChange(string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, Guid? toUuid, string toUuidType, CancellationToken cancellationToken = default)
    {
        // DONE
        // GetCurrentResourceRegistryDelegation
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            throw new ArgumentException($"Param: {nameof(resourceId)} cannot be null or whitespace.");
        }

        if (offeredByPartyId == 0)
        {
            throw new ArgumentException($"Param: {nameof(offeredByPartyId)} cannot be zero.");
        }

        if (coveredByPartyId == null && coveredByUserId == null && toUuidType != "SystemUser")
        {
            throw new ArgumentException($"All params: {nameof(coveredByUserId)}, {nameof(coveredByPartyId)}, {nameof(toUuid)} cannot be null.");
        }

        return await dbContext.LegacyResourceRegistryDelegationChanges.AsNoTracking()
            .Where(t => t.ResourceId == resourceId)
            .Where(t => t.OfferedByPartyId == offeredByPartyId)
            .WhereIf(coveredByPartyId != null, t => t.CoveredByPartyId == coveredByPartyId)
            .WhereIf(coveredByUserId != null, t => t.CoveredByUserId == coveredByUserId)
            .WhereIf(toUuidType == "SystemUser", t => t.ToUuid == toUuid)
            .WhereIf(toUuidType == "SystemUser", t => t.ToUuidType == toUuidType)
            .OrderByDescending(t => t.ResourceRegistryDelegationChangeId)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    /// <inheritdoc/>
    public async Task<DelegationChanges> GetCurrentDelegationChangeAltinnApp(string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, Guid? toUuid, string toUuidType, CancellationToken cancellationToken = default)
    {
        // DONE
        // GetCurrentAppDelegation
        if (string.IsNullOrWhiteSpace(resourceId))
        {
            throw new ArgumentException($"Param: {nameof(resourceId)} cannot be null or whitespace.");
        }

        if (offeredByPartyId == 0)
        {
            throw new ArgumentException($"Param: {nameof(offeredByPartyId)} cannot be zero.");
        }

        if (coveredByPartyId == null && coveredByUserId == null && toUuidType != "SystemUser")
        {
            throw new ArgumentException($"All params: {nameof(coveredByUserId)}, {nameof(coveredByPartyId)}, {nameof(toUuid)} cannot be null.");
        }

        return await dbContext.LegacyDelegationChanges.AsNoTracking()
            .Where(t => t.AltinnAppId == resourceId)
            .Where(t => t.OfferedByPartyId == offeredByPartyId)
            .WhereIf(coveredByPartyId != null, t => t.CoveredByPartyId == coveredByPartyId)
            .WhereIf(coveredByUserId != null, t => t.CoveredByUserId == coveredByUserId)
            .WhereIf(toUuidType == "SystemUser", t => t.ToUuid == toUuid)
            .WhereIf(toUuidType == "SystemUser", t => t.ToUuidType == toUuidType)
            .OrderByDescending(t => t.DelegationChangeId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<InstanceDelegationChanges> GetLastInstanceDelegationChange(InstanceDelegationChanges request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public async Task<List<DelegationChanges>> GetOfferedDelegations(List<int> offeredByPartyIds, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public async Task<List<DelegationChanges>> GetOfferedResourceRegistryDelegations(int offeredByPartyId, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public async Task<List<DelegationChanges>> GetReceivedResourceRegistryDelegationsForCoveredByPartys(List<int> coveredByPartyIds, List<int> offeredByPartyIds = null, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public async Task<List<DelegationChanges>> GetReceivedResourceRegistryDelegationsForCoveredByUser(int coveredByUserId, List<int> offeredByPartyIds, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public async Task<List<DelegationChanges>> GetResourceRegistryDelegationChanges(List<string> resourceIds, int offeredByPartyId, int coveredByPartyId, ResourceType resourceType, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public async Task<InstanceDelegationChanges> InsertInstanceDelegation(InstanceDelegationChanges instanceDelegationChange, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
    public async Task<bool> InsertMultipleInstanceDelegations(List<PolicyWriteOutput> policyWriteOutputs, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
