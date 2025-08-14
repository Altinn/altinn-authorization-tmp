using Altinn.AccessManagement.Api.Enduser.Services.Models;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessManagement.Enduser.Services;

public class NewRelationService(AppDbContext dbContext)
{
    /// <inheritdoc />
    public async Task<IEnumerable<RelationDto>> GetConnectionsToOthers(Guid partyId, Guid? toId = null, Guid? roleId = null, Guid? packageId = null, Guid? resourceId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Relations.AsNoTracking().Where(t => t.FromId == partyId);

        if (toId.HasValue)
        {
            query.Where(t => t.ToId == toId.Value);
        }

        if (roleId.HasValue)
        {
            query.Where(t => t.RoleId == roleId.Value);
        }

        if (packageId.HasValue)
        {
            query.Where(t => t.PackageId == packageId.Value);
        }

        if (resourceId.HasValue)
        {
            query.Where(t => t.ResourceId == resourceId.Value);
        }

        var res = await query.ToListAsync(cancellationToken);

        return ExtractConnectionsToOthers(res, includeSubConnections: false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RelationDto>> GetConnectionsToOthers(Guid partyId, Guid? toId = null, Guid? roleId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Relations.AsNoTracking().Where(t => t.FromId == partyId);

        if (toId.HasValue)
        {
            query.Where(t => t.ToId == toId.Value);
        }

        if (roleId.HasValue)
        {
            query.Where(t => t.RoleId == roleId.Value);
        }


        var res = await query.ToListAsync(cancellationToken);
        return ExtractConnectionsToOthers(res, includeSubConnections: false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RelationDto>> GetConnectionsFromOthers(Guid partyId, Guid? fromId = null, Guid? roleId = null, Guid? packageId = null, Guid? resourceId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Relations.AsNoTracking().Where(t => t.ToId == partyId);

        if (fromId.HasValue)
        {
            query.Where(t => t.FromId == fromId.Value);
        }

        if (roleId.HasValue)
        {
            query.Where(t => t.RoleId == roleId.Value);
        }

        if (packageId.HasValue)
        {
            query.Where(t => t.PackageId == packageId.Value);
        }

        if (resourceId.HasValue)
        {
            query.Where(t => t.ResourceId == resourceId.Value);
        }

        var res = await query.ToListAsync(cancellationToken);

        return ExtractConnectionsToOthers(res, includeSubConnections: false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RelationDto>> GetConnectionsFromOthers(Guid partyId, Guid? fromId = null, Guid? roleId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Relations.AsNoTracking().Where(t => t.ToId == partyId);

        if (fromId.HasValue)
        {
            query.Where(t => t.FromId == fromId.Value);
        }

        if (roleId.HasValue)
        {
            query.Where(t => t.RoleId == roleId.Value);
        }

        var res = await query.ToListAsync(cancellationToken);

        return ExtractConnectionsToOthers(res, includeSubConnections: false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PackagePermission>> GetPackagePermissionsFromOthers(Guid partyId, Guid? fromId = null, Guid? packageId = null, CancellationToken cancellationToken = default)
    {
        var filter = relationPermissionRepository.CreateFilterBuilder();
        filter.Equal(t => t.ToId, partyId);

        if (fromId.HasValue)
        {
            filter.Equal(t => t.FromId, fromId.Value);
        }

        if (packageId.HasValue)
        {
            filter.Equal(t => t.PackageId, packageId.Value);
        }

        var res = await relationPermissionRepository.GetExtended(filter, cancellationToken: cancellationToken);

        if (res is { } && res.Any() && res.Where(r => r.Package is { }) is var packages)
        {
            return packages.DistinctBy(t => t.Package.Id).Select(permission => new PackagePermission()
            {
                Package = permission.Package,
                Permissions = packages.Where(t => t.Package.Id == permission.Package.Id).Select(ConvertToPermission)
            });
        }

        return [];
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PackagePermission>> GetPackagePermissionsToOthers(Guid partyId, Guid? toId = null, Guid? packageId = null, CancellationToken cancellationToken = default)
    {
        var filter = relationPermissionRepository.CreateFilterBuilder();
        filter.Equal(t => t.FromId, partyId);

        if (toId.HasValue)
        {
            filter.Equal(t => t.ToId, toId.Value);
        }

        if (packageId.HasValue)
        {
            filter.Equal(t => t.PackageId, packageId.Value);
        }

        var res = await relationPermissionRepository.GetExtended(filter, cancellationToken: cancellationToken);
        if (res is { } && res.Any() && res.Where(r => r.Package is { }) is var packages)
        {
            return packages.DistinctBy(t => t.Package.Id).Select(permission => new PackagePermission()
            {
                Package = permission.Package,
                Permissions = packages.Where(t => t.Package.Id == permission.Package.Id).Select(ConvertToPermission)
            });
        }

        return [];
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ResourcePermission>> GetResourcePermissionsFromOthers(Guid partyId, Guid? fromId = null, Guid? packageId = null, Guid? resourceId = null, CancellationToken cancellationToken = default)
    {
        var filter = relationPermissionRepository.CreateFilterBuilder();
        filter.Equal(t => t.ToId, partyId);

        if (fromId.HasValue)
        {
            filter.Equal(t => t.FromId, fromId.Value);
        }

        if (packageId.HasValue)
        {
            filter.Equal(t => t.PackageId, packageId.Value);
        }

        if (resourceId.HasValue)
        {
            filter.Equal(t => t.ResourceId, resourceId.Value);
        }

        var res = await relationPermissionRepository.GetExtended(filter, cancellationToken: cancellationToken);

        if (res is { } && res.Any() && res.Where(r => r.Resource is { } && r.Package is { }) is var resources)
        {
            return resources.DistinctBy(t => t.Resource.Id).Select(permission => new ResourcePermission()
            {
                Resource = permission.Resource,
                Permissions = resources.Where(t => t.Package.Id == permission.Package.Id).Select(ConvertToPermission)
            });
        }

        return [];
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PackageDelegationCheck>> GetAssignablePackagePermissions(Guid partyId, Guid fromId, IEnumerable<Guid>? packageIds = null, CancellationToken cancellationToken = default)
    {
        return await relationPermissionRepository.GetAssignableAccessPackages(fromId, partyId, packageIds, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ResourcePermission>> GetResourcePermissionsToOthers(Guid partyId, Guid? toId = null, Guid? packageId = null, Guid? resourceId = null, CancellationToken cancellationToken = default)
    {
        var filter = relationPermissionRepository.CreateFilterBuilder();
        filter.Equal(t => t.FromId, partyId);

        if (toId.HasValue)
        {
            filter.Equal(t => t.ToId, toId.Value);
        }

        if (packageId.HasValue)
        {
            filter.Equal(t => t.PackageId, packageId.Value);
        }

        if (resourceId.HasValue)
        {
            filter.Equal(t => t.ResourceId, resourceId.Value);
        }

        var res = await relationPermissionRepository.GetExtended(filter, cancellationToken: cancellationToken);

        if (res is { } && res.Any() && res.Where(r => r.Resource is { } && r.Package is { }) is var resources)
        {
            return resources.DistinctBy(t => t.Resource.Id).Select(permission => new ResourcePermission()
            {
                Resource = permission.Resource,
                Permissions = resources.Where(t => t.Package.Id == permission.Package.Id).Select(ConvertToPermission)
            });
        }

        return [];
    }

    #region Extractors and Converters

    private IEnumerable<RelationDto> ExtractConnectionsToOthers(IEnumerable<Relation> res, bool includeSubConnections = false)
    {
        return res.Where(t => t.Reason == "Direct").DistinctBy(t => t.To.Id).Select(relation => new RelationDto()
        {
            Party = relation.To,
            Roles = res.Where(t => t.To.Id == relation.To.Id).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.To.Id == relation.To.Id && t.Package != null).Select(t => t.Package).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubConnectionToOthers(res, relation.To.Id).ToList() : new()
        });
    }

    private IEnumerable<RelationDto> ExtractSubConnectionToOthers(IEnumerable<Relation> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.Via.Id == party).DistinctBy(t => t.To.Id).Select(relation => new RelationDto()
        {
            Party = relation.To,
            Roles = res.Where(t => t.To.Id == relation.To.Id).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.To.Id == relation.To.Id && t.Package != null).Select(t => t.Package).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }

    //private IEnumerable<RelationDto> ExtractConnectionsToOthers(IEnumerable<Relation> res, bool includeSubConnections = false)
    //{
    //    return res.Where(t => t.Reason == "Direct").DistinctBy(t => t.To.Id).Select(relation => new RelationDto()
    //    {
    //        Party = relation.To,
    //        Roles = res.Where(t => t.To.Id == relation.To.Id).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
    //        Connections = includeSubConnections ? ExtractSubConnectionToOthers(res, relation.To.Id).ToList() : new()
    //    });
    //}

    //private IEnumerable<RelationDto> ExtractSubConnectionToOthers(IEnumerable<Relation> res, Guid party)
    //{
    //    return res.Where(t => t.Reason != "Direct" && t.Via.Id == party).DistinctBy(t => t.To.Id).Select(relation => new RelationDto()
    //    {
    //        Party = relation.To,
    //        Roles = res.Where(t => t.To.Id == relation.To.Id).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
    //        Connections = new()
    //    });
    //}

    //private IEnumerable<CompactRelationDto> ExtractConnectionsFromOthers(IEnumerable<ExtCompactRelation> res, bool includeSubConnections = false)
    //{
    //    return res.DistinctBy(t => t.From.Id).Select(relation => new CompactRelationDto()
    //    {
    //        Party = relation.From,
    //        Roles = res.Where(t => t.From.Id == relation.From.Id).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
    //        Connections = includeSubConnections ? ExtractSubConnectionFromOthers(res, relation.From.Id).ToList() : new()
    //    });
    //}

    //private IEnumerable<RelationDto> ExtractConnectionsFromOthers(IEnumerable<ExtRelation> res, bool includeSubConnections = false)
    //{
    //    return res.DistinctBy(t => t.From.Id).Select(relation => new RelationDto()
    //    {
    //        Party = relation.From,
    //        Roles = res.Where(t => t.From.Id == relation.From.Id).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
    //        Packages = res.Where(t => t.From.Id == relation.From.Id && t.Package != null).Select(t => t.Package).DistinctBy(t => t.Id).ToList(),
    //        Connections = includeSubConnections ? ExtractSubConnectionFromOthers(res, relation.From.Id).ToList() : new()
    //    });
    //}

    //private IEnumerable<CompactRelationDto> ExtractSubConnectionFromOthers(IEnumerable<ExtCompactRelation> res, Guid party)
    //{
    //    return res.Where(t => t.Reason != "Direct" && t.Via.Id == party).DistinctBy(t => t.From.Id).Select(relation => new CompactRelationDto()
    //    {
    //        Party = relation.From,
    //        Roles = res.Where(t => t.From.Id == relation.From.Id).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
    //        Connections = new()
    //    });
    //}

    //private IEnumerable<RelationDto> ExtractSubConnectionFromOthers(IEnumerable<ExtRelation> res, Guid party)
    //{
    //    return res.Where(t => t.Reason != "Direct" && t.Via.Id == party).DistinctBy(t => t.From.Id).Select(relation => new RelationDto()
    //    {
    //        Party = relation.From,
    //        Roles = res.Where(t => t.From.Id == relation.From.Id).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
    //        Packages = res.Where(t => t.From.Id == relation.From.Id && t.Package != null).Select(t => t.Package).DistinctBy(t => t.Id).ToList(),
    //        Connections = new()
    //    });
    //}

    //private CompactPermission ConvertToCompactPermission(ExtRelation connection)
    //{
    //    return new CompactPermission()
    //    {
    //        From = connection.From,
    //        To = connection.To
    //    };
    //}

    //private Permission ConvertToPermission(ExtRelation connection)
    //{
    //    return new Permission()
    //    {
    //        From = connection.From,
    //        To = connection.To,
    //        Via = connection.Via,
    //        ViaRole = connection.ViaRole,
    //        Role = connection.Role
    //    };
    //}

    #endregion
}
