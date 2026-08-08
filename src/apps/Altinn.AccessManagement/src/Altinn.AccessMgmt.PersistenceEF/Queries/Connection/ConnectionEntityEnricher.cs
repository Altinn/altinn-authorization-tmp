using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection.Models;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Queries.Connection;

/// <summary>
/// Responsible for enriching connection query records with entity, child, and role data.
/// </summary>
internal class ConnectionEntityEnricher(AppDbContext db)
{
    /// <summary>
    /// Enriches the given records with entity, role, and child-nesting data.
    /// </summary>
    public async Task<List<ConnectionQueryExtendedRecord>> EnrichAsync(List<ConnectionQueryExtendedRecord> allKeys, ConnectionQueryFilter filter, bool delayedEnrichment, CancellationToken ct)
    {
        var entityDict = await FetchEntitiesAsync(allKeys, ct);
        var childrenDict = delayedEnrichment ? await FetchChildrenAsync(entityDict, filter, delayedEnrichment, ct) : [];
        var rolesDict = await FetchRolesAsync(ct);

        return ApplyEnrichment(allKeys, entityDict, childrenDict, rolesDict, delayedEnrichment, filter);
    }

    /// <summary>
    /// Bulk-loads entities by collected party IDs from the records.
    /// </summary>
    private async Task<Dictionary<Guid, Entity>> FetchEntitiesAsync(List<ConnectionQueryExtendedRecord> allKeys, CancellationToken ct)
    {
        HashSet<Guid> parties = [];
        foreach (var item in allKeys)
        {
            parties.Add(item.FromId);
            parties.Add(item.ToId);

            if (item.ViaId != null)
            {
                parties.Add((Guid)item.ViaId);
            }
        }

        var entities = await db
            .Entities
            .AsNoTracking()
            .Where(e => parties.Contains(e.Id))
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

        return entities.ToDictionary(e => e.Id);
    }

    /// <summary>
    /// Loads child entities for hierarchy nesting.
    /// </summary>
    private async Task<Dictionary<Guid, List<Entity>>> FetchChildrenAsync(Dictionary<Guid, Entity> entityDict, ConnectionQueryFilter filter, bool delayedEnrichment, CancellationToken ct)
    {
        var parentIds = entityDict.Keys.ToList();
        var fromIdSet = delayedEnrichment && filter.FromIds?.Count > 0 ? new HashSet<Guid>(filter.FromIds) : null;

        var allChildren = await db
            .Entities
            .AsNoTracking()
            .Where(e => e.ParentId != null && parentIds.Contains((Guid)e.ParentId))
            .WhereIf(fromIdSet != null, e => fromIdSet!.Contains(e.Id))
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
                VariantId = e.VariantId
            })
            .Distinct()
            .AsNoTracking()
            .ToListAsync(ct);

        // Assign Parent in-memory from the already-loaded entity dictionary
        foreach (var child in allChildren)
        {
            if (child.ParentId != null && entityDict.TryGetValue((Guid)child.ParentId, out var parent))
            {
                child.Parent = parent;
            }
        }

        Dictionary<Guid, List<Entity>> childrenDict = [];
        foreach (var child in allChildren)
        {
            if (!childrenDict.TryGetValue((Guid)child.ParentId!, out var list))
            {
                list = [child];
                childrenDict.Add((Guid)child.ParentId!, list);
            }
            else
            {
                list.Add(child);
            }
        }

        return childrenDict;
    }

    /// <summary>
    /// Loads roles. Could be cached or fetched from RoleConstants instead to avoid DB roundtrip. Added as issue #3793.
    /// </summary>
    private async Task<Dictionary<Guid, Role>> FetchRolesAsync(CancellationToken ct)
    {
        var roles = await db.Roles.Include(r => r.Provider).ThenInclude(p => p.Type).AsNoTracking().ToListAsync(ct);
        return roles.ToDictionary(r => r.Id);
    }

    /// <summary>
    /// Attaches entities/roles to records, and expands children.
    /// </summary>
    private static List<ConnectionQueryExtendedRecord> ApplyEnrichment(List<ConnectionQueryExtendedRecord> allKeys, Dictionary<Guid, Entity> entityDict, Dictionary<Guid, List<Entity>> childrenDict, Dictionary<Guid, Role> rolesDict, bool delayedEnrichment, ConnectionQueryFilter filter)
    {
        var fromIdSet = delayedEnrichment && filter.FromIds?.Count > 0 ? new HashSet<Guid>(filter.FromIds) : null;

        List<ConnectionQueryExtendedRecord> keysWithChildren = [];
        foreach (var c in allKeys)
        {
            c.From = entityDict[c.FromId];
            c.To = entityDict[c.ToId];
            c.Via = c.ViaId != null ? entityDict[(Guid)c.ViaId] : null;
            c.Role = c.RoleId != Guid.Empty ? rolesDict[c.RoleId] : null;
            c.ViaRole = c.ViaRoleId != null && c.ViaRoleId != Guid.Empty ? rolesDict[(Guid)c.ViaRoleId] : null;
            keysWithChildren.Add(c);

            if (delayedEnrichment && c.Reason != ConnectionReason.Hierarchy && childrenDict.TryGetValue(c.From.Id, out List<Entity> childrenForKey))
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

        if (fromIdSet != null)
        {
            keysWithChildren = keysWithChildren.Where(c => fromIdSet.Contains(c.FromId)).ToList();
        }

        return keysWithChildren;
    }
}
