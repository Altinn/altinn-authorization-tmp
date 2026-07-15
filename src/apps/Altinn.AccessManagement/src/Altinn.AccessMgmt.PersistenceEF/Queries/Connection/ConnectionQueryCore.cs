using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection.Models;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Queries.Connection;

/// <summary>
/// Core connection query responsible for building base queries and loading packages.
/// Provides the performance-optimized path for PIP and AuthorizedParties without entity enrichment.
/// </summary>
public class ConnectionQueryCore
{
    // This is a workaround to prevent Postgres from choosing a bad join order when there is a filter on FromId in delegations.
    // It looks like the same join order is chosen no matter if we use a low value (1) or a very large value. A very large value is
    // chosen to ensure that the cutoff doesn't really reduce the result set.
    private const int PostgresPlanHintTakeLimit = 100_000_000;

    protected readonly AppDbContext Db;

    public ConnectionQueryCore(AppDbContext db)
    {
        Db = db;
    }

    /// <summary>
    /// Checks if connection exists between two parties.
    /// Returns first result only (Assignment => Delegation => Hierarchy => KeyRole)
    /// </summary>
    public async Task<(bool Result, ConnectionReason? Reason)> HasConnection(Guid fromId, Guid toId)
    {
        return await HasConnection(fromId, toId, [ConnectionReason.Assignment, ConnectionReason.Delegation, ConnectionReason.Hierarchy, ConnectionReason.KeyRole]);
    }

    /// <summary>
    /// Checks if connection exists between two parties.
    /// Returns first result only (Assignment => Delegation => Hierarchy => KeyRole)
    /// </summary>
    public async Task<(bool Result, ConnectionReason? Reason)> HasConnection(Guid fromId, Guid toId, ConnectionReason[] reasons)
    {
        if (reasons == null || reasons.Length == 0)
        {
            return (false, null);
        }

        if (reasons.Contains(ConnectionReason.Assignment))
        {
            var assignments = Db.Assignments.AsNoTracking()
                .Where(t => t.FromId == fromId && t.ToId == toId);

            if (await assignments.AnyAsync())
            {
                return (true, ConnectionReason.Assignment);
            }
        }

        if (reasons.Contains(ConnectionReason.Delegation))
        {
            var delegations = Db.Delegations.AsNoTracking()
                .Where(t => t.From.FromId == fromId && t.To.ToId == toId);

            if (await delegations.AnyAsync())
            {
                return (true, ConnectionReason.Delegation);
            }
        }

        if (reasons.Contains(ConnectionReason.Hierarchy))
        {
            var hierarchy =
            from a in Db.Assignments.AsNoTracking()
            join e in Db.Entities.AsNoTracking() on a.FromId equals e.ParentId
            where a.ToId == toId && e.Id == fromId
            select 1;

            if (await hierarchy.AnyAsync())
            {
                return (true, ConnectionReason.Hierarchy);
            }
        }

        if (reasons.Contains(ConnectionReason.KeyRole))
        {
            var keyRoles =
            from a in Db.Assignments.AsNoTracking()
            join ae in Db.Entities.AsNoTracking() on a.FromId equals ae.Id
            join k in Db.Assignments.AsNoTracking() on a.ToId equals k.FromId
            join kr in Db.Roles.AsNoTracking() on k.RoleId equals kr.Id
            where a.FromId == fromId && kr.IsKeyRole == true && k.ToId == toId && (a.RoleId != RoleConstants.ParticipantSharedResponsibility.Id || ae.VariantId != EntityVariantConstants.IKS.Id)
            select 1;

            if (await keyRoles.AnyAsync())
            {
                return (true, ConnectionReason.KeyRole);
            }
        }

        return (false, null);
    }

    /// <summary>
    /// Builds and executes the base connection query, returning base records without entity enrichment.
    /// Optionally loads packages.
    /// </summary>
    public async Task<List<ConnectionQueryBaseRecord>> GetBaseConnectionsAsync(ConnectionQueryFilter filter, ConnectionQueryDirection direction, CancellationToken ct = default)
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

        return await baseQuery.ToListAsync(ct);
    }

    /// <summary>
    /// Loads packages for the given base records and returns a ConnectionIndex.
    /// </summary>
    public async Task<ConnectionIndex<ConnectionQueryPackage>> LoadPackagesByKeyAsync(IEnumerable<ConnectionQueryBaseRecord> keys, ConnectionQueryFilter filter, CancellationToken ct)
    {
        var packageSet = filter.PackageIds?.Count > 0 ? new HashSet<Guid>(filter.PackageIds) : null;
        var index = new ConnectionIndex<ConnectionQueryPackage>();

        var assignmentPackageKeys = keys.Where(k => k.AssignmentId.HasValue && k.RoleId == RoleConstants.Rightholder.Id).Select(k => k.AssignmentId).Distinct().ToList();

        SortedList<Guid, List<Guid>> assignmentPackages = [];
        SortedSet<Guid> assignmentPackageIds = [];
        if (assignmentPackageKeys.Count > 0)
        {
            var assignmentPackagesRaw = await Db.AssignmentPackages.Where(a => assignmentPackageKeys.Contains(a.AssignmentId))
                .WhereIf(packageSet is not null, p => packageSet!.Contains(p.PackageId))
                .Select(ap => new { ap.PackageId, ap.AssignmentId })
                .ToListAsync(ct);

            foreach (var assignmentPackage in assignmentPackagesRaw)
            {
                assignmentPackageIds.Add(assignmentPackage.PackageId);
                if (assignmentPackages.TryGetValue(assignmentPackage.AssignmentId, out var ids))
                {
                    ids.Add(assignmentPackage.PackageId);
                }
                else
                {
                    assignmentPackages.Add(assignmentPackage.AssignmentId, [assignmentPackage.PackageId]);
                }
            }
        }

        var rolePackageKeys = keys.Where(k => k.AssignmentId.HasValue).Select(k => k.RoleId).Distinct().ToList();
        var rolePackagesRaw = await Db.RolePackages.Where(r => r.HasAccess && rolePackageKeys.Contains(r.RoleId))
            .WhereIf(packageSet is not null, p => packageSet!.Contains(p.PackageId))
            .Select(rp => new { rp.PackageId, rp.RoleId, rp.EntityVariantId })
            .ToListAsync(ct);
        SortedList<Guid, List<Guid>> rolePackagesForAll = [];
        SortedList<Guid, Dictionary<Guid, List<Guid>>> rolePackagesForEntity = [];
        SortedSet<Guid> rolePackageIds = [];
        foreach (var rolePackage in rolePackagesRaw)
        {
            rolePackageIds.Add(rolePackage.PackageId);
            if (rolePackage.EntityVariantId == null)
            {
                if (rolePackagesForAll.TryGetValue(rolePackage.RoleId, out var ids))
                {
                    ids.Add(rolePackage.PackageId);
                }
                else
                {
                    rolePackagesForAll.Add(rolePackage.RoleId, [rolePackage.PackageId]);
                }
            }
            else
            {
                if (rolePackagesForEntity.TryGetValue(rolePackage.RoleId, out var variantDict))
                {
                    if (variantDict.TryGetValue((Guid)rolePackage.EntityVariantId, out var ids))
                    {
                        ids.Add(rolePackage.PackageId);
                    }
                    else
                    {
                        variantDict.Add((Guid)rolePackage.EntityVariantId, [rolePackage.PackageId]);
                    }
                }
                else
                {
                    rolePackagesForEntity.Add(rolePackage.RoleId, new() { { (Guid)rolePackage.EntityVariantId, [rolePackage.PackageId] } });
                }
            }
        }

        SortedList<Guid, Guid> entityVariants = [];
        var entityKeys = keys.Where(k => k.AssignmentId.HasValue && rolePackagesForEntity.ContainsKey(k.RoleId)).Select(k => k.FromId).Distinct().ToList();
        if (entityKeys.Count > 0)
        {
            var entityVariantsRaw = await Db.Entities.Where(e => entityKeys.Contains(e.Id))
                .Select(e => new { e.Id, e.VariantId })
                .ToListAsync(ct);
            foreach (var entityVariant in entityVariantsRaw)
            {
                entityVariants[entityVariant.Id] = entityVariant.VariantId;
            }
        }

        var delegationIds = keys.Select(k => k.DelegationId).Where(id => id != null).Distinct().ToList();
        var delegationPackagesRaw = delegationIds.Count == 0 ? [] :
            await Db.DelegationPackages.Where(d => delegationIds.Contains(d.DelegationId))
            .WhereIf(packageSet is not null, p => packageSet!.Contains(p.PackageId))
            .Select(d => new { d.PackageId, d.DelegationId })
            .ToListAsync(ct);
        SortedList<Guid, List<Guid>> delegationPackages = [];
        SortedSet<Guid> delegationPackageIds = [];
        foreach (var delegationPackage in delegationPackagesRaw)
        {
            delegationPackageIds.Add(delegationPackage.PackageId);
            if (delegationPackages.TryGetValue(delegationPackage.DelegationId, out var ids))
            {
                ids.Add(delegationPackage.PackageId);
            }
            else
            {
                delegationPackages.Add(delegationPackage.DelegationId, [delegationPackage.PackageId]);
            }
        }

        var packageIds = assignmentPackageIds
            .Union(rolePackageIds)
            .Union(delegationPackageIds)
            .Distinct()
            .ToList();

        var packagesRaw = await Db.Packages.Where(p => packageIds.Contains(p.Id)).Select(p => new { p.Id, p.Name, p.AreaId, p.Urn }).ToListAsync(ct);
        SortedList<Guid, ConnectionQueryPackage> packages = [];
        foreach (var package in packagesRaw)
        {
            packages[package.Id] = new() { Id = package.Id, Name = package.Name, AreaId = package.AreaId, Urn = package.Urn };
        }

        foreach (var key in keys)
        {
            IEnumerable<Guid> keyPackageIds = [];

            if (!key.AssignmentId.HasValue)
            {
                // if connection record is a client delegation, we only need to consider delegationpackages, and not from assignment or role packages.
                var clientDelegationPackages = key.DelegationId.HasValue
                 ? (key.DelegationId != null && delegationPackages.ContainsKey((Guid)key.DelegationId)) ? delegationPackages[(Guid)key.DelegationId] : []
                 : [];

                keyPackageIds = clientDelegationPackages.Distinct();
            }
            else if (key.AssignmentId.HasValue && key.RoleId == RoleConstants.Rightholder.Id)
            {
                // if connection is for rightholder we only need to consider assignment packages and not role packages or client delegations
                var assPackages = key.AssignmentId.HasValue
                     ? assignmentPackages.ContainsKey((Guid)key.AssignmentId) ? assignmentPackages[(Guid)key.AssignmentId] : []
                     : [];

                keyPackageIds = assPackages.Distinct();
            }
            else
            {
                // else we need to consider check for role packages
                List<Guid> rolePackagesForEntityForKey = [];
                if (rolePackagesForEntity.TryGetValue(key.RoleId, out var entityDict)
                    && entityDict.TryGetValue(entityVariants[key.FromId], out var entityIds))
                {
                    rolePackagesForEntityForKey = entityIds;
                }

                keyPackageIds = (rolePackagesForAll.TryGetValue(key.RoleId, out List<Guid> packagesForAll) ? packagesForAll : [])
                        .Union(rolePackagesForEntityForKey).Distinct();
            }

            if (!keyPackageIds.Any())
            {
                continue;
            }

            List<ConnectionQueryPackage> keyPackages = [];
            foreach (var id in keyPackageIds)
            {
                keyPackages.Add(packages[id]);
            }

            index.AddRange(new(key.FromId, key.ToId, key.RoleId, key.AssignmentId, key.DelegationId, key.ViaId, key.ViaRoleId), keyPackages);
        }

        return index;
    }

    /// <summary>
    /// Enriches packages with their resource details.
    /// </summary>
    public async Task EnrichPackageResourcesAsync(ConnectionIndex<ConnectionQueryPackage> packageIndex, ConnectionQueryFilter filter, CancellationToken ct = default)
    {
        var packageIds = packageIndex.Pairs
            .SelectMany(kv => kv.Value)
            .Select(p => p.Id)
            .Distinct()
            .ToList();

        if (packageIds.Count == 0)
        {
            return;
        }

        var resourceSet = filter.ResourceIds is { Count: > 0 }
            ? new HashSet<Guid>(filter.ResourceIds!)
            : null;

        var rows = await Db.PackageResources
            .Where(pr => packageIds.Contains(pr.PackageId))
            .WhereIf(resourceSet is not null, pr => resourceSet!.Contains(pr.ResourceId))
            .Join(Db.Resources, pr => pr.ResourceId, r => r.Id, (pr, r) => new
            {
                pr.PackageId,
                Resource = new ConnectionQueryResource { Id = r.Id, Name = r.Name }
            })
            .AsNoTracking()
            .ToListAsync(ct);

        var resourcesByPackage = rows
            .GroupBy(x => x.PackageId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(z => z.Resource).DistinctBy(r => r.Id).ToList()
            );

        foreach (var kv in packageIndex.Pairs)
        {
            var packages = kv.Value;

            foreach (var pkg in packages.ToList())
            {
                if (resourcesByPackage.TryGetValue(pkg.Id, out var list))
                {
                    pkg.Resources = list.ToList();
                }
                else
                {
                    pkg.Resources = new List<ConnectionQueryResource>(capacity: 0);
                }
            }

            // Remove empty packages if IncludePackages is false
            if (!filter.IncludePackages)
            {
                kv.Value.RemoveAll(p => p.Resources.Count == 0);
            }
        }
    }

    protected IQueryable<ConnectionQueryBaseRecord> BuildBaseQueryFromOthers(ConnectionQueryFilter filter, bool doChildNesting, bool applyFromFilter)
    {
        var toId = filter.ToIds.First();
        var fromSet = filter.FromIds?.Count > 0 ? new HashSet<Guid>(filter.FromIds) : null;
        var fromSetForDelegation = fromSet != null && filter.IncludeDelegation ? new HashSet<Guid>(fromSet) : [];
        var viaSet = filter.ViaIds?.Count > 0 ? new HashSet<Guid>(filter.ViaIds) : null;
        var viaRoleSet = filter.ViaRoleIds?.Count > 0 ? new HashSet<Guid>(filter.ViaRoleIds) : null;
        var roleSet = filter.RoleIds?.Count > 0 ? new HashSet<Guid>(filter.RoleIds) : null;
        var roleSetExclude = BuildRoleExcludeSet(filter);

        if (fromSet != null && filter.IncludeDelegation)
        {
            var parentIds = Db.Entities
                .Where(e => fromSet.Distinct().Contains(e.Id) && e.ParentId != null)
                .AsNoTracking()
                .Select(e => e.ParentId.Value)
                .Distinct()
                .ToList();
            fromSetForDelegation.UnionWith(parentIds);

            var enksOfInnh = Db.Assignments
                .AsNoTracking()
                .Where(a => fromSet.Distinct().Contains(a.ToId))
                .Where(a => a.RoleId == RoleConstants.Innehaver.Id)
                .Select(a => a.FromId)
                .Distinct()
                .ToList();
            fromSetForDelegation.UnionWith(enksOfInnh);
        }

        var reviRegnRoleSet = new HashSet<Guid>
        {
            RoleConstants.Accountant.Id,
            RoleConstants.Auditor.Id,
            RoleConstants.AccountantWithoutSigningRights.Id,
            RoleConstants.AccountantWithSigningRights.Id,
            RoleConstants.AccountantSalary.Id,
            RoleConstants.AssistantAuditor.Id,
            RoleConstants.AuditorInCharge.Id
        };

        var direct =
            Db.Assignments
                .Where(a1 => a1.ToId == toId)
                .Select(a1 => new ConnectionQueryBaseRecord
                {
                    AssignmentId = a1.Id,
                    DelegationId = null,
                    FromId = a1.FromId,
                    ToId = a1.ToId,
                    RoleId = a1.RoleId,
                    ViaId = null,
                    ViaRoleId = null,
                    Reason = ConnectionReason.Assignment,
                    IsKeyRoleAccess = false,
                    IsMainUnitAccess = false,
                    IsRoleMap = false,
                });

        var keyrole =
            direct
                .Join(
                    Db.Roles,
                    d => d.RoleId,
                    r => r.Id,
                    (d, r) => new { d, r }
                )
                .Where(x => x.r.IsKeyRole)
                .Join(
                    Db.Assignments,
                    x => x.d.FromId,
                    a2 => a2.ToId,
                    (x, a2) => new { x, a2 }
                )
                .Join(
                    Db.Entities,
                    y => y.a2.FromId,
                    e => e.Id,
                    (y, e) => new { y.x, y.a2, e }
                )
                .Where(z => !(z.e.VariantId == EntityVariantConstants.IKS.Id && z.a2.RoleId == RoleConstants.ParticipantSharedResponsibility.Id))
                .Select(z => new ConnectionQueryBaseRecord
                {
                    AssignmentId = z.a2.Id,
                    DelegationId = null,
                    FromId = z.a2.FromId,
                    ToId = z.x.d.ToId,
                    RoleId = z.a2.RoleId,
                    ViaId = z.x.d.FromId,
                    ViaRoleId = z.x.d.RoleId,
                    Reason = ConnectionReason.KeyRole,
                    IsKeyRoleAccess = true,
                    IsMainUnitAccess = false,
                    IsRoleMap = false,
                });

        var a1 = filter.IncludeKeyRole
            ? direct.Concat(keyrole)
            : direct;

        var rolemap =
            a1
                .Join(
                    Db.RoleMaps,
                    dkr => dkr.RoleId,
                    rm => rm.HasRoleId,
                    (dkr, rm) => new ConnectionQueryBaseRecord
                    {
                        AssignmentId = dkr.AssignmentId,
                        DelegationId = null,
                        FromId = dkr.FromId,
                        ToId = dkr.ToId,
                        RoleId = rm.GetRoleId,
                        ViaId = dkr.ViaId,
                        ViaRoleId = null,
                        Reason = ConnectionReason.RoleMap,
                        IsKeyRoleAccess = dkr.IsKeyRoleAccess,
                        IsMainUnitAccess = false,
                        IsRoleMap = true,
                    });

        var delegations =
            Db.Assignments
                .WhereIf(fromSetForDelegation.Count > 0, x => fromSetForDelegation.Contains(x.FromId))
                .Join(
                    Db.Delegations,
                    fa => fa.Id,
                    d => d.FromId,
                    (fa, d) => new { fa, d }
                )
                //// Generate a postgres limit to force a better execution plan when there's a party filter. Ref. comment on constant PostgresPlanHintTakeLimit.
                .TakeIf(fromSetForDelegation.Count > 0, PostgresPlanHintTakeLimit)
                .Join(
                    Db.Assignments,
                    x => x.d.ToId,
                    dkr => dkr.Id,
                    (x, dkr) => new { x, dkr }
                )
                .Where(t =>
                    t.dkr.ToId == toId &&
                    t.dkr.RoleId == RoleConstants.Agent.Id
                )
                .Select(t => new ConnectionQueryBaseRecord
                {
                    AssignmentId = null,
                    DelegationId = t.x.d.Id,
                    FromId = t.x.fa.FromId,
                    ToId = t.dkr.ToId,
                    RoleId = t.x.fa.RoleId,
                    ViaId = t.x.fa.ToId,
                    ViaRoleId = t.dkr.RoleId,
                    Reason = ConnectionReason.Delegation,
                    IsKeyRoleAccess = false,
                    IsMainUnitAccess = false,
                    IsRoleMap = false,
                });

        var a2 = filter.IncludeDelegation
            ? a1.Concat(rolemap).Concat(delegations)
            : a1.Concat(rolemap);

        var fromChildren = !doChildNesting ? a2 :
        a2
        .Join(
            Db.Entities,
            c => c.FromId,
            e => e.ParentId,
            (c, e) => new ConnectionQueryBaseRecord
            {
                AssignmentId = c.AssignmentId,
                DelegationId = c.DelegationId,
                FromId = e.Id,
                ToId = c.ToId,
                RoleId = c.RoleId,
                ViaId = c.FromId,
                ViaRoleId = c.ViaRoleId,
                Reason = ConnectionReason.Hierarchy,
                IsKeyRoleAccess = c.IsKeyRoleAccess,
                IsMainUnitAccess = true,
                IsRoleMap = c.IsRoleMap,
            });

        var innehaverConnections =
            from reviRegnConnection in a2
            join innehaverConnection in Db.Assignments on reviRegnConnection.FromId equals innehaverConnection.FromId
            join innehaver in Db.Entities on innehaverConnection.ToId equals innehaver.Id
            join enk in Db.Entities on innehaverConnection.FromId equals enk.Id
            where reviRegnRoleSet.Contains(reviRegnConnection.RoleId)
                && innehaverConnection.RoleId == RoleConstants.Innehaver.Id
                && enk.VariantId == EntityVariantConstants.ENK.Id
                && innehaver.DateOfDeath == null
                && (!enk.IsDeleted || (enk.DeletedAt != null && enk.DeletedAt.Value.AddYears(2) > DateTime.UtcNow))
            select new ConnectionQueryBaseRecord()
            {
                AssignmentId = reviRegnConnection.AssignmentId,
                DelegationId = reviRegnConnection.DelegationId,
                FromId = innehaverConnection.ToId,
                ToId = reviRegnConnection.ToId,
                RoleId = reviRegnConnection.RoleId,
                ViaId = innehaverConnection.FromId,
                ViaRoleId = innehaverConnection.RoleId,
                IsKeyRoleAccess = reviRegnConnection.IsKeyRoleAccess,
                IsRoleMap = reviRegnConnection.IsRoleMap,
                IsMainUnitAccess = reviRegnConnection.IsMainUnitAccess,
                Reason = ConnectionReason.Hierarchy,
            };

        var query = doChildNesting
            ? filter.IncludeSubConnections ? a2.Concat(fromChildren).Concat(innehaverConnections) : a2.Concat(fromChildren)
            : filter.IncludeSubConnections ? a2.Concat(innehaverConnections) : a2;

        return
            query
            .FromIdContains(fromSet, applyFromFilter)
            .ViaIdContains(viaSet)
            .ViaRoleIdContains(viaRoleSet)
            .RoleIdContains(roleSet)
            .RoleIdExcludes(roleSetExclude);
    }

    protected IQueryable<ConnectionQueryBaseRecord> BuildBaseQueryToOthers(ConnectionQueryFilter filter)
    {
        var fromId = filter.FromIds.First();
        var toSet = filter.ToIds?.Count > 0 ? new HashSet<Guid>(filter.ToIds) : null;
        var viaSet = filter.ViaIds?.Count > 0 ? new HashSet<Guid>(filter.ViaIds) : null;
        var viaRoleSet = filter.ViaRoleIds?.Count > 0 ? new HashSet<Guid>(filter.ViaRoleIds) : null;
        var roleSet = filter.RoleIds?.Count > 0 ? new HashSet<Guid>(filter.RoleIds) : null;
        var roleSetExclude = BuildRoleExcludeSet(filter);

        /*
        Direct Assignments
        */

        var direct =
            from childAss in Db.Assignments
            where childAss.FromId == fromId
            select new ConnectionQueryBaseRecord()
            {
                AssignmentId = childAss.Id,
                DelegationId = null,
                FromId = childAss.FromId,
                ToId = childAss.ToId,
                RoleId = childAss.RoleId,
                ViaId = null,
                ViaRoleId = null,
                IsRoleMap = false,
                IsKeyRoleAccess = false,
                IsMainUnitAccess = false,
                Reason = ConnectionReason.Assignment
            };

        /*
        If FromId is a subunit, this will get mainunit assignments
        */
        var mainAssignments =
            from e in Db.Entities
            where e.Id == fromId
            join ass in Db.Assignments on e.ParentId equals ass.FromId
            select new ConnectionQueryBaseRecord()
            {
                AssignmentId = ass.Id,
                DelegationId = null,
                FromId = e.Id,
                ToId = ass.ToId,
                RoleId = ass.RoleId,
                ViaId = ass.FromId,
                ViaRoleId = null,
                IsRoleMap = false,
                IsKeyRoleAccess = false,
                IsMainUnitAccess = true,
                Reason = ConnectionReason.Hierarchy
            };

        /*
        Combine direct and mainunit assignments based on filter request
        */
        var allAssignments = filter.IncludeMainUnitConnections ? direct.Union(mainAssignments) : direct;

        /*
        Add RoleMap roles to allAssignments
        */
        var roleMapAssignments =
           from assignment in allAssignments
           join rolemap in Db.RoleMaps on assignment.RoleId equals rolemap.HasRoleId
           select new ConnectionQueryBaseRecord()
           {
               AssignmentId = assignment.AssignmentId,
               DelegationId = null,
               FromId = assignment.FromId,
               ToId = assignment.ToId,
               RoleId = rolemap.GetRoleId,
               ViaId = assignment.ViaId,
               ViaRoleId = assignment.ViaRoleId,
               IsRoleMap = true,
               IsKeyRoleAccess = assignment.IsKeyRoleAccess,
               IsMainUnitAccess = assignment.IsMainUnitAccess,
               Reason = ConnectionReason.RoleMap
           };

        /*
        Add Delegations from AllAssignments
        */
        var clientDelegations =
           from delegation in Db.Delegations
           join fromAssignment in allAssignments on delegation.FromId equals fromAssignment.AssignmentId
           join toAssignment in Db.Assignments on delegation.ToId equals toAssignment.Id
           select new ConnectionQueryBaseRecord()
           {
               AssignmentId = null,
               DelegationId = delegation.Id,
               FromId = fromAssignment.FromId,
               ToId = toAssignment.ToId,
               RoleId = fromAssignment.RoleId,
               ViaId = fromAssignment.ToId,
               ViaRoleId = toAssignment.RoleId,
               IsRoleMap = false,
               IsKeyRoleAccess = false,
               IsMainUnitAccess = false,
               Reason = ConnectionReason.Delegation
           };

        /*
        Add KeyRoles on allAssignments
        */
        var keyRoleAssignments =
            from all in allAssignments.Concat(roleMapAssignments) // Must include RoleMap assignments
            join fromEntity in Db.Entities on all.FromId equals fromEntity.Id
            join keyRoleAssignment in Db.Assignments on all.ToId equals keyRoleAssignment.FromId
            join role in Db.Roles on keyRoleAssignment.RoleId equals role.Id
            where role.IsKeyRole && !(fromEntity.VariantId == EntityVariantConstants.IKS.Id && all.RoleId == RoleConstants.ParticipantSharedResponsibility.Id)
            select new ConnectionQueryBaseRecord()
            {
                AssignmentId = all.AssignmentId,
                DelegationId = null,
                FromId = all.FromId,
                ToId = keyRoleAssignment.ToId,
                RoleId = all.RoleId,
                ViaId = keyRoleAssignment.FromId,
                ViaRoleId = keyRoleAssignment.RoleId,
                IsKeyRoleAccess = true,
                IsRoleMap = all.IsRoleMap,
                IsMainUnitAccess = all.IsMainUnitAccess,
                Reason = ConnectionReason.KeyRole
            };

        /*
        Combine everything
        */
        IQueryable<ConnectionQueryBaseRecord> allCombined = allAssignments.Union(roleMapAssignments);
        if (filter.IncludeDelegation)
        {
            allCombined = allCombined.Union(clientDelegations);
        }

        if (filter.IncludeKeyRole)
        {
            allCombined = allCombined.Union(keyRoleAssignments);
        }

        return allCombined
            .ToIdContains(toSet)
            .ViaIdContains(viaSet)
            .ViaRoleIdContains(viaRoleSet)
            .RoleIdContains(roleSet)
            .RoleIdExcludes(roleSetExclude);
    }

    /// <summary>
    /// Builds the standard role exclusion set from the filter.
    /// </summary>
    protected static HashSet<Guid> BuildRoleExcludeSet(ConnectionQueryFilter filter)
    {
        var roleSetExclude = filter.ExcludeRoleIds?.Count > 0 ? new HashSet<Guid>(filter.ExcludeRoleIds) : [];
        roleSetExclude.Add(RoleConstants.Supplier.Id); // Supplier role should never be included in results as it is only used for maskinporten schemas.
        if (!filter.IncludeAppControlledInstances)
        {
            roleSetExclude.Add(RoleConstants.AppControlledRightholder.Id); // App-controlled instance access should be excluded if not explicitly requested.
        }

        return roleSetExclude;
    }
}
