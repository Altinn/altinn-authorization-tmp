using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Helpers;
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

        var isFromSet = SetFrom(from, filter);
        var isToSet = SetTo(to, filter);
        if (isFromSet || isToSet)
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
                filter.Equal(f => f.FromId, from);
                return true;
            }

            filter.NotSet(f => f.FromId);
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

        var isFromSet = SetFrom(from, filter);
        var isToSet = SetTo(to, filter);
        if (isFromSet || isToSet)
        {
            filter.NotSet(t => t.Id);
            filter.NotSet(t => t.RoleId);
            filter.NotSet(t => t.FacilitatorId);
        }
        else
        {
            Unreachable();
        }

        var result = await ConnectionPackageRepository.Get(filter, cancellationToken: cancellationToken);
        return result.ToList();
    }

    /// <inheritdoc />
    public async Task<Result<AssignmentPackage>> AddPackage(Guid from, Guid to, string roleCode, string packageUrn, CancellationToken cancellationToken = default)
    {
        var filter = PackageRepository.CreateFilterBuilder()
            .Add(t => t.Urn, packageUrn, FilterComparer.EndsWith);

        var packages = await PackageRepository.Get(filter, cancellationToken: cancellationToken);
        var problem = ValidationRules.Validate(ValidationRules.QueryParameters.PackageUrnLookup(packages));
        if (problem is { })
        {
            return problem;
        }

        var package = packages.First();
        return await AddPackage(from, to, roleCode, package.Id, "packageUrn", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<AssignmentPackage>> AddPackage(Guid from, Guid to, string roleCode, Guid packageId, CancellationToken cancellationToken = default)
    {
        return await AddPackage(from, to, roleCode, packageId, "packageId", cancellationToken);
    }

    private async Task<Result<AssignmentPackage>> AddPackage(Guid from, Guid to, string roleCode, Guid packageId, string queryParamName, CancellationToken cancellationToken)
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
        problem = ValidationRules.Validate(ValidationRules.QueryParameters.VerifyAssignmentRoleExists(existingAssignments, roleCode));
        if (problem is { })
        {
            return problem;
        }

        var assignment = existingAssignments.First();

        var userPackageFilter = ConnectionPackageRepository.CreateFilterBuilder()
            .Equal(t => t.ToId, DbAudit.Value.ChangedBy)
            .Equal(t => t.FromId, from)
            .Equal(t => t.PackageId, packageId);

        var userPackags = await ConnectionPackageRepository.GetExtended(userPackageFilter, cancellationToken: cancellationToken);
        var userpackage = userPackags.Where(p => p.PackageId == packageId)
            .ToList();

        problem = ValidationRules.Validate(
            ValidationRules.QueryParameters.AnyPackages(userpackage, queryParamName),
            ValidationRules.QueryParameters.PackageIsAssignableByUser(userpackage, queryParamName),
            ValidationRules.QueryParameters.PackageIsAssignableByDefinition(userpackage, queryParamName)
        );

        if (problem is { })
        {
            return problem;
        }

        var assignmentPackageFilter = AssignmentPackageRepository
            .CreateFilterBuilder()
            .Equal(t => t.AssignmentId, assignment.Id)
            .Equal(t => t.PackageId, packageId);

        var existingPackageAssignment = await AssignmentPackageRepository.Get(assignmentPackageFilter, cancellationToken: cancellationToken);
        if (existingPackageAssignment != null && existingPackageAssignment.Any())
        {
            return existingPackageAssignment.First();
        }

        var createResult = await AssignmentPackageRepository.Create(new AssignmentPackage() { AssignmentId = assignment.Id, PackageId = packageId }, DbAudit.Value, cancellationToken: cancellationToken);
        if (createResult == 0)
        {
            Unreachable();
        }

        var createCheckResult = await AssignmentPackageRepository.Get(t => t.AssignmentId, assignment.Id, cancellationToken: cancellationToken);
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

        var checkResult = await AssignmentPackageRepository.Get(packageFilter, cancellationToken: cancellationToken);
        if (checkResult == null && !checkResult.Any())
        {
            return null;
        }

        var deleteResult = await AssignmentPackageRepository.Delete(packageFilter, DbAudit.Value, cancellationToken: cancellationToken);
        if (deleteResult == 0)
        {
            return null;
        }

        return null;
    }

    private ValidationProblemInstance? ValidateAssignmentData(ExtEntity entityFrom, ExtEntity entityTo, IEnumerable<Role> roles)
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
    Task<Result<List<ExtConnection>>> Get(Guid? from = null, Guid? to = null, CancellationToken cancellationToken = default);

    Task<Result<Assignment>> AddAssignment(Guid from, Guid to, string roleCode, CancellationToken cancellationToken = default);

    Task<ValidationProblemInstance> RemoveAssignment(Guid from, Guid to, string roleCode, bool cascade, CancellationToken cancellationToken = default);

    Task<Result<List<ConnectionPackage>>> GetPackages(Guid? from, Guid? to, CancellationToken cancellationToken = default);

    Task<Result<AssignmentPackage>> AddPackage(Guid fromId, Guid toId, string rolecode, Guid packageId, CancellationToken cancellationToken = default);

    Task<Result<AssignmentPackage>> AddPackage(Guid fromId, Guid toId, string rolecode, string packageUrn, CancellationToken cancellationToken = default);

    Task<ValidationProblemInstance> RemovePackage(Guid fromId, Guid toId, string roleCode, Guid packageId, CancellationToken cancellationToken = default);
}
