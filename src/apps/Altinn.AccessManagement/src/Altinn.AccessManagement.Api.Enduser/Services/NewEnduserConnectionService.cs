using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement.Api.Enduser.Services.Models;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessManagement.Enduser.Services;

/// <summary>
/// Service for managing connections.
/// </summary>
public class NewEnduserConnectionService(
   AuditValues auditValues,
   AppDbContext db, 
   NewRelationService RelationService
    ) : INewEnduserConnectionService
{
    /// <inheritdoc />
    public async Task<Result<List<RelationDto>>> Get(Guid? fromId = null, Guid? toId = null, CancellationToken cancellationToken = default)
    {
        var res = await db.Relations.Where(t => t.FromId == fromId).ToListAsync();

        if (fromId is { } && fromId.Value != Guid.Empty)
        {
            var result = await RelationService.GetConnectionsToOthers(fromId.Value, toId is { } && toId != Guid.Empty ? toId : null, null, cancellationToken: cancellationToken);
            return result.ToList();
        }

        if (toId is { } && toId != Guid.Empty)
        {
            var result = await RelationService.GetConnectionsFromOthers(toId.Value, fromId is { } && fromId.Value != Guid.Empty ? fromId : null, null, cancellationToken: cancellationToken);
            return result.ToList();
        }

        Unreachable();
        return default;
    }

    /// <inheritdoc />
    public async Task<Result<Assignment>> AddAssignment(Guid fromId, Guid toId, string role, CancellationToken cancellationToken = default)
    {
        var dependencies = await GetAssignmentDependencies(fromId, toId, role, cancellationToken);
        if (ValidateAssignmentData(dependencies.EntityFrom, dependencies.EntityTo, dependencies.Role) is var problem && problem is { })
        {
            return problem;
        }

        var roleId = dependencies.Role.Id;
        var existingAssignment = await db.Assignments.AsNoTracking().Where(t => t.FromId == fromId && t.ToId == toId && t.RoleId == roleId).ToListAsync();

        if (existingAssignment != null && existingAssignment.Any())
        {
            return existingAssignment.First();
        }

        var assignment = new Assignment()
        {
            FromId = fromId,
            ToId = toId,
            RoleId = roleId,
        };

        db.Database.SetAuditSession(auditValues);
        db.Assignments.Add(assignment);
        var result = await db.SaveChangesAsync(cancellationToken);
        if (result == 0)
        {
            Unreachable();
        }

        return assignment;
    }

    /// <inheritdoc />
    public async Task<ValidationProblemInstance> RemoveAssignment(Guid fromId, Guid toId, string role, bool cascade, CancellationToken cancellationToken = default)
    {
        var dependencies = await GetAssignmentDependencies(fromId, toId, role, cancellationToken);
        if (ValidateAssignmentData(dependencies.EntityFrom, dependencies.EntityTo, dependencies.Role) is var problem && problem is { })
        {
            return problem;
        }

        var roleId = dependencies.Role.Id;
        var existingAssignments = await db.Assignments.AsNoTracking().Where(t => t.FromId == fromId && t.ToId == toId && t.RoleId == roleId).ToListAsync();

        if (existingAssignments != null && !existingAssignments.Any())
        {
            return null;
        }

        var existingAssignment = existingAssignments.First();

        if (!cascade)
        {
            var packages = db.AssignmentPackages.AsNoTracking().Where(t => t.AssignmentId == existingAssignment.Id);
            var delegationsFrom = db.Delegations.AsNoTracking().Where(t => t.FromId == existingAssignment.Id);
            var delegationsTo = db.Delegations.AsNoTracking().Where(t => t.ToId == existingAssignment.Id);

            problem = EnduserValidationRules.Validate(
                EnduserValidationRules.QueryParameters.HasPackagesAssigned(packages),
                EnduserValidationRules.QueryParameters.HasDelegationsAssigned(delegationsFrom),
                EnduserValidationRules.QueryParameters.HasDelegationsAssigned(delegationsTo)
            );
            if (problem is { })
            {
                return problem;
            }
        }

        db.Database.SetAuditSession(auditValues);
        db.Assignments.Remove(existingAssignment);
        var result = await db.SaveChangesAsync();

        if (result == 0)
        {
            Unreachable();
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<Result<List<PackagePermission>>> GetPackages(Guid? fromId, Guid? toId, CancellationToken cancellationToken = default)
    {
        if (fromId is { } && fromId.Value != Guid.Empty)
        {
            var result = await RelationService.GetPackagePermissionsToOthers(fromId.Value, toId is { } && toId != Guid.Empty ? toId : null, null, cancellationToken);
            return result.ToList();
        }

        if (toId is { } && toId.Value != Guid.Empty)
        {
            var result = await RelationService.GetPackagePermissionsFromOthers(toId.Value, fromId is { } && fromId.Value != Guid.Empty ? fromId : null, null, cancellationToken);
            return result.ToList();
        }

        Unreachable();
        return default;
    }

    /// <inheritdoc />
    public async Task<Result<AssignmentPackage>> AddPackage(Guid fromId, Guid toId, string role, string package, CancellationToken cancellationToken = default)
    {
        package = (package.StartsWith("urn:", StringComparison.Ordinal) || package.StartsWith(':')) ? package : ":" + package;

        var packages = await db.Packages.AsNoTracking().Where(t => t.Urn.EndsWith(package)).ToListAsync();
        var problem = EnduserValidationRules.Validate(EnduserValidationRules.QueryParameters.PackageUrnLookup(packages, package));
        if (problem is { })
        {
            return problem;
        }

        var packageResult = packages.First();
        return await AddPackage(fromId, toId, role, packageResult.Id, "package", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<AssignmentPackage>> AddPackage(Guid fromId, Guid toId, string role, Guid packageId, CancellationToken cancellationToken = default)
    {
        return await AddPackage(fromId, toId, role, packageId, "packageId", cancellationToken);
    }

    private async Task<Result<AssignmentPackage>> AddPackage(Guid fromId, Guid toId, string role, Guid packageId, string queryParamName, CancellationToken cancellationToken)
    {
        var dependencies = await GetAssignmentDependencies(fromId, toId, role, cancellationToken);
        if (ValidateAssignmentData(dependencies.EntityFrom, dependencies.EntityTo, dependencies.Role) is var problem && problem is { })
        {
            return problem;
        }

        var roleId = dependencies.Role.Id;
        var existingAssignments = await db.Assignments.AsNoTracking().Where(t => t.FromId == fromId && t.ToId == toId && t.RoleId == roleId).ToListAsync();

        problem = EnduserValidationRules.Validate(EnduserValidationRules.QueryParameters.VerifyAssignmentRoleExists(existingAssignments, role));
        Assignment assignment;
        if (problem is { })
        {
            var relationsResult = await Get(fromId, toId, cancellationToken: cancellationToken);
            if (relationsResult is { } && relationsResult.Value is { } && relationsResult.Value.Any())
            {
                var assignmentResult = await AddAssignment(fromId, toId, "rettighetshaver", cancellationToken);
                assignment = assignmentResult.Value;
            }
            else
            {
                return problem;
            }
        }
        else
        {
            assignment = existingAssignments.First();
        }

        var check = await CheckPackage(fromId, packageIds: [packageId], cancellationToken);
        if (check.IsProblem)
        {
            return check.Problem;
        }

        problem = EnduserValidationRules.Validate(
            EnduserValidationRules.QueryParameters.AuthorizePackageAssignment(check.Value),
            EnduserValidationRules.QueryParameters.PackageIsAssignableToRecipient(check.Value.Select(p => p.Package.Urn), dependencies.EntityTo, queryParamName)
        );

        if (problem is { })
        {
            return problem;
        }

        var existingPackageAssignment = await db.AssignmentPackages.Where(t => t.AssignmentId == assignment.Id && t.PackageId == packageId).ToListAsync();
        if (existingPackageAssignment != null && existingPackageAssignment.Any())
        {
            return existingPackageAssignment.First();
        }

        db.Database.SetAuditSession(auditValues);
        db.AssignmentPackages.Add(new AssignmentPackage() { AssignmentId = assignment.Id, PackageId = packageId });
        var createResult = await db.SaveChangesAsync();
        if (createResult == 0)
        {
            Unreachable();
        }

        var createCheckResult = await db.AssignmentPackages.Where(t => t.AssignmentId == assignment.Id && t.PackageId == packageId).ToListAsync();
        if (createCheckResult == null || !createCheckResult.Any())
        {
            Unreachable();
        }

        return createCheckResult.First();
    }

    /// <inheritdoc/>
    public async Task<ValidationProblemInstance> RemovePackage(Guid fromId, Guid toId, string role, string package, CancellationToken cancellationToken = default)
    {
        package = (package.StartsWith("urn:", StringComparison.Ordinal) || package.StartsWith(":", StringComparison.Ordinal)) ? package : ":" + package;

        var packages = await db.Packages.AsNoTracking().Where(t => t.Urn.EndsWith(package)).ToListAsync();
        var problem = EnduserValidationRules.Validate(EnduserValidationRules.QueryParameters.PackageUrnLookup(packages, package));
        if (problem is { })
        {
            return problem;
        }

        var packageResult = packages.First();
        return await RemovePackage(fromId, toId, role, packageResult.Id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ValidationProblemInstance> RemovePackage(Guid fromId, Guid toId, string role, Guid packageId, CancellationToken cancellationToken = default)
    {
        var roleResult = await db.Roles.AsNoTracking().Where(t => t.Code == role).ToListAsync(cancellationToken);
        if (roleResult == null || !roleResult.Any())
        {
            return null;
        }

        var result = await db.Assignments.AsNoTracking().Where(t => t.FromId == fromId && t.ToId ==toId && t.RoleId == roleResult.First().Id).ToListAsync();
        if (result == null || !result.Any())
        {
            return null;
        }

        var assignment = result.First();
        var userPackages = await db.ConnectionPackages.AsNoTracking().Where(t => t.FromId == fromId && t.ToId == toId && t.PackageId == packageId).ToListAsync(cancellationToken);

        if (!userPackages.Any())
        {
            return null;
        }

        var checkResult = await db.AssignmentPackages.AsNoTracking().Where(t => t.AssignmentId == assignment.Id && t.PackageId == packageId).ToListAsync(cancellationToken);
        if (checkResult == null && !checkResult.Any())
        {
            return null;
        }

        db.Database.SetAuditSession(auditValues);
        var assignmentPackage = await db.AssignmentPackages.FirstAsync(t => t.AssignmentId == assignment.Id && t.PackageId == packageId);
        db.AssignmentPackages.Remove(assignmentPackage);

        var deleteResult = await db.SaveChangesAsync(cancellationToken);
        if (deleteResult == 0)
        {
            return null;
        }

        return null;
    }

    public async Task<Result<IEnumerable<AccessPackageDto.Check>>> CheckPackage(Guid party, IEnumerable<string> packages, IEnumerable<Guid> packageIds = null, CancellationToken cancellationToken = default)
    {
        packages = packages.Select(p =>
            p.StartsWith("urn:", StringComparison.Ordinal)
                ? p
                : (p.StartsWith(":", StringComparison.Ordinal)
                    ? $"urn:altinn:accesspackage{p}"
                    : $"urn:altinn:accesspackage:{p}"));

        var allPackages = await db.Packages.AsNoTracking().Where(t => packages.Contains(t.Urn)).ToListAsync(cancellationToken);
        var problem = EnduserValidationRules.Validate(EnduserValidationRules.QueryParameters.PackageUrnLookup(allPackages, packages));
        if (problem is { })
        {
            return problem;
        }

        return await CheckPackage(party, (List<Guid>)[.. packageIds, .. allPackages.Select(p => p.Id)], cancellationToken);
    }

    public async Task<Result<IEnumerable<AccessPackageDto.Check>>> CheckPackage(Guid party, IEnumerable<Guid>? packageIds = null, CancellationToken cancellationToken = default)
    {
        var assignablePackages = await RelationService.GetAssignablePackagePermissions(
            auditValues.ChangedBy,
            party,
            packageIds,
            cancellationToken: cancellationToken);

        return assignablePackages.GroupBy(p => p.Package.Id).Select(group =>
        {
            var firstPackage = group.First();
            return new AccessPackageDto.Check
            {
                Package = new AccessPackageDto.Compact
                {
                    Id = firstPackage.Package.Id,
                    Urn = firstPackage.Package.Urn,
                    AreaId = firstPackage.Package.AreaId
                },
                Result = group.Any(p => p.Result),
                Reasons = group.Select(p => new AccessPackageDto.Check.Reason
                {
                    Description = p.Reason.Description,
                    RoleId = p.Reason.RoleId,
                    RoleUrn = p.Reason.RoleUrn,
                    FromId = p.Reason.FromId,
                    FromName = p.Reason.FromName,
                    ToId = p.Reason.ToId,
                    ToName = p.Reason.ToName,
                    ViaId = p.Reason.ViaId,
                    ViaName = p.Reason.ViaName,
                    ViaRoleId = p.Reason.ViaRoleId,
                    ViaRoleUrn = p.Reason.ViaRoleUrn
                })
            };
        }).ToList();
    }

    private ValidationProblemInstance? ValidateAssignmentData(Entity entityFrom, Entity entityTo, Role role)
    {
        var problem = EnduserValidationRules.Validate(
            EnduserValidationRules.QueryParameters.PartyExists(entityFrom, "from"),
            EnduserValidationRules.QueryParameters.PartyExists(entityTo, "to"),
            EnduserValidationRules.QueryParameters.PartyIsEntityType(entityFrom, "Organisasjon", "from"),
            EnduserValidationRules.QueryParameters.PartyIsEntityType(entityTo, "Organisasjon", "to")
        );

        if (problem is { })
        {
            return problem;
        }

        if (role is null)
        {
            Unreachable();
        }

        return null;
    }

    private async Task<(Entity EntityFrom, Entity EntityTo, Role Role)> GetAssignmentDependencies(Guid from, Guid to, string roleCode, CancellationToken cancellationToken)
    {
        var entityFrom = await db.Entities.FirstOrDefaultAsync(t => t.Id == from);
        var entityTo = await db.Entities.FirstOrDefaultAsync(t => t.Id == to);
        var role = await db.Roles.FirstOrDefaultAsync(t => t.Code == roleCode);

        return (entityFrom, entityTo, role);
    }

    [DoesNotReturn]
    private void Unreachable() =>
        throw new UnreachableException();

    private static string SpanName(string spanName) =>
        $"{nameof(EnduserConnectionService)}: {spanName}";
}


/// <summary>
/// Interface for managing connections.
/// </summary>
public interface INewEnduserConnectionService
{
    /// <summary>
    /// Retrieves a list of external connections, optionally filtered by origin and/or destination entity IDs.
    /// </summary>
    /// <param name="fromId">ID of the originating entity to filter connections by.</param>
    /// <param name="toId">ID of the target entity to filter connections by.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a list of <see cref="Connection"/> instances matching the criteria.
    /// </returns>
    Task<Result<List<RelationDto>>> Get(Guid? fromId = null, Guid? toId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a role assignment between two entities.
    /// </summary>
    /// <param name="fromId">ID of the entity from which the assignment originates.</param>
    /// <param name="toId">ID of the entity to which the assignment is made.</param>
    /// <param name="role">Name of the role to assign.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the newly created <see cref="Assignment"/>.
    /// </returns>
    Task<Result<Assignment>> AddAssignment(Guid fromId, Guid toId, string role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a specific role assignment between two entities.
    /// </summary>
    /// <param name="fromId">ID of the entity from which the assignment originates.</param>
    /// <param name="toId">ID of the entity to which the assignment was made.</param>
    /// <param name="role">Name of the role to remove.</param>
    /// <param name="cascade">If <c>false</c>, stop if there are any dependent records.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="ValidationProblemInstance"/> indicating success or describing any validation errors.
    /// </returns>
    Task<ValidationProblemInstance> RemoveAssignment(Guid fromId, Guid toId, string role, bool cascade, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of connection packages, optionally filtered by origin and/or destination entity IDs.
    /// </summary>
    /// <param name="fromId">ID of the originating entity to filter packages by.</param>
    /// <param name="toId">ID of the target entity to filter packages by.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a list of <see cref="ConnectionPackage"/> instances.
    /// </returns>
    Task<Result<List<PackagePermission>>> GetPackages(Guid? fromId, Guid? toId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a package to an assignment (by package ID) based on the role between two entities.
    /// </summary>
    /// <param name="fromId">ID of the entity from which the assignment originates.</param>
    /// <param name="toId">ID of the entity to which the assignment is made.</param>
    /// <param name="role">Name of the role assigned.</param>
    /// <param name="packageId">Unique identifier of the package to assign.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the created <see cref="AssignmentPackage"/>.
    /// </returns>
    Task<Result<AssignmentPackage>> AddPackage(Guid fromId, Guid toId, string role, Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a package to an assignment (by package name or code) based on the role between two entities.
    /// </summary>
    /// <param name="fromId">ID of the entity from which the assignment originates.</param>
    /// <param name="toId">ID of the entity to which the assignment is made.</param>
    /// <param name="role">Name of the role assigned.</param>
    /// <param name="package">Urn value of the package to assign.</param>
    /// <param name="cancellationToken">
    /// Token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the created <see cref="AssignmentPackage"/>.
    /// </returns>
    Task<Result<AssignmentPackage>> AddPackage(Guid fromId, Guid toId, string role, string package, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a package (by package ID) from assignment based on a specific role between two entities.
    /// </summary>
    /// <param name="fromId">ID of the entity from which the assignment originates.</param>
    /// <param name="toId">ID of the entity to which the assignment was made.</param>
    /// <param name="role">Name of the role from which the package is removed.</param>
    /// <param name="packageId">Unique identifier of the package to remove.</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="ValidationProblemInstance"/> indicating success or describing any validation errors.
    /// </returns>
    Task<ValidationProblemInstance> RemovePackage(Guid fromId, Guid toId, string role, Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a package (by package name or code) from assignment based on a specific role between two entities.
    /// </summary>
    /// <param name="fromId">ID of the entity from which the assignment originates.</param>
    /// <param name="toId">ID of the entity to which the assignment was made.</param>
    /// <param name="role">Name of the role from which the package is removed.</param>
    /// <param name="package">Urn value of the package to remove.</param>
    /// <param name="cancellationToken">
    /// Token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A <see cref="ValidationProblemInstance"/> indicating success or describing any validation errors.
    /// </returns>
    Task<ValidationProblemInstance> RemovePackage(Guid fromId, Guid toId, string role, string package, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an authpenticated user is an access manager and has the necessary permissions to delegate a specific access package.
    /// </summary>
    /// <param name="party">ID of the person.</param>
    /// <param name="packageIds">Filter param using unique package identifiers.</param>
    /// <param name="cancellationToken">
    /// Token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A <see cref="ValidationProblemInstance"/> indicating success or describing any validation errors.
    /// </returns>
    Task<Result<IEnumerable<AccessPackageDto.Check>>> CheckPackage(Guid party, IEnumerable<Guid> packageIds = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an authpenticated user is an access manager and has the necessary permissions to delegate a specific access package.
    /// </summary>
    /// <param name="party">ID of the person.</param>
    /// <param name="packages">Filter param using urn package identifiers.</param>
    /// <param name="packageIds">Filter param using unique package identifiers.</param>
    /// <param name="cancellationToken">
    /// Token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A <see cref="ValidationProblemInstance"/> indicating success or describing any validation errors.
    /// </returns>
    Task<Result<IEnumerable<AccessPackageDto.Check>>> CheckPackage(Guid party, IEnumerable<string> packages, IEnumerable<Guid> packageIds = null, CancellationToken cancellationToken = default);
}
