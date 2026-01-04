using Altinn.AccessManagement.Core.Models;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc/>
public class AuthorizedPartyRepoServiceEf(AppDbContext db, ConnectionQuery connectionQuery, IServiceProvider _serviceProvider) : IAuthorizedPartyRepoServiceEf
{
    /// <inheritdoc/>
    public async Task<Entity?> GetEntity(Guid id, CancellationToken ct = default) =>
        await db.Entities
            .AsNoTracking()
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc/>
    public async Task<Entity?> GetEntityByPartyId(int partyId, CancellationToken ct = default) =>
        await db.Entities
            .AsNoTracking()
            .Where(t => t.PartyId == partyId)
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc/>
    public async Task<Entity?> GetEntityByOrganizationId(string organizationId, CancellationToken ct = default) => 
        await db.Entities
            .AsNoTracking()
            .Where(e => e.OrganizationIdentifier == organizationId)
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc/>
    public async Task<Entity?> GetEntityByPersonId(string personId, CancellationToken ct = default) => 
        await db.Entities
            .AsNoTracking()
            .Where(e => e.PersonIdentifier == personId)
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc/>
    public async Task<Entity?> GetEntityByUserId(int userId, CancellationToken ct = default) =>
        await db.Entities
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc/>
    public async Task<Entity?> GetEntityByUsername(string username, CancellationToken ct = default) =>
        await db.Entities
            .AsNoTracking()
            .Where(e => e.Username == username)
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc/>
    public async Task<IEnumerable<Entity>> GetEntities(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        return await db.Entities
            .AsNoTracking()
            .Where(r => ids.Contains(r.Id))
            .Include(t => t.Parent)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Entity>> GetEntitiesByPartyIds(IEnumerable<int> partyIds, CancellationToken ct = default)
    {
        return await db.Entities
            .AsNoTracking()
            .Where(r => r.PartyId.HasValue && partyIds.Contains(r.PartyId.Value))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Entity>> GetSubunits(IEnumerable<Guid> parentIds, CancellationToken ct = default)
    {
        return await db.Entities
            .AsNoTracking()
            .Where(t => t.ParentId.HasValue && parentIds.Contains(t.ParentId.Value))
            .Include(t => t.Parent)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Assignment>> GetKeyRoleAssignments(Guid toId, CancellationToken ct = default)
    {
        return await db.Assignments.AsNoTracking()
            .Where(t => t.ToId == toId)
            .Include(t => t.Role)
            .Where(t => t.Role.IsKeyRole)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<List<ConnectionQueryExtendedRecord>> GetConnectionsFromOthers(
        Guid toId,
        AuthorizedPartiesFilters filters = null,
        CancellationToken ct = default)
    {
        return await connectionQuery.GetConnectionsAsync(
        new ConnectionQueryFilter()
        {
            ToIds = [toId],
            FromIds = filters?.PartyFilter?.Keys.ToList(),
            PackageIds = filters?.PackageFilter?.Keys.ToList(),
            EnrichEntities = filters?.IncludeAccessPackages ?? false,
            IncludeSubConnections = true,
            IncludeKeyRole = filters?.IncludePartiesViaKeyRoles == AuthorizedPartiesIncludeFilter.True ? true : false,
            IncludeMainUnitConnections = true,
            IncludeDelegation = true,
            IncludePackages = filters?.IncludeAccessPackages ?? false,
            IncludeResource = false,
            EnrichPackageResources = false,
            ExcludeDeleted = false
        },
        ConnectionQueryDirection.FromOthers,
        useNewQuery: true,
        ct);
    }

    /// <inheritdoc />
    public async Task<List<ConnectionQueryExtendedRecord>> GetPipConnectionsFromOthers(
        Guid toId,
        AuthorizedPartiesFilters filters = null,
        CancellationToken ct = default)
    {
        return await connectionQuery.GetPipConnectionPackagesAsync(
        new ConnectionQueryFilter()
        {
            ToIds = [toId],
            FromIds = filters?.PartyFilter?.Keys.ToList(),
            PackageIds = null,
            EnrichEntities = false,
            IncludeSubConnections = true,
            IncludeKeyRole = filters?.IncludePartiesViaKeyRoles == AuthorizedPartiesIncludeFilter.True ? true : false,
            IncludeMainUnitConnections = true,
            IncludeDelegation = true,
            IncludePackages = filters?.IncludeAccessPackages ?? true,
            IncludeResource = false,
            EnrichPackageResources = false,
            ExcludeDeleted = false
        },
        ct);
    }

    public async Task<SortedDictionary<string, Resource>> GetResourcesByProvider(string? providerCode = null, IEnumerable<string>? resourceIds = null, CancellationToken ct = default)
    {
        var list = await db.Resources
            .AsNoTracking()
            .Include(res => res.Provider)
            .WhereIf(providerCode != null, res => res.Provider.Code == providerCode)
            .WhereIf(resourceIds != null, res => resourceIds!.Contains(res.RefId))
            .ToListAsync(ct);

        var sorted = new SortedDictionary<string, Resource>(StringComparer.Ordinal);
        foreach (var item in list)
        {
            sorted[item.RefId] = item;
        }

        return sorted;

    }

    public async Task<SortedDictionary<string, List<Resource>>> GetResourcesGroupedByRoleCode(
        string? providerCode = null,
        IEnumerable<string>? resourceIds = null,
        CancellationToken ct = default)
    {
        var roleResources = await db.RoleResources
            .AsNoTracking()
            .Include(rr => rr.Role)
            .Include(rr => rr.Resource)
                .ThenInclude(res => res.Provider)
            .WhereIf(providerCode != null, rr => rr.Resource.Provider.Code == providerCode)
            .WhereIf(resourceIds != null, rr => resourceIds!.Contains(rr.Resource.RefId))
            .ToListAsync(ct);

        var sorted = new SortedDictionary<string, List<Resource>>();
        foreach (var group in roleResources.GroupBy(rr => rr.RoleId))
        {
            Role role = group.First().Role;
            sorted[role.Code] = group.Select(rr => rr.Resource).ToList();

            if (role.LegacyCode != null)
            {
                sorted[role.LegacyCode] = sorted[role.Code];
            }
        }

        return sorted;
    }

    public async Task<SortedDictionary<Guid, List<Resource>>> GetResourcesGroupedByPackageId(string? providerCode = null, IEnumerable<string>? resourceIds = null, CancellationToken ct = default)
    {
        var packageResources = await db.PackageResources
            .AsNoTracking()
            .Include(pr => pr.Resource)
                .ThenInclude(res => res.Provider)
            .WhereIf(providerCode != null, pr => pr.Resource.Provider.Code == providerCode)
            .WhereIf(resourceIds != null, pr => resourceIds.Contains(pr.Resource.RefId))
            .ToListAsync(ct);

        var sorted = new SortedDictionary<Guid, List<Resource>>();
        foreach (var group in packageResources.GroupBy(pr => pr.PackageId))
        {
            sorted[group.Key] = group.Select(pr => pr.Resource).ToList();
        }

        return sorted;
    }

    public async Task<SortedDictionary<Guid, List<Role>>> GetRolesGroupedByPackageId(IEnumerable<Guid>? roleIds = null, IEnumerable<Guid>? packageIds = null, CancellationToken ct = default)
    {
        var rolePackages = await db.RolePackages
            .AsNoTracking()
            .Include(rp => rp.Role)
            .WhereIf(roleIds != null, rp => roleIds.Contains(rp.RoleId))
            .WhereIf(packageIds != null, rp => packageIds.Contains(rp.PackageId))
            .ToListAsync(ct);

        var sorted = new SortedDictionary<Guid, List<Role>>();
        foreach (var group in rolePackages.GroupBy(rp => rp.PackageId))
        {
            sorted[group.Key] = group.Select(rp => rp.Role).ToList();
        }

        return sorted;
    }
}
