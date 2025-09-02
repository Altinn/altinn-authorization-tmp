﻿using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc />
public class ConnectionService(AppDbContext dbContext, DtoMapper dtoConverter) : IConnectionService
{
    /// <inheritdoc />
    public async Task<IEnumerable<ConnectionPackageDto>> GetConnectionsToOthers(Guid partyId, Guid? toId = null, Guid? roleId = null, Guid? packageId = null, Guid? resourceId = null, CancellationToken cancellationToken = default)
    {
        var result = await dbContext.Connections.AsNoTracking()
            .Where(t => t.FromId == partyId)
            .WhereIf(toId.HasValue, t => t.ToId == toId.Value)
            .WhereIf(roleId.HasValue, t => t.RoleId == roleId.Value)
            .WhereIf(packageId.HasValue, t => t.PackageId == packageId.Value)
            .WhereIf(resourceId.HasValue, t => t.ResourceId == resourceId.Value)
            .ToListAsync(cancellationToken);

        return ExtractRelationPackageDtoToOthers(result, includeSubConnections: false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ConnectionDto>> GetConnectionsToOthers(Guid partyId, Guid? toId = null, Guid? roleId = null, CancellationToken cancellationToken = default)
    {
        var result = await dbContext.Connections.AsNoTracking()
            .Where(t => t.FromId == partyId)
            .WhereIf(toId.HasValue, t => t.ToId == toId.Value)
            .WhereIf(roleId.HasValue, t => t.RoleId == roleId.Value)
            .ToListAsync(cancellationToken);

        return ExtractRelationDtoToOthers(result, includeSubConnections: false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ConnectionPackageDto>> GetConnectionsFromOthers(Guid partyId, Guid? fromId = null, Guid? roleId = null, Guid? packageId = null, Guid? resourceId = null, CancellationToken cancellationToken = default)
    {
        var result = await dbContext.Connections.AsNoTracking()
            .Where(t => t.ToId == partyId)
            .WhereIf(fromId.HasValue, t => t.FromId == fromId.Value)
            .WhereIf(roleId.HasValue, t => t.RoleId == roleId.Value)
            .WhereIf(packageId.HasValue, t => t.PackageId == packageId.Value)
            .WhereIf(resourceId.HasValue, t => t.ResourceId == resourceId.Value)
            .ToListAsync(cancellationToken);

        return ExtractRelationPackageDtoFromOthers(result, includeSubConnections: false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ConnectionDto>> GetConnectionsFromOthers(Guid partyId, Guid? fromId = null, Guid? roleId = null, CancellationToken cancellationToken = default)
    {
        var result = await dbContext.Connections.AsNoTracking()
            .Where(t => t.ToId == partyId)
            .WhereIf(fromId.HasValue, t => t.FromId == fromId.Value)
            .WhereIf(roleId.HasValue, t => t.RoleId == roleId.Value)
            .ToListAsync(cancellationToken);

        return ExtractRelationDtoFromOthers(result, includeSubConnections: false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PackagePermissionDto>> GetPackagePermissionsFromOthers(Guid partyId, Guid? fromId = null, Guid? packageId = null, CancellationToken cancellationToken = default)
    {
        var result = await dbContext.Connections
            .AsNoTracking()
            .Include(t => t.Package)
            .Where(t => t.ToId == partyId)
            .WhereIf(fromId.HasValue, t => t.FromId == fromId.Value)
            .WhereIf(packageId.HasValue, t => t.PackageId == packageId.Value)
            .ToListAsync(cancellationToken);

        if (result is { } && result.Any() && result.Where(r => r.Package is { }) is var packages)
        {
            return packages.DistinctBy(t => t.Package.Id).Select(permission => new PackagePermissionDto()
            {
                Package = permission.Package,
                Permissions = packages.Where(t => t.Package.Id == permission.Package.Id).Select(DtoMapper.ConvertToPermission)
            });
        }

        return [];
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PackagePermissionDto>> GetPackagePermissionsToOthers(Guid partyId, Guid? toId = null, Guid? packageId = null, CancellationToken cancellationToken = default)
    {
        var result = await dbContext.Connections
            .AsNoTracking()
            .Include(t => t.Package)
            .Where(t => t.FromId == partyId)
            .WhereIf(toId.HasValue, t => t.ToId == toId.Value)
            .WhereIf(packageId.HasValue, t => t.PackageId == packageId.Value)
            .ToListAsync(cancellationToken);

        if (result is { } && result.Any() && result.Where(r => r.Package is { }) is var packages)
        {
            return packages.DistinctBy(t => t.Package.Id).Select(permission => new PackagePermissionDto()
            {
                Package = permission.Package,
                Permissions = packages.Where(t => t.Package.Id == permission.Package.Id).Select(DtoMapper.ConvertToPermission)
            });
        }

        return [];
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ResourcePermission>> GetResourcePermissionsFromOthers(Guid partyId, Guid? fromId = null, Guid? packageId = null, Guid? resourceId = null, CancellationToken cancellationToken = default)
    {
        var result = await dbContext.Connections
            .AsNoTracking()
            .Include(t => t.Package)
            .Include(t => t.Resource)
            .Where(t => t.ToId == partyId)
            .WhereIf(fromId.HasValue, t => t.FromId == fromId.Value)
            .WhereIf(packageId.HasValue, t => t.PackageId == packageId.Value)
            .WhereIf(resourceId.HasValue, t => t.ResourceId == resourceId.Value)
            .ToListAsync(cancellationToken);

        if (result is { } && result.Any() && result.Where(r => r.Package is { }) is var packages)
        {
            return packages.DistinctBy(t => t.Resource.Id).Select(permission => new ResourcePermission()
            {
                Resource = permission.Resource,
                Permissions = packages.Where(t => t.Resource.Id == permission.Resource.Id).Select(DtoMapper.ConvertToPermission)
            });
        }

        return [];
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ResourcePermission>> GetResourcePermissionsToOthers(Guid partyId, Guid? toId = null, Guid? packageId = null, Guid? resourceId = null, CancellationToken cancellationToken = default)
    {
        var result = await dbContext.Connections
            .AsNoTracking()
            .Include(t => t.Package)
            .Include(t => t.Resource)
            .Where(t => t.FromId == partyId)
            .WhereIf(toId.HasValue, t => t.ToId == toId.Value)
            .WhereIf(packageId.HasValue, t => t.PackageId == packageId.Value)
            .WhereIf(resourceId.HasValue, t => t.ResourceId == resourceId.Value)
            .ToListAsync(cancellationToken);

        if (result is { } && result.Any() && result.Where(r => r.Package is { }) is var packages)
        {
            return packages.DistinctBy(t => t.Resource.Id).Select(permission => new ResourcePermission()
            {
                Resource = permission.Resource,
                Permissions = packages.Where(t => t.Resource.Id == permission.Resource.Id).Select(DtoMapper.ConvertToPermission)
            });
        }

        return [];
    }

    #region Mappers
    private IEnumerable<ConnectionPackageDto> ExtractRelationPackageDtoToOthers(IEnumerable<Connection> res, bool includeSubConnections = false)
    {
        return res.Where(t => t.Reason == "Direct").DistinctBy(t => t.ToId).Select(relation => new ConnectionPackageDto()
        {
            Party = relation.To,
            Roles = res.Where(t => t.ToId == relation.ToId).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.ToId == relation.ToId && t.Package != null).Select(t => t.Package).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoToOthers(res, relation.ToId).ToList() : new()
        });
    }
    
    private IEnumerable<ConnectionPackageDto> ExtractSubRelationPackageDtoFromOthers(IEnumerable<Connection> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.ViaId == party).DistinctBy(t => t.FromId).Select(relation => new ConnectionPackageDto()
        {
            Party = relation.From,
            Roles = res.Where(t => t.FromId == relation.FromId).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.FromId == relation.FromId && t.Package != null).Select(t => t.Package).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }
    
    private IEnumerable<ConnectionDto> ExtractSubRelationDtoFromOthers(IEnumerable<Connection> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.ViaId == party).DistinctBy(t => t.FromId).Select(relation => new ConnectionDto()
        {
            Party = relation.From,
            Roles = res.Where(t => t.FromId == relation.FromId).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }
    
    private IEnumerable<ConnectionDto> ExtractRelationDtoToOthers(IEnumerable<Connection> res, bool includeSubConnections = false)
    {
        return res.Where(t => t.Reason == "Direct").DistinctBy(t => t.ToId).Select(relation => new ConnectionDto()
        {
            Party = relation.To,
            Roles = res.Where(t => t.ToId == relation.ToId).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoToOthers(res, relation.ToId).ToList() : new()
        });
    }
    
    private IEnumerable<ConnectionDto> ExtractSubRelationDtoToOthers(IEnumerable<Connection> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.ViaId == party).DistinctBy(t => t.To.Id).Select(relation => new ConnectionDto()
        {
            Party = relation.To,
            Roles = res.Where(t => t.ToId == relation.To.Id).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }
    
    private IEnumerable<ConnectionPackageDto> ExtractSubRelationPackageDtoToOthers(IEnumerable<Connection> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.ViaId == party).DistinctBy(t => t.ToId).Select(relation => new ConnectionPackageDto()
        {
            Party = relation.To,
            Roles = res.Where(t => t.ToId == relation.ToId).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.ToId == relation.ToId && t.Package != null).Select(t => t.Package).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }

    private IEnumerable<ConnectionDto> ExtractRelationDtoFromOthers(IEnumerable<Connection> res, bool includeSubConnections = false)
    {
        return res.DistinctBy(t => t.FromId).Select(relation => new ConnectionDto()
        {
            Party = relation.From,
            Roles = res.Where(t => t.FromId == relation.FromId).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoFromOthers(res, relation.FromId).ToList() : new()
        });
    }

    private IEnumerable<ConnectionPackageDto> ExtractRelationPackageDtoFromOthers(IEnumerable<Connection> res, bool includeSubConnections = false)
    {
        return res.DistinctBy(t => t.FromId).Select(relation => new ConnectionPackageDto()
        {
            Party = relation.From,
            Roles = res.Where(t => t.FromId == relation.FromId).Select(t => t.Role).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.FromId == relation.FromId && t.Package != null).Select(t => t.Package).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoFromOthers(res, relation.FromId).ToList() : new()
        });
    }
    #endregion
}
