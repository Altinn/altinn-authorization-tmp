using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Microsoft.EntityFrameworkCore;

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
    public async Task<IEnumerable<PackagePermissionDto>> GetPackagesFromOthers(
        Guid toId,
        Guid? fromId = null,
        IEnumerable<Guid>? packageIds = null,
        CancellationToken ct = default)
    {
        var connections = await connectionQuery.GetConnectionsAsync(
        new ConnectionQueryFilter()
        {
            ToIds = [toId],
            FromIds = fromId.HasValue ? new[] { fromId.Value } : null,
            PackageIds = packageIds != null ? packageIds.ToList() : null,
            EnrichEntities = true,
            IncludeSubConnections = true,
            IncludeKeyRole = true,
            IncludeMainUnitConnections = true,
            IncludeDelegation = true,
            IncludePackages = true,
            IncludeResource = false,
            EnrichPackageResources = false,
            ExcludeDeleted = false
        },
        ConnectionQueryDirection.FromOthers,
        ct);

        return DtoMapper.ConvertPackages(connections);
    }

    public async Task<Dictionary<string, Resource>> GetResourcesByProvider(string? providerCode = null, IEnumerable<string>? resourceIds = null, CancellationToken ct = default)
    {
        return await db.Resources
            .AsNoTracking()
            .Include(res => res.Provider)
            .WhereIf(providerCode != null, res => res.Provider.Code == providerCode)
            .WhereIf(resourceIds != null, res => resourceIds.Contains(res.RefId))
            .ToDictionaryAsync(res => res.RefId, res => res, ct);
    }

    public async Task<Dictionary<Guid, IEnumerable<RoleResource>>> GetRoleResourcesByProvider(string? providerCode = null, IEnumerable<string>? resourceIds = null, CancellationToken ct = default)
    {
        var roleResources = await db.RoleResources
            .AsNoTracking()
            .Include(rr => rr.Role)
            .Include(rr => rr.Resource)
            .ThenInclude(res => res.Provider)
            .WhereIf(providerCode != null, rr => rr.Resource.Provider.Code == providerCode)
            .WhereIf(resourceIds != null, rr => resourceIds.Contains(rr.Resource.RefId))
            .ToListAsync(ct);

        return roleResources.GroupBy(rr => rr.RoleId)
            .ToDictionary(g => g.Key, g => g.AsEnumerable());
    }

    public async Task<Dictionary<Guid, IEnumerable<PackageResource>>> GetPackageResourcesByProvider(string? providerCode = null, IEnumerable<string>? resourceIds = null, CancellationToken ct = default)
    {
        /*
        var options = new AuditValues(SystemEntityConstants.ResourceRegistryImportSystem);
        using var scope = _serviceProvider.CreateEFScope(options);
        var dbContext = scope.ServiceProvider.GetService<AppDbContext>();
        var adminPackageResource = new PackageResource
        {
            PackageId = Guid.Parse("0195efb8-7c80-7a95-ad36-900c3d8ad300"),
            ResourceId = Guid.Parse("019a7eac-c384-73eb-b8ac-4209c0c43c38"),
            ////Audit_ValidFrom = DateTime.UtcNow,
            ////Audit_ChangedBy = Guid.Parse("14fd92db-c124-4208-ba62-293cbabff2ad"),
            ////Audit_ChangedBySystem = Guid.Parse("14fd92db-c124-4208-ba62-293cbabff2ad"),
            ////Audit_ChangeOperation = "019a7eac-bd7b-7a00-975a-c85e85ca342d"
        };

        var HAdminPackageResource = new PackageResource
        {
            PackageId = Guid.Parse("0195efb8-7c80-7e16-ab0c-36dc8ab1a29d"),
            ResourceId = Guid.Parse("019a7eac-c384-73eb-b8ac-4209c0c43c38"),
            ////Audit_ValidFrom = DateTime.UtcNow,
            ////Audit_ChangedBy = Guid.Parse("14fd92db-c124-4208-ba62-293cbabff2ad"),
            ////Audit_ChangedBySystem = Guid.Parse("14fd92db-c124-4208-ba62-293cbabff2ad"),
            ////Audit_ChangeOperation = "019a7eac-bd7b-7a00-975a-c85e85ca342d"
        };

        dbContext.PackageResources.Add(adminPackageResource);
        dbContext.PackageResources.Add(HAdminPackageResource);
        await dbContext.SaveChangesAsync(ct);
        */

        var packageResources = await db.PackageResources
            .AsNoTracking()
            .Include(pr => pr.Resource)
            .ThenInclude(res => res.Provider)
            .WhereIf(providerCode != null, pr => pr.Resource.Provider.Code == providerCode)
            .WhereIf(resourceIds != null, pr => resourceIds.Contains(pr.Resource.RefId))
            .ToListAsync(ct);

        return packageResources.GroupBy(pr => pr.PackageId)
            .ToDictionary(g => g.Key, g => g.AsEnumerable());
    }
}
