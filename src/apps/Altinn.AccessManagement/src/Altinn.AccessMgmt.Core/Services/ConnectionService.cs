﻿using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.Core.Validation;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc />
public partial class ConnectionService(AppDbContext dbContext) : IConnectionService
{
    public async Task<Result<List<ConnectionDto>>> Get(Guid? fromId = null, Guid? toId = null, CancellationToken cancellationToken = default)
    {
        if (fromId is { } && fromId.Value != Guid.Empty)
        {
            var result = await GetConnectionsToOthers(fromId.Value, toId is { } && toId != Guid.Empty ? toId : null, null, cancellationToken: cancellationToken);
            return result.ToList();
        }

        if (toId is { } && toId != Guid.Empty)
        {
            var result = await GetConnectionsFromOthers(toId.Value, fromId is { } && fromId.Value != Guid.Empty ? fromId : null, null, cancellationToken: cancellationToken);
            return result.ToList();
        }

        return new List<ConnectionDto>();
    }

    public async Task<Result<AssignmentDto>> AddAssignment(Guid fromId, Guid toId, Role role, Action<ConnectionOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(role);
        var options = new ConnectionOptions(configureOptions);
        var entities = await dbContext.Entities
            .AsNoTracking()
            .Where(e => e.Id == fromId || e.Id == toId)
            .Include(e => e.Type)
            .ToListAsync(cancellationToken);

        var fromEntity = entities.FirstOrDefault(e => e.Id == fromId);
        var toEntity = entities.FirstOrDefault(e => e.Id == toId);

        var problem = ValidateEntities(fromEntity, toEntity, options);
        if (problem is { })
        {
            return problem;
        }

        var existingAssignment = await dbContext.Assignments
            .AsNoTracking()
            .Where(e => e.FromId == fromId)
            .Where(e => e.ToId == toId)
            .Where(e => e.RoleId == role.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingAssignment is { })
        {
            return DtoMapper.Convert(existingAssignment);
        }

        var assignment = new Assignment()
        {
            FromId = fromId,
            ToId = toId,
            RoleId = role.Id,
        };

        await dbContext.Assignments.AddAsync(assignment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return DtoMapper.Convert(assignment);
    }

    public async Task<ValidationProblemInstance> RemoveAssignment(Guid fromId, Guid toId, Role role, bool cascade = false, Action<ConnectionOptions> configureConnectionOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(role);
        var options = new ConnectionOptions(configureConnectionOptions);
        var entities = await dbContext.Entities
            .AsNoTracking()
            .Where(e => e.Id == fromId || e.Id == toId)
            .Include(e => e.Type)
            .ToListAsync(cancellationToken);

        var fromEntity = entities.FirstOrDefault(e => e.Id == fromId);
        var toEntity = entities.FirstOrDefault(e => e.Id == toId);

        var problem = ValidateEntities(fromEntity, toEntity, options);
        if (problem is { })
        {
            return problem;
        }

        var existingAssignment = await dbContext.Assignments
            .AsTracking()
            .Where(e => e.FromId == fromId)
            .Where(e => e.ToId == toId)
            .Where(e => e.RoleId == role.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingAssignment is null)
        {
            return null;
        }

        if (!cascade)
        {
            var assignedPackages = await dbContext.AssignmentPackages
                .AsNoTracking()
                .Where(p => p.AssignmentId == existingAssignment.Id)
                .ToListAsync(cancellationToken);

            var delegationsFrom = await dbContext.Delegations
                .AsNoTracking()
                .Where(p => p.FromId == fromId)
                .ToListAsync(cancellationToken);

            var delegationsTo = await dbContext.Delegations
                .AsNoTracking()
                .Where(p => p.ToId == toId)
                .ToListAsync(cancellationToken);

            problem = ValidationComposer.Validate(
                AssignementPackageValidation.HasAssignedPackages(assignedPackages),
                DelegationValidation.HasDelegationsAssigned(delegationsFrom),
                DelegationValidation.HasDelegationsAssigned(delegationsTo)
            );

            if (problem is { })
            {
                return problem;
            }
        }

        dbContext.Remove(existingAssignment);
        await dbContext.SaveChangesAsync(cancellationToken);
        return null;
    }

    public async Task<Result<List<PackagePermissionDto>>> GetPackages(Guid? fromId, Guid? toId, CancellationToken cancellationToken = default)
    {
        if (fromId is { } && fromId.Value != Guid.Empty)
        {
            var result = await GetPackagePermissionsToOthers(fromId.Value, toId is { } && toId != Guid.Empty ? toId : null, null, cancellationToken);
            return result.ToList();
        }

        if (toId is { } && toId.Value != Guid.Empty)
        {
            var result = await GetPackagePermissionsFromOthers(toId.Value, fromId is { } && fromId.Value != Guid.Empty ? fromId : null, null, cancellationToken);
            return result.ToList();
        }

        return new List<PackagePermissionDto>();
    }

    public async Task<Result<AssignmentPackageDto>> AddPackage(Guid fromId, Guid toId, Role role, Guid packageId, Action<ConnectionOptions> configureConnectionOptions = null, CancellationToken cancellationToken = default)
    {
        return await AddPackage(fromId, toId, role, packageId, "packageId", configureConnectionOptions, cancellationToken);
    }

    public async Task<Result<AssignmentPackageDto>> AddPackage(Guid fromId, Guid toId, Role role, string packageUrn, Action<ConnectionOptions> configureConnectionOptions = null, CancellationToken cancellationToken = default)
    {
        packageUrn = (packageUrn.StartsWith("urn:", StringComparison.Ordinal) || packageUrn.StartsWith(':')) ? packageUrn : ":" + packageUrn;

        var package = await dbContext.Packages
            .AsNoTracking()
            .Where(p => p.Urn.EndsWith(packageUrn))
            .FirstOrDefaultAsync(cancellationToken);

        var problem = ValidationComposer.Validate(PackageValidation.PackageExists(package));
        if (problem is { })
        {
            return problem;
        }

        return await AddPackage(fromId, toId, role, package.Id, "package", configureConnectionOptions, cancellationToken);
    }

    public async Task<ValidationProblemInstance> RemovePackage(Guid fromId, Guid toId, Role role, string packageUrn, CancellationToken cancellationToken = default)
    {
        packageUrn = packageUrn.StartsWith("urn:", StringComparison.Ordinal) || packageUrn.StartsWith(':') ? packageUrn : ":" + packageUrn;
        if (PackageConstants.TryGetByUrn(packageUrn, out var result))
        {
            return await RemovePackage(fromId, toId, role, result, cancellationToken);
        }

        return ValidationComposer.Validate(PackageValidation.PackageUrnLookup([], [packageUrn]));
    }

    public async Task<ValidationProblemInstance> RemovePackage(Guid fromId, Guid toId, Role role, Guid packageId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(role);

        var assignment = await dbContext.Assignments
            .AsNoTracking()
            .Where(a => a.FromId == fromId)
            .Where(a => a.ToId == toId)
            .Where(a => a.RoleId == role.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (assignment is null)
        {
            return null;
        }

        var existingAssignmentPackages = await dbContext.AssignmentPackages
            .AsTracking()
            .Where(a => a.AssignmentId == assignment.Id)
            .Where(a => a.PackageId == packageId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingAssignmentPackages is null)
        {
            return null;
        }

        dbContext.Remove(assignment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return null;
    }

    private async Task<Result<AssignmentPackageDto>> AddPackage(Guid fromId, Guid toId, Role role, Guid packageId, string queryParamName, Action<ConnectionOptions> configureConnectionOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(role);
        var options = new ConnectionOptions(configureConnectionOptions);
        var entities = await dbContext.Entities
            .AsNoTracking()
            .Where(e => e.Id == fromId || e.Id == toId)
            .Include(e => e.Type)
            .ToListAsync(cancellationToken);

        var fromEntity = entities.FirstOrDefault(e => e.Id == fromId);
        var toEntity = entities.FirstOrDefault(e => e.Id == toId);

        var problem = ValidateEntities(fromEntity, toEntity, options);
        if (problem is { })
        {
            return problem;
        }

        var assignment = await dbContext.Assignments
                .AsNoTracking()
                .Where(a => a.FromId == fromId)
                .Where(a => a.ToId == toId)
                .Where(a => a.RoleId == role.Id)
                .FirstOrDefaultAsync(cancellationToken);

        if (assignment is null)
        {
            var result = await AddAssignment(fromId, toId, RoleConstants.Rightholder, configureConnectionOptions, cancellationToken);
            if (result.IsProblem)
            {
                return result.Problem;
            }

            assignment = await dbContext.Assignments
                .FirstOrDefaultAsync(a => a.Id == result.Value.Id, cancellationToken: cancellationToken);

            if (assignment is null)
            {
                return Problems.ConsentCantBeAccepted;
            }
        }

        var check = await CheckPackage(fromId, packageIds: [packageId], cancellationToken);
        if (check.IsProblem)
        {
            return check.Problem;
        }

        problem = ValidationComposer.Validate(
            PackageValidation.AuthorizePackageAssignment(check.Value),
            PackageValidation.PackageIsAssignableToRecipient(check.Value.Select(p => p.Package.Urn), toEntity.Type, queryParamName)
        );

        if (problem is { })
        {
            return problem;
        }

        var existingAssignmentPackage = await dbContext.AssignmentPackages
            .AsNoTracking()
            .Where(a => a.AssignmentId == assignment.Id)
            .Where(a => a.PackageId == packageId)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingAssignmentPackage is { })
        {
            return DtoMapper.Convert(existingAssignmentPackage);
        }

        existingAssignmentPackage = new AssignmentPackage()
        {
            AssignmentId = assignment.Id,
            PackageId = packageId,
        };

        await dbContext.AssignmentPackages.AddAsync(existingAssignmentPackage);
        await dbContext.SaveChangesAsync(cancellationToken);

        return DtoMapper.Convert(existingAssignmentPackage);
    }

        public Task<Result<IEnumerable<AccessPackageDto.Check>>> CheckPackage(Guid party, IEnumerable<Guid> packageIds = null, CancellationToken cancellationToken = default)
    {
        return null;
        // return assignablePackages.GroupBy(p => p.Package.Id).Select(group =>
        // {
        //     var firstPackage = group.First();
        //     return new AccessPackageDto.Check
        //     {
        //         Package = new AccessPackageDto
        //         {
        //             Id = firstPackage.Package.Id,
        //             Urn = firstPackage.Package.Urn,
        //             AreaId = firstPackage.Package.AreaId
        //         },
        //         Result = group.Any(p => p.Result),
        //         Reasons = group.Select(p => new AccessPackageDto.Check.Reason
        //         {
        //             Description = p.Reason.Description,
        //             RoleId = p.Reason.RoleId,
        //             RoleUrn = p.Reason.RoleUrn,
        //             FromId = p.Reason.FromId,
        //             FromName = p.Reason.FromName,
        //             ToId = p.Reason.ToId,
        //             ToName = p.Reason.ToName,
        //             ViaId = p.Reason.ViaId,
        //             ViaName = p.Reason.ViaName,
        //             ViaRoleId = p.Reason.ViaRoleId,
        //             ViaRoleUrn = p.Reason.ViaRoleUrn
        //         })
        //     };
        // }).ToList();
    }

    public async Task<Result<IEnumerable<AccessPackageDto.Check>>> CheckPackage(Guid party, IEnumerable<string> packages, IEnumerable<Guid> packageIds = null, CancellationToken cancellationToken = default)
    {
        packages = packages.Select(p =>
        p.StartsWith("urn:", StringComparison.Ordinal)
            ? p
            : (p.StartsWith(":", StringComparison.Ordinal)
                ? $"urn:altinn:accesspackage{p}"
                : $"urn:altinn:accesspackage:{p}"));

        var packagesFound = packages.Select(p =>
        {
            if (PackageConstants.TryGetByUrn(p, out var package))
            {
                return package.Entity;
            }

            return null;
        }).Where(p => p is { });

        var problem = ValidationComposer.Validate(PackageValidation.PackageUrnLookup(packagesFound, packages));
        if (problem is { })
        {
            return problem;
        }

        return await CheckPackage(party, [.. packageIds, .. packagesFound.Select(p => p.Id)], cancellationToken);
    }

    private ValidationProblemInstance? ValidateEntities(Entity from, Entity to, ConnectionOptions options)
    {
        var entityExists = ValidationComposer.Validate(
            EntityValidation.FromExists(from),
            EntityValidation.ToExists(to)
        );

        if (entityExists is { })
        {
            return entityExists;
        }

        var entitiesIsOfRightType = ValidationComposer.Validate(
            EntityTypeValidation.FromIsOfType(from.Type, [.. options.SupportedFromEntityTypes]),
            EntityTypeValidation.ToIsOfType(to.Type, [.. options.SupportedToEntityTypes])
        );

        if (entitiesIsOfRightType is { })
        {
            return entitiesIsOfRightType;
        }

        return null;
    }
}

public sealed class ConnectionOptions
{
    internal ConnectionOptions(Action<ConnectionOptions> configureConnectionService)
    {
        if (configureConnectionService is { })
        {
            configureConnectionService(this);
        }
    }

    public IEnumerable<string> SupportedFromEntityTypes { get; set; } = [];

    public IEnumerable<string> SupportedToEntityTypes { get; set; } = [];
}

/// <inheritdoc />
public partial class ConnectionService : IConnectionRepository
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
