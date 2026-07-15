using System.Text.Json;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection.Models;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Queries.Connection;

/// <summary>
/// Full connection query with entity enrichment, resource and instance loading.
/// Extends <see cref="ConnectionQueryCore"/> which provides the base query building.
/// </summary>
public class ConnectionQuery : ConnectionQueryCore
{
    public ConnectionQuery(AppDbContext db)
        : base(db)
    {
    }

    public async Task<List<ConnectionQueryExtendedRecord>> GetConnectionsFromOthersAsync(ConnectionQueryFilter filter, CancellationToken ct = default)
    {
        return await GetConnectionsAsync(filter, ConnectionQueryDirection.FromOthers, ct);
    }

    public async Task<List<ConnectionQueryExtendedRecord>> GetConnectionsToOthersAsync(ConnectionQueryFilter filter, CancellationToken ct = default)
    {
        return await GetConnectionsAsync(filter, ConnectionQueryDirection.ToOthers, ct);
    }

    /// <summary>
    /// Returns connections between entities based on assignments and delegations, with full enrichment.
    /// </summary>
    public async Task<List<ConnectionQueryExtendedRecord>> GetConnectionsAsync(ConnectionQueryFilter filter, ConnectionQueryDirection direction, CancellationToken ct = default)
    {
        try
        {
            bool delayChildNesting = true;
            bool delayFromFilter = true;
            if (direction == ConnectionQueryDirection.ToOthers || (filter.FromIds?.Count > 0 && filter.FromIds?.Count <= 20))
            {
                delayChildNesting = false;
                delayFromFilter = false;
            }

            var baseQuery = direction == ConnectionQueryDirection.FromOthers
                ? BuildBaseQueryFromOthers(
                        filter,
                        filter.IncludeSubConnections && !delayChildNesting,
                        !filter.IncludeSubConnections || !delayFromFilter)
                : BuildBaseQueryToOthers(filter);

            var result = baseQuery.Select(ToDtoEmpty).ToList();
            if (filter.IncludePackages || filter.EnrichPackageResources)
            {
                try
                {
                    var pkgs = await LoadPackagesByKeyAsync(result, filter, ct);
                    if (filter.EnrichPackageResources)
                    {
                        await EnrichPackageResourcesAsync(pkgs, filter, ct);
                    }

                    result = Attach(result, pkgs, p => p.Id, (dto, list) => dto.Packages = list);

                    // Remove connections where no packages were found if filtering on specific packages
                    if (filter.PackageIds != null)
                    {
                        result.RemoveAll(t => t.Packages.Count == 0);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to include packages", ex);
                }
            }

            try
            {
                if (filter.IncludeResources)
                {
                    result = await LoadResourcesByKeyAsync(result, filter, ct);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to include resources", ex);
            }

            try
            {
                if (filter.IncludeInstances)
                {
                    result = await LoadInstancesByKeyAsync(result, filter, ct);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to include instances", ex);
            }

            if (filter.EnrichEntities || filter.ExcludeDeleted)
            {
                result = await EnrichEntities(
                    result,
                    filter.ExcludeDeleted,
                    direction,
                    filter,
                    filter.IncludeSubConnections && delayChildNesting,
                    filter.IncludeSubConnections && delayFromFilter,
                    ct);
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get connections with filter: {JsonSerializer.Serialize(filter)}", ex);
        }
    }

    private async Task<List<ConnectionQueryExtendedRecord>> EnrichEntities(List<ConnectionQueryExtendedRecord> allKeys, bool excludeDeleted, ConnectionQueryDirection direction, ConnectionQueryFilter filter, bool doChildNesting, bool applyFromFilter, CancellationToken ct)
    {
        SortedSet<Guid> parties = [];
        foreach (var item in allKeys)
        {
            if (!parties.Contains(item.FromId))
            {
                parties.Add(item.FromId);
            }

            if (!parties.Contains(item.ToId))
            {
                parties.Add(item.ToId);
            }

            if (item.ViaId != null && !parties.Contains((Guid)item.ViaId))
            {
                parties.Add((Guid)item.ViaId);
            }
        }

        SortedList<Guid, Entity> entityDict = [];
        var entitites = await Db
            .Entities
            .AsNoTracking()
            .Where(e => parties.Contains(e.Id))
            .Include(t => t.Parent)
            .Select(e => new Entity()
            {
                Id = e.Id,
                Name = e.Name,
                OrganizationIdentifier = e.OrganizationIdentifier,
                ParentId = e.ParentId,
                PersonIdentifier = e.PersonIdentifier,
                DateOfBirth = e.DateOfBirth,
                DateOfDeath = e.DateOfDeath,
                PartyId = e.PartyId,
                IsDeleted = e.IsDeleted,
                DeletedAt = e.DeletedAt,
                UserId = e.UserId,
                Username = e.Username,
                TypeId = e.TypeId,
                VariantId = e.VariantId,
                EmailIdentifier = e.EmailIdentifier,
                Parent = e.Parent != null ? new Entity()
                {
                    Id = e.Parent.Id,
                    Name = e.Parent.Name,
                    OrganizationIdentifier = e.Parent.OrganizationIdentifier,
                    ParentId = e.Parent.ParentId,
                    PersonIdentifier = e.Parent.PersonIdentifier,
                    DateOfBirth = e.Parent.DateOfBirth,
                    DateOfDeath = e.Parent.DateOfDeath,
                    PartyId = e.Parent.PartyId,
                    IsDeleted = e.Parent.IsDeleted,
                    DeletedAt = e.Parent.DeletedAt,
                    UserId = e.Parent.UserId,
                    Username = e.Parent.Username,
                    TypeId = e.Parent.TypeId,
                    VariantId = e.Parent.VariantId,
                    EmailIdentifier = e.Parent.EmailIdentifier
                }
                : null
            })
            .Distinct()
            .AsNoTracking()
            .ToListAsync(ct);

        foreach (var entity in entitites)
        {
            entityDict.Add(entity.Id, entity);
        }

        SortedList<Guid, List<Entity>> childrenDict = [];
        if (doChildNesting)
        {
            var allChildren = await Db
                .Entities
                .AsNoTracking()
                .Where(e => e.ParentId != null && entityDict.Keys.Contains((Guid)e.ParentId))
                .Select(e => new Entity()
                {
                    Id = e.Id,
                    Name = e.Name,
                    OrganizationIdentifier = e.OrganizationIdentifier,
                    ParentId = e.ParentId,
                    Parent = entityDict[(Guid)e.ParentId],
                    PersonIdentifier = e.PersonIdentifier,
                    DateOfBirth = e.DateOfBirth,
                    DateOfDeath = e.DateOfDeath,
                    PartyId = e.PartyId,
                    IsDeleted = e.IsDeleted,
                    DeletedAt = e.DeletedAt,
                    UserId = e.UserId,
                    Username = e.Username,
                    TypeId = e.TypeId,
                    VariantId = e.VariantId
                })
                .Distinct()
                .AsNoTracking()
                .ToListAsync(ct);

            if (applyFromFilter && filter.FromIds != null && filter.FromIds.Count > 0)
            {
                allChildren = allChildren.Where(c => filter.FromIds.Contains(c.Id)).ToList();
            }

            foreach (var child in allChildren)
            {
                if (!childrenDict.ContainsKey((Guid)child.ParentId))
                {
                    childrenDict.Add((Guid)child.ParentId, [child]);
                }
                else
                {
                    childrenDict[(Guid)child.ParentId].Add(child);
                }
            }
        }

        // Could be cached
        var roles = await Db.Roles.Include(r => r.Provider).ThenInclude(p => p.Type).AsNoTracking().ToListAsync(ct);
        SortedList<string, Role> sortedRoles = [];
        foreach (var role in roles)
        {
            sortedRoles.Add(role.Id.ToString(), role);
        }

        List<ConnectionQueryExtendedRecord> keysWithChildren = [];
        foreach (var c in allKeys)
        {
            if (excludeDeleted && ((direction == ConnectionQueryDirection.FromOthers && c.From.IsDeleted) || (direction == ConnectionQueryDirection.ToOthers && c.To.IsDeleted)))
            {
                continue;
            }

            c.From = entityDict[c.FromId];
            c.To = entityDict[c.ToId];
            c.Via = c.ViaId != null ? entityDict[(Guid)c.ViaId] : null;
            c.Role = c.RoleId != Guid.Empty ? sortedRoles[c.RoleId.ToString()] : null;
            c.ViaRole = c.ViaRoleId != null && c?.ViaRoleId != Guid.Empty ? sortedRoles[c.ViaRoleId.ToString()] : null;
            keysWithChildren.Add(c);

            if (doChildNesting && c.Reason != ConnectionReason.Hierarchy && childrenDict.TryGetValue(c.From.Id, out List<Entity> childrenForKey))
            {
                foreach (var child in childrenForKey)
                {
                    keysWithChildren.Add(new()
                    {
                        AssignmentId = c.AssignmentId,
                        FromId = child.Id,
                        From = child,
                        To = c.To,
                        ToId = c.ToId,
                        ViaId = c.FromId,
                        Via = c.From,
                        RoleId = c.RoleId,
                        Role = c.Role,
                        ViaRoleId = c.ViaRoleId,
                        ViaRole = c.ViaRole,

                        DelegationId = c.DelegationId,
                        IsKeyRoleAccess = c.IsKeyRoleAccess,
                        IsMainUnitAccess = true,
                        IsRoleMap = c.IsRoleMap,
                        Reason = ConnectionReason.Hierarchy,

                        Packages = c.Packages,
                        Resources = c.Resources
                    });
                }
            }
        }

        if (applyFromFilter && filter.FromIds != null && filter.FromIds.Count > 0)
        {
            keysWithChildren = keysWithChildren.Where(c => filter.FromIds.Contains(c.FromId)).ToList();
        }

        return keysWithChildren;
    }

    private async Task<List<ConnectionQueryExtendedRecord>> LoadResourcesByKeyAsync(List<ConnectionQueryExtendedRecord> allKeys, ConnectionQueryFilter filter, CancellationToken ct)
    {
        var resourceSet = filter.ResourceIds?.Count > 0 ? new HashSet<Guid>(filter.ResourceIds) : null;

        var rightholderAssignments = GetRightholderAssignments(allKeys, filter);
        var rightholderAssignmentIds = rightholderAssignments.Select(a => (Guid)a.AssignmentId).Distinct().ToList();
        if (rightholderAssignmentIds.Count == 0)
        {
            return allKeys;
        }

        var assignmentResources = await Db.AssignmentResources
            .Where(ai => rightholderAssignmentIds.Contains(ai.AssignmentId))
            .Select(ai => new { ai.AssignmentId, ai.Id, ai.ResourceId })
            .WhereIf(resourceSet is not null, x => resourceSet!.Contains(x.ResourceId))
            .Join(Db.Resources, x => x.ResourceId, r => r.Id, (x, r) => new
            {
                x.AssignmentId,
                r.Id,
                r.Name,
                r.RefId
            })
            .AsNoTracking()
            .ToListAsync(ct);

        SortedList<Guid, List<ConnectionQueryResource>> resourcesByAssignment = [];
        foreach (var ai in assignmentResources)
        {
            if (resourcesByAssignment.TryGetValue(ai.AssignmentId, out var list))
            {
                list.Add(new ConnectionQueryResource()
                {
                    Id = ai.Id,
                    Name = ai.Name,
                    RefId = ai.RefId
                });
            }
            else
            {
                resourcesByAssignment[ai.AssignmentId] =
                [
                    new ConnectionQueryResource()
                    {
                        Id = ai.Id,
                        Name = ai.Name,
                        RefId = ai.RefId
                    }
                ];
            }
        }

        foreach (var key in allKeys)
        {
            if (key.AssignmentId.HasValue && resourcesByAssignment.TryGetValue((Guid)key.AssignmentId!, out var list))
            {
                key.Resources = list;
            }
        }

        return allKeys;
    }

    private async Task<List<ConnectionQueryExtendedRecord>> LoadInstancesByKeyAsync(List<ConnectionQueryExtendedRecord> allKeys, ConnectionQueryFilter filter, CancellationToken ct)
    {
        var instanceSet = filter.InstanceIds?.Count > 0 ? new HashSet<string>(filter.InstanceIds) : null;
        var resourceSet = filter.ResourceIds?.Count > 0 ? new HashSet<Guid>(filter.ResourceIds) : null;

        // Assignment → AssignmentInstance
        var rightholderAssignments = GetRightholderAssignments(allKeys, filter);
        var rightholderAssignmentIds = rightholderAssignments.Where(a => a.Reason != ConnectionReason.Hierarchy && !a.IsMainUnitAccess).Select(a => (Guid)a.AssignmentId).Distinct().ToList();
        if (rightholderAssignmentIds.Count == 0)
        {
            return allKeys;
        }

        var assignmentInstances = await Db.AssignmentInstances
            .Where(ai => rightholderAssignmentIds.Contains(ai.AssignmentId))
            .Select(ai => new { ai.AssignmentId, ai.Id, ai.ResourceId, ai.InstanceId })
            .WhereIf(instanceSet is not null, x => instanceSet!.Contains(x.InstanceId))
            .WhereIf(resourceSet is not null, x => resourceSet!.Contains(x.ResourceId))
            .Join(Db.Resources, x => x.ResourceId, r => r.Id, (x, r) => new
            {
                x.AssignmentId,
                x.Id,
                x.ResourceId,
                x.InstanceId,
                ResourceName = r.Name,
                ResourceRefId = r.RefId
            })
            .AsNoTracking()
            .ToListAsync(ct);

        SortedList<Guid, List<ConnectionQueryInstance>> instancesByAssignment = [];
        foreach (var ai in assignmentInstances)
        {
            if (instancesByAssignment.TryGetValue(ai.AssignmentId, out var list))
            {
                list.Add(new ConnectionQueryInstance()
                {
                    Id = ai.Id,
                    ResourceId = ai.ResourceId,
                    InstanceId = ai.InstanceId,
                    ResourceName = ai.ResourceName,
                    ResourceRefId = ai.ResourceRefId
                });
            }
            else
            {
                instancesByAssignment[ai.AssignmentId] =
                [
                    new ConnectionQueryInstance()
                    {
                        Id = ai.Id,
                        ResourceId = ai.ResourceId,
                        InstanceId = ai.InstanceId,
                        ResourceName = ai.ResourceName,
                        ResourceRefId = ai.ResourceRefId
                    }
                ];
            }
        }

        foreach (var key in allKeys.Where(k => k.AssignmentId.HasValue && rightholderAssignmentIds.Contains((Guid)k.AssignmentId) && k.Reason != ConnectionReason.Hierarchy && !k.IsMainUnitAccess))
        {
            if (key.AssignmentId.HasValue && instancesByAssignment.TryGetValue((Guid)key.AssignmentId!, out var list))
            {
                key.Instances = list;
            }
        }

        return allKeys;
    }

    private static List<ConnectionQueryExtendedRecord> GetRightholderAssignments(List<ConnectionQueryExtendedRecord> allKeys, ConnectionQueryFilter filter)
    {
        List<Guid> rightholderRoles = [RoleConstants.Rightholder.Id];
        if (filter.IncludeAppControlledInstances)
        {
            rightholderRoles.Add(RoleConstants.AppControlledRightholder.Id);
        }

        return allKeys.Where(a => a.AssignmentId.HasValue && rightholderRoles.Contains(a.RoleId)).ToList();
    }

    private static List<ConnectionQueryExtendedRecord> Attach<T>(IEnumerable<ConnectionQueryExtendedRecord> results, ConnectionIndex<T> index, Func<T, Guid> idSelector, Action<ConnectionQueryExtendedRecord, List<T>> assign)
    {
        foreach (var dto in results)
        {
            var vals = index.Get(dto.CompositeKey()).DistinctBy(idSelector).ToList();
            assign(dto, vals);
        }

        return results is List<ConnectionQueryExtendedRecord> list ? list : results.ToList();
    }

    private static ConnectionQueryExtendedRecord ToDtoEmpty(ConnectionQueryBaseRecord x) => new()
    {
        FromId = x.FromId,
        ToId = x.ToId,
        RoleId = x.RoleId,
        AssignmentId = x.AssignmentId,
        DelegationId = x.DelegationId,
        ViaId = x.ViaId,
        ViaRoleId = x.ViaRoleId,
        Reason = x.Reason,
        IsKeyRoleAccess = x.IsKeyRoleAccess,
        IsMainUnitAccess = x.IsMainUnitAccess,
        IsRoleMap = x.IsRoleMap
    };
}

public sealed class ConnectionIndex<T>
{
    private readonly Dictionary<ConnectionCompositeKey, List<T>> map = new();

    public void Add(ConnectionCompositeKey key, T item)
    {
        if (!map.TryGetValue(key, out var list))
        {
            map[key] = list = new List<T>(4);
        }

        list.Add(item);
    }

    public void AddRange(ConnectionCompositeKey key, IEnumerable<T> items)
    {
        if (!map.TryGetValue(key, out var list))
        {
            map[key] = list = new List<T>();
        }

        list.AddRange(items);
    }

    public IReadOnlyList<T> Get(ConnectionCompositeKey key) =>
        map.TryGetValue(key, out var list) ? list : Array.Empty<T>();

    public IEnumerable<KeyValuePair<ConnectionCompositeKey, List<T>>> Pairs => map;
}

internal static class ConnectionQueryExtensions
{
    internal static IQueryable<ConnectionQueryBaseRecord> ToIdContains(this IQueryable<ConnectionQueryBaseRecord> query, HashSet<Guid> ids)
    {
        if (ids is null || ids.Count == 0)
        {
            return query;
        }

        if (ids.Count == 1)
        {
            var id = ids.First();
            return query.Where(t => t.ToId == id);
        }

        return query.Where(t => ids.Contains(t.ToId));
    }

    internal static IQueryable<ConnectionQueryBaseRecord> FromIdContains(this IQueryable<ConnectionQueryBaseRecord> query, HashSet<Guid> ids, bool applyFromFilter = true)
    {
        if (!applyFromFilter || ids is null || ids.Count == 0)
        {
            return query;
        }

        if (ids.Count == 1)
        {
            var id = ids.First();
            return query.Where(t => t.FromId == id);
        }

        return query.Where(t => ids.Contains(t.FromId));
    }

    internal static IQueryable<ConnectionQueryBaseRecord> ViaIdContains(this IQueryable<ConnectionQueryBaseRecord> query, HashSet<Guid> ids)
    {
        if (ids is null || ids.Count == 0)
        {
            return query;
        }

        if (ids.Count == 1)
        {
            var id = ids.First();
            return query.Where(t => t.ViaId.HasValue && t.ViaId.Value == id);
        }

        return query.Where(t => t.ViaId.HasValue && ids.Contains(t.ViaId.Value));
    }

    internal static IQueryable<ConnectionQueryBaseRecord> RoleIdExcludes(this IQueryable<ConnectionQueryBaseRecord> query, HashSet<Guid> ids)
    {
        if (ids is null || ids.Count == 0)
        {
            return query;
        }

        if (ids.Count == 1)
        {
            var id = ids.First();
            return query.Where(t => t.RoleId != id);
        }

        return query.Where(t => !ids.Contains(t.RoleId));
    }

    internal static IQueryable<ConnectionQueryBaseRecord> RoleIdContains(this IQueryable<ConnectionQueryBaseRecord> query, HashSet<Guid> ids)
    {
        if (ids is null || ids.Count == 0)
        {
            return query;
        }

        if (ids.Count == 1)
        {
            var id = ids.First();
            return query.Where(t => t.RoleId == id);
        }

        return query.Where(t => ids.Contains(t.RoleId));
    }

    internal static IQueryable<ConnectionQueryBaseRecord> ViaRoleIdContains(this IQueryable<ConnectionQueryBaseRecord> query, HashSet<Guid> ids)
    {
        if (ids is null || ids.Count == 0)
        {
            return query;
        }

        if (ids.Count == 1)
        {
            var id = ids.First();
            return query.Where(t => t.ViaRoleId.HasValue && t.ViaRoleId == id);
        }

        return query.Where(t => t.ViaRoleId.HasValue && ids.Contains(t.ViaRoleId.Value));
    }
}

public enum ConnectionQueryDirection
{
    FromOthers,
    ToOthers
}
