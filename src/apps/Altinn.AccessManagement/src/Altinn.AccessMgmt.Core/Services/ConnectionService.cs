using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc />
public class ConnectionService(AppDbContext dbContext) : IConnectionService
{
    /// <inheritdoc />
    public async Task<IEnumerable<BasicConnectionDto>> GetConnectionsToOthers(Guid? fromId = null, Guid? viaId = null, Guid? toId = null, CancellationToken cancellationToken = default)
    {
        if (!fromId.HasValue && !toId.HasValue && !viaId.HasValue)
        {
            throw new ArgumentNullException();
        }

        var result = await dbContext.Connections.AsNoTracking()
            .WhereIf(fromId.HasValue, t => t.FromId == fromId.Value)
            .WhereIf(toId.HasValue, t => t.ToId == toId.Value)
            .WhereIf(viaId.HasValue, t => t.ViaId == viaId.Value)
            .ToListAsync(cancellationToken);

        return ExtractBasicConnectionDto(result);
    }

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
                Package = DtoMapper.ConvertCompactPackage(permission.Package),
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
                Package = DtoMapper.ConvertCompactPackage(permission.Package),
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

    public async Task<IEnumerable<Package>> GetConnectionPackages(Guid fromId, Guid toId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Connections.AsNoTracking().Where(t => t.FromId == fromId && t.ToId == toId).Include(t => t.Package).Select(t => t.Package).ToListAsync(cancellationToken);
    }

    private async Task<Assignment> GetAssignment(Guid fromId, Guid toId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Assignments.AsNoTracking().Where(t => t.FromId == fromId && t.ToId == toId && t.RoleId == roleId).FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<IEnumerable<Assignment>> GetAssignment(Guid fromId, Guid toId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Assignments.AsNoTracking().Where(t => t.FromId == fromId && t.ToId == toId).ToListAsync(cancellationToken);
    }

    private async Task<Delegation> GetDelegation(Guid fromId, Guid toId, Guid roleId, Guid viaRoleId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Delegations.AsNoTracking().Where(t => t.From.FromId == fromId && t.To.ToId == toId && t.From.RoleId == viaRoleId && t.To.RoleId == roleId).FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<IEnumerable<Delegation>> GetDelegation(Guid fromId, Guid toId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Delegations.AsNoTracking().Where(t => t.From.FromId == fromId && t.To.ToId == toId).ToListAsync(cancellationToken);
    }

    public async Task<DelegationPackage> GetOrAddPackage(Guid partyId, Guid fromId, Guid toId, Guid roleId, Guid viaId, Guid viaRoleId, Guid packageId, CancellationToken cancellationToken = default)
    {
        var delegations = await GetDelegation(fromId, toId);
        if (delegations == null || !delegations.Any())
        {
            throw new Exception("Delegation not found");
        }

        var delegation = delegations.FirstOrDefault(t => t.From.RoleId == viaRoleId && t.To.RoleId == roleId);
        if (delegation == null)
        {
            throw new Exception("Delegation not found");
        }

        var assignmentPackages = await dbContext.AssignmentPackages.AsNoTracking().Where(t => t.AssignmentId == delegation.FromId).ToListAsync();
        var assignmentPackage = assignmentPackages.FirstOrDefault(t => t.Id.Equals(packageId));
        if (assignmentPackage == null)
        {
            throw new Exception("Assignment does not have the package assigned on this entity");
        }

        if (!assignmentPackage.Package.IsDelegable)
        {
            throw new Exception("Package is not delegable");
        }

        var delegationPackage = await dbContext.DelegationPackages.Where(t => t.DelegationId == delegation.Id && t.PackageId == packageId).FirstOrDefaultAsync(cancellationToken);
        if (delegationPackage == null)
        {
            delegationPackage = new DelegationPackage() { DelegationId = delegation.Id, PackageId = packageId };
            dbContext.DelegationPackages.Add(delegationPackage);
            var res = await dbContext.SaveChangesAsync(new AuditValues(partyId, AuditDefaults.InternalApi, Guid.NewGuid().ToString()));
            if (res == 0)
            {
                throw new Exception("Unable to add package to delegation");
            }
        }

        return delegationPackage;
    }

    public async Task<AssignmentPackage> GetOrAddPackage(Guid partyId, Guid fromId, Guid toId, Guid roleId, Guid packageId, CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignment(fromId, toId, roleId, cancellationToken);
        if (assignment == null)
        {
            throw new Exception("Assignment not found");
        }

        var userPackages = await GetConnectionPackages(assignment.FromId, partyId, cancellationToken: cancellationToken);
        var userPackage = userPackages.FirstOrDefault(t => t.Id.Equals(packageId));

        if (userPackage == null)
        {
            throw new Exception("User does not have the package assigned on this entity");
        }

        // if (!userPackage.CanAssign)
        // {
        //     throw new Exception("User can't assign package");
        // }

        if (!userPackage.IsAssignable)
        {
            throw new Exception("Package is not assignable");
        }

        var assignmentPackage = await dbContext.AssignmentPackages.Where(t => t.AssignmentId == assignment.Id && t.PackageId == packageId).FirstOrDefaultAsync(cancellationToken);
        if (assignmentPackage == null)
        {
            assignmentPackage = new AssignmentPackage() { AssignmentId = assignment.Id, PackageId = packageId };
            dbContext.AssignmentPackages.Add(assignmentPackage);
            var res = await dbContext.SaveChangesAsync(new AuditValues(partyId, AuditDefaults.InternalApi, Guid.NewGuid().ToString()));
            if (res == 0)
            {
                throw new Exception("Unable to add package to assignment");
            }
        }

        return assignmentPackage;
    }

    #region Mappers
    private IEnumerable<ConnectionPackageDto> ExtractRelationPackageDtoToOthers(IEnumerable<Connection> res, bool includeSubConnections = false)
    {
        return res.Where(t => t.Reason == "Direct").DistinctBy(t => t.ToId).Select(relation => new ConnectionPackageDto()
        {
            Party = DtoMapper.Convert(relation.To),
            Roles = res.Where(t => t.ToId == relation.ToId).Select(t => DtoMapper.ConvertCompactRole(t.Role)).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.ToId == relation.ToId && t.Package != null).Select(t => DtoMapper.ConvertCompactPackage(t.Package)).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoToOthers(res, relation.ToId).ToList() : new()
        });
    }
    
    private IEnumerable<ConnectionPackageDto> ExtractSubRelationPackageDtoFromOthers(IEnumerable<Connection> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.ViaId == party).DistinctBy(t => t.FromId).Select(relation => new ConnectionPackageDto()
        {
            Party = DtoMapper.Convert(relation.From),
            Roles = res.Where(t => t.FromId == relation.FromId).Select(t => DtoMapper.ConvertCompactRole(t.Role)).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.FromId == relation.FromId && t.Package != null).Select(t => DtoMapper.ConvertCompactPackage(t.Package)).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }

    private IEnumerable<BasicConnectionDto> ExtractBasicConnectionDto(IEnumerable<Connection> res)
    {
        return res.Select(t => new BasicConnectionDto()
        {
            From = t.From,
            Via = t.Via,
            To = t.To,
            Role = t.Role,
            ViaRole = t.ViaRole
        });
    }

    private IEnumerable<ConnectionDto> ExtractSubRelationDtoFromOthers(IEnumerable<Connection> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.ViaId == party).DistinctBy(t => t.FromId).Select(relation => new ConnectionDto()
        {
            Party = DtoMapper.Convert(relation.From),
            Roles = res.Where(t => t.FromId == relation.FromId).Select(t => DtoMapper.ConvertCompactRole(t.Role)).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }
    
    private IEnumerable<ConnectionDto> ExtractRelationDtoToOthers(IEnumerable<Connection> res, bool includeSubConnections = false)
    {
        return res.Where(t => t.Reason == "Direct").DistinctBy(t => t.ToId).Select(relation => new ConnectionDto()
        {
            Party = DtoMapper.Convert(relation.To),
            Roles = res.Where(t => t.ToId == relation.ToId).Select(t => DtoMapper.ConvertCompactRole(t.Role)).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoToOthers(res, relation.ToId).ToList() : new()
        });
    }
    
    private IEnumerable<ConnectionDto> ExtractSubRelationDtoToOthers(IEnumerable<Connection> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.ViaId == party).DistinctBy(t => t.To.Id).Select(relation => new ConnectionDto()
        {
            Party = DtoMapper.Convert(relation.To),
            Roles = res.Where(t => t.ToId == relation.To.Id).Select(t => DtoMapper.ConvertCompactRole(t.Role)).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }
    
    private IEnumerable<ConnectionPackageDto> ExtractSubRelationPackageDtoToOthers(IEnumerable<Connection> res, Guid party)
    {
        return res.Where(t => t.Reason != "Direct" && t.ViaId == party).DistinctBy(t => t.ToId).Select(relation => new ConnectionPackageDto()
        {
            Party = DtoMapper.Convert(relation.To),
            Roles = res.Where(t => t.ToId == relation.ToId).Select(t => DtoMapper.ConvertCompactRole(t.Role)).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.ToId == relation.ToId && t.Package != null).Select(t => DtoMapper.ConvertCompactPackage(t.Package)).DistinctBy(t => t.Id).ToList(),
            Connections = new()
        });
    }

    private IEnumerable<ConnectionDto> ExtractRelationDtoFromOthers(IEnumerable<Connection> res, bool includeSubConnections = false)
    {
        return res.DistinctBy(t => t.FromId).Select(relation => new ConnectionDto()
        {
            Party = DtoMapper.Convert(relation.From),
            Roles = res.Where(t => t.FromId == relation.FromId).Select(t => DtoMapper.ConvertCompactRole(t.Role)).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoFromOthers(res, relation.FromId).ToList() : new()
        });
    }

    private IEnumerable<ConnectionPackageDto> ExtractRelationPackageDtoFromOthers(IEnumerable<Connection> res, bool includeSubConnections = false)
    {
        return res.DistinctBy(t => t.FromId).Select(relation => new ConnectionPackageDto()
        {
            Party = DtoMapper.Convert(relation.From),
            Roles = res.Where(t => t.FromId == relation.FromId).Select(t => DtoMapper.ConvertCompactRole(t.Role)).DistinctBy(t => t.Id).ToList(),
            Packages = res.Where(t => t.FromId == relation.FromId && t.Package != null).Select(t => DtoMapper.ConvertCompactPackage(t.Package)).DistinctBy(t => t.Id).ToList(),
            Connections = includeSubConnections ? ExtractSubRelationDtoFromOthers(res, relation.FromId).ToList() : new()
        });
    }
    #endregion
}
