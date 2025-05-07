using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Helpers;
using Altinn.AccessMgmt.Persistence.Repositories;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Enduser.Services;

/// <summary>
/// Service for managing connections.
/// </summary>
public class ConnectionService(
    IDbAudit dbAudit,
    IConnectionRepository connectionRepository,
    IConnectionPackageRepository connectionPackageRepository,
    IPackageRepository packageRepository,
    IRoleRepository roleRepository,
    IAssignmentRepository assignmentRepository,
    IDelegationRepository delegationRepository,
    IAssignmentPackageRepository assignmentPackageRepository,
    IEntityRepository entityRepository
    ) : IEnduserConnectionService
{
    private IDbAudit DbAudit { get; } = dbAudit;

    private IConnectionRepository ConnectionRepository { get; } = connectionRepository;

    private IConnectionPackageRepository ConnectionPackageRepository { get; } = connectionPackageRepository;

    private IPackageRepository PackageRepository { get; } = packageRepository;

    private IRoleRepository RoleRepository { get; } = roleRepository;

    private IAssignmentRepository AssignmentRepository { get; } = assignmentRepository;

    private IDelegationRepository DelegationRepository { get; } = delegationRepository;

    private IAssignmentPackageRepository AssignmentPackageRepository { get; } = assignmentPackageRepository;

    private IEntityRepository EntityRepository { get; } = entityRepository;

    /// <inheritdoc />
    public async Task<Result<List<ExtConnection>>> Get(Guid? from = null, Guid? to = null, CancellationToken cancellationToken = default)
    {
        var filter = ConnectionRepository.CreateFilterBuilder();
        static bool SetFrom(Guid? from, GenericFilterBuilder<Connection> filter)
        {
            if (from != null && from.HasValue && from.Value != Guid.Empty)
            {
                filter.Equal(t => t.FromId, from);
                return true;
            }

            filter.NotSet(t => t.FromId);
            return false;
        }

        static bool SetTo(Guid? to, GenericFilterBuilder<Connection> filter)
        {
            if (to != null && to.HasValue && to.Value != Guid.Empty)
            {
                filter.Equal(t => t.ToId, to);
                return true;
            }

            filter.NotSet(t => t.ToId);
            return false;
        }

        if (SetFrom(from, filter) || SetTo(to, filter))
        {
            filter.NotSet(t => t.Id);
            filter.NotSet(t => t.FacilitatorId);
        }
        else
        {
            Unreachable();
        }

        var result = await ConnectionRepository.GetExtended(filter, cancellationToken: cancellationToken);
        return result?.ToList() ?? [];
    }

    /// <inheritdoc />
    public async Task<Result<Assignment>> AddAssignment(Guid from, Guid to, string roleCode, CancellationToken cancellationToken = default)
    {
        var dependencies = await GetAssignmentDependencies(from, to, roleCode, cancellationToken);
        if (ValidateAssignmentData(dependencies.EntityFrom, dependencies.EntityTo, dependencies.Roles) is var problem && problem is { })
        {
            return problem;
        }

        var roleId = dependencies.Roles.First().Id;
        var filter = AssignmentRepository.CreateFilterBuilder()
            .Equal(t => t.FromId, from)
            .Equal(t => t.ToId, to)
            .Equal(t => t.RoleId, roleId);

        var existingAssignment = await AssignmentRepository.Get(filter, cancellationToken: cancellationToken);
        if (existingAssignment != null && existingAssignment.Any())
        {
            return existingAssignment.First();
        }

        var assignment = new Assignment()
        {
            FromId = from,
            ToId = to,
            RoleId = roleId,
        };

        var result = await AssignmentRepository.Create(assignment, DbAudit.Value, cancellationToken);
        if (result == 0)
        {
            Unreachable();
        }

        return assignment;
    }

    /// <inheritdoc />
    public async Task<ValidationProblemInstance> RemoveAssignment(Guid from, Guid to, string roleCode, bool cascade, CancellationToken cancellationToken = default)
    {
        var dependencies = await GetAssignmentDependencies(from, to, roleCode, cancellationToken);
        if (ValidateAssignmentData(dependencies.EntityFrom, dependencies.EntityTo, dependencies.Roles) is var problem && problem is { })
        {
            return problem;
        }

        var roleId = dependencies.Roles.First().Id;
        var filter = AssignmentRepository.CreateFilterBuilder()
            .Equal(t => t.FromId, from)
            .Equal(t => t.ToId, to)
            .Equal(t => t.RoleId, roleId);

        var existingAssignments = await AssignmentRepository.Get(filter, cancellationToken: cancellationToken);
        if (existingAssignments != null && !existingAssignments.Any())
        {
            return null;
        }

        var existingAssignment = existingAssignments.First();

        if (!cascade)
        {
            var packages = await AssignmentPackageRepository.Get(f => f.AssignmentId, existingAssignment.Id, cancellationToken: cancellationToken);
            var delegationsFrom = await DelegationRepository.Get(f => f.FromId, existingAssignment.Id, cancellationToken: cancellationToken);
            var delegationsTo = await DelegationRepository.Get(f => f.ToId, existingAssignment.Id, cancellationToken: cancellationToken);

            problem = ValidationRules.Validate(
                ValidationRules.QueryParameters.HasPackagesAssigned(packages),
                ValidationRules.QueryParameters.HasDelegationsAssigned(delegationsFrom),
                ValidationRules.QueryParameters.HasDelegationsAssigned(delegationsTo)
            );
            if (problem is { })
            {
                return problem;
            }
        }

        var result = await AssignmentRepository.Delete(existingAssignment.Id, DbAudit.Value, cancellationToken: cancellationToken);
        if (result == 0)
        {
            Unreachable();
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<Result<List<ConnectionPackage>>> GetPackages(Guid? from, Guid? to, CancellationToken cancellationToken = default)
    {
        var filter = ConnectionPackageRepository.CreateFilterBuilder();
        static bool SetFrom(Guid? from, GenericFilterBuilder<ConnectionPackage> filter)
        {
            if (from != null && from.HasValue && from.Value != Guid.Empty)
            {
                filter.Equal(t => t.FromId, from);
                return true;
            }

            filter.NotSet(t => t.FromId);
            return false;
        }

        static bool SetTo(Guid? to, GenericFilterBuilder<ConnectionPackage> filter)
        {
            if (to != null && to.HasValue && to.Value != Guid.Empty)
            {
                filter.Equal(t => t.ToId, to);
                return true;
            }

            filter.NotSet(t => t.ToId);
            return false;
        }

        if (SetFrom(from, filter) || SetTo(to, filter))
        {
            filter.NotSet(t => t.Id);
            filter.NotSet(t => t.FacilitatorId);
        }
        else
        {
            Unreachable();
        }

        var result = await ConnectionPackageRepository.Get(filter, cancellationToken: cancellationToken);
        return result.ToList();
    }

    public async Task<Result<AssignmentPackage>> AddAssignmentPackage(Guid assignmentId, string packageUrn, CancellationToken cancellationToken = default)
    {
        /* Add : prefix */
        var filter = PackageRepository.CreateFilterBuilder().Add(t => t.Urn, packageUrn, AccessMgmt.Persistence.Core.Helpers.FilterComparer.EndsWith);
        var package = await PackageRepository.Get(filter);

        return await AddAssignmentPackage(assignmentId, package.First(), cancellationToken);
    }

    public async Task<Result<AssignmentPackage>> AddAssignmentPackage(Guid assignmentId, Guid packageId, CancellationToken cancellationToken = default)
    {
        var package = await PackageRepository.Get(packageId);

        return await AddAssignmentPackage(assignmentId, package, cancellationToken);
    }

    private async Task<Result<AssignmentPackage>> AddAssignmentPackage(Guid assignmentId, Package package, CancellationToken cancellationToken = default)
    {
        var assignment = await AssignmentRepository.Get(assignmentId);

        var assignmentPackageFilter = AssignmentPackageRepository
            .CreateFilterBuilder()
            .Equal(t => t.AssignmentId, assignmentId)
            .Equal(t => t.PackageId, package.Id);

        var existingPackageAssignment = await AssignmentPackageRepository.Get(assignmentPackageFilter, cancellationToken: cancellationToken);
        if (existingPackageAssignment != null && existingPackageAssignment.Any())
        {
            return existingPackageAssignment.First();
        }

        var createResult = await AssignmentPackageRepository.Create(new AssignmentPackage() { AssignmentId = assignmentId, PackageId = package.Id }, DbAudit.Value, cancellationToken: cancellationToken);
        if (createResult == 0)
        {
            Unreachable();
        }

        var createCheckResult = await AssignmentPackageRepository.Get(t => t.AssignmentId, assignmentId, cancellationToken: cancellationToken);
        if (createCheckResult == null || !existingPackageAssignment.Any())
        {
            Unreachable();
        }

        return createCheckResult.First();
    }

    /// <inheritdoc />
    public async Task<Result<AssignmentPackage>> AddPackage(Guid fromId, Guid toId, string roleCode, string packageUrn, CancellationToken cancellationToken = default)
    {
        var filter = PackageRepository.CreateFilterBuilder()
            .Add(t => t.Urn, packageUrn, AccessMgmt.Persistence.Core.Helpers.FilterComparer.EndsWith);

        var packages = await PackageRepository.Get(filter, cancellationToken: cancellationToken);
        var problem = ValidationRules.Validate(ValidationRules.QueryParameters.PackageUrnLookup(packages));
        if (problem is { })
        {
            return problem;
        }

        var package = packages.First();
        return await AddPackage(fromId, toId, roleCode, package.Id, "packgeUrn", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<AssignmentPackage>> AddPackage(Guid fromId, Guid toId, string roleCode, Guid packageId, CancellationToken cancellationToken = default)
    {
        return await AddPackage(fromId, toId, roleCode, packageId, "packageId", cancellationToken);
    }

    private async Task<Result<AssignmentPackage>> AddPackage(Guid fromId, Guid toId, string roleCode, Guid packageId, string queryParamName, CancellationToken cancellationToken)
    {
        var res = await Get(from: fromId, to: toId);
        res.Value.Where(t => t.Role.Code.Equals(roleCode));

        var assignment = await AddAssignment(fromId, toId, roleCode, cancellationToken);
        if (assignment.IsProblem)
        {
            return assignment.Problem;
        }

        var connectionPackageFilter = ConnectionPackageRepository.CreateFilterBuilder()
            .Equal(t => t.FromId, fromId)
            .Equal(t => t.ToId, toId)
            .Equal(t => t.PackageId, packageId);

        var connectionPackages = await ConnectionPackageRepository.GetExtended(connectionPackageFilter, cancellationToken: cancellationToken);

        var problem = ValidationRules.Validate(
            ValidationRules.QueryParameters.AnyPackages(connectionPackages, queryParamName),
            ValidationRules.QueryParameters.PackageIsAssignableByUser(connectionPackages, queryParamName),
            ValidationRules.QueryParameters.PackageIsAssignableByDefinition(connectionPackages, queryParamName)
        );

        if (problem is { })
        {
            return problem;
        }

        var assignmentPackageFilter = AssignmentPackageRepository
            .CreateFilterBuilder()
            .Equal(t => t.AssignmentId, assignment.Value.Id)
            .Equal(t => t.PackageId, packageId);

        var existingPackageAssignment = await AssignmentPackageRepository.Get(assignmentPackageFilter, cancellationToken: cancellationToken);
        if (existingPackageAssignment != null && existingPackageAssignment.Any())
        {
            return existingPackageAssignment.First();
        }

        var createResult = await AssignmentPackageRepository.Create(new AssignmentPackage() { AssignmentId = assignment.Value.Id, PackageId = packageId }, DbAudit.Value, cancellationToken: cancellationToken);
        if (createResult == 0)
        {
            Unreachable();
        }

        var createCheckResult = await AssignmentPackageRepository.Get(t => t.AssignmentId, assignment.Value.Id, cancellationToken: cancellationToken);
        if (createCheckResult == null || !existingPackageAssignment.Any())
        {
            Unreachable();
        }

        return createCheckResult.First();
    }

    /// <inheritdoc/>
    public async Task<ValidationProblemInstance> RemovePackage(Guid fromId, Guid toId, string roleCode, Guid packageId, CancellationToken cancellationToken = default)
    {
        var roleResult = await RoleRepository.Get(t => t.Code, roleCode, cancellationToken: cancellationToken);
        if (roleResult == null || !roleResult.Any())
        {
            return null;
        }

        var assignmentFilter = AssignmentRepository.CreateFilterBuilder()
            .Equal(t => t.FromId, fromId)
            .Equal(t => t.ToId, toId)
            .Equal(t => t.RoleId, roleResult.First().Id);

        var result = await AssignmentRepository.Get(assignmentFilter, cancellationToken: cancellationToken);
        if (result == null || !result.Any())
        {
            return null;
        }

        var assignment = result.First();

        var connectionPackageFilter = ConnectionPackageRepository.CreateFilterBuilder()
            .Equal(t => t.FromId, fromId)
            .Equal(t => t.ToId, toId)
            .Equal(t => t.PackageId, packageId);

        var userPackages = await ConnectionPackageRepository.GetExtended(connectionPackageFilter, cancellationToken: cancellationToken);

        if (!userPackages.Any())
        {
            return null;
        }

        var packageFilter = AssignmentPackageRepository.CreateFilterBuilder()
            .Equal(t => t.AssignmentId, assignment.Id)
            .Equal(t => t.PackageId, packageId);

        var checkResult = await AssignmentPackageRepository.Get(assignmentFilter, cancellationToken: cancellationToken);
        if (checkResult == null && !checkResult.Any())
        {
            return null;
        }

        var deleteResult = await AssignmentPackageRepository.DeleteCross(assignment.Id, packageId, DbAudit.Value, cancellationToken: cancellationToken);
        if (deleteResult == 0)
        {
            return null;
        }

        return null;
    }

    private async Task<IEnumerable<ExtConnectionPackage>> GetConnectionPackages(Guid fromId, Guid toId, Guid packageId, CancellationToken cancellationToken = default)
    {
        /* Get packages assigned to entity from a specific entity, to see if entity can assign it to another entity */

        var filter = ConnectionPackageRepository.CreateFilterBuilder();
        filter.Equal(t => t.FromId, fromId);
        filter.Equal(t => t.ToId, toId);
        filter.Equal(t => t.PackageId, packageId);
        return await ConnectionPackageRepository.GetExtended(filter, cancellationToken: cancellationToken);
    }

    private ValidationProblemInstance ValidateAssignmentData(ExtEntity entityFrom, ExtEntity entityTo, IEnumerable<Role> roles)
    {
        var problem = ValidationRules.Validate(
            ValidationRules.QueryParameters.PartyExists(entityFrom, "from"),
            ValidationRules.QueryParameters.PartyExists(entityTo, "to"),
            ValidationRules.QueryParameters.PartyIsEntityType(entityFrom, "Organisasjon", "from"),
            ValidationRules.QueryParameters.PartyIsEntityType(entityTo, "Organisasjon", "to")
        );

        if (problem is { })
        {
            return problem;
        }

        if (roles is null || !roles.Any())
        {
            Unreachable();
        }

        return null;
    }

    private async Task<(ExtEntity EntityFrom, ExtEntity EntityTo, IEnumerable<Role> Roles)> GetAssignmentDependencies(Guid from, Guid to, string roleCode, CancellationToken cancellationToken)
    {
        var entityFromTask = EntityRepository.GetExtended(from, cancellationToken: cancellationToken);
        var entityToTask = EntityRepository.GetExtended(to, cancellationToken: cancellationToken);
        var roleTask = RoleRepository.Get(t => t.Code, roleCode, cancellationToken: cancellationToken);
        return (await entityFromTask, await entityToTask, await roleTask);
    }

    [DoesNotReturn]
    private void Unreachable()
    {
        throw new UnreachableException();
    }
}

/// <summary>
/// Interface for managing connections.
/// </summary>
public interface IEnduserConnectionService
{
    Task<Result<ExtConnection>> Get(Guid id, CancellationToken cancellationToken = default);

    Task<Result<List<ExtConnection>>> Get(Guid? from = null, Guid? to = null, Guid? facilitator = null, CancellationToken cancellationToken = default);

    Task<Result<Assignment>> AddAssignment(Guid from, Guid to, string roleCode, CancellationToken cancellationToken = default);

    Task<ValidationProblemInstance> RemoveAssignment(Guid from, Guid to, string roleCode, bool cascade, CancellationToken cancellationToken = default);

    Task<Result<List<ConnectionPackage>>> GetPackages(Guid? from, Guid? to, CancellationToken cancellationToken = default);

    Task<Result<AssignmentPackage>> AddPackage(Guid fromId, Guid toId, string rolecode, Guid packageId, CancellationToken cancellationToken = default);

    Task<Result<AssignmentPackage>> AddPackage(Guid fromId, Guid toId, string rolecode, string packageUrn, CancellationToken cancellationToken = default);

    Task<ValidationProblemInstance> RemovePackage(Guid fromId, Guid toId, string roleCode, Guid packageId, CancellationToken cancellationToken = default);
}
