﻿using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessMgmt.Persistence.Services;

/// <inheritdoc/>
public class AssignmentService(
    IInheritedAssignmentRepository inheritedAssignmentRepository,
    IAssignmentRepository assignmentRepository,
    IPackageRepository packageRepository,
    IAssignmentPackageRepository assignmentPackageRepository,
    IAssignmentResourceRepository assignmentResourceRepository,
    IRoleRepository roleRepository,
    IRolePackageRepository rolePackageRepository,
    IEntityRepository entityRepository
    ) : IAssignmentService
{
    private readonly IAssignmentRepository assignmentRepository = assignmentRepository;
    private readonly IInheritedAssignmentRepository inheritedAssignmentRepository = inheritedAssignmentRepository;
    private readonly IPackageRepository packageRepository = packageRepository;
    private readonly IAssignmentPackageRepository assignmentPackageRepository = assignmentPackageRepository;
    private readonly IRoleRepository roleRepository = roleRepository;
    private readonly IRolePackageRepository rolePackageRepository = rolePackageRepository;
    private readonly IEntityRepository entityRepository = entityRepository;

    /// <inheritdoc/>
    public async Task<Assignment> GetAssignment(Guid fromId, Guid toId, Guid roleId)
    {
        var filter = assignmentRepository.CreateFilterBuilder();
        filter.Equal(t => t.FromId, fromId);
        filter.Equal(t => t.ToId, toId);
        filter.Equal(t => t.RoleId, roleId);

        var result = await assignmentRepository.Get(filter);
        if (result == null || !result.Any())
        {
            return null;
        }

        return result.First();
    }

    /// <inheritdoc/>
    public async Task<Assignment> GetAssignment(Guid fromId, Guid toId, string roleCode)
    {
        var roleResult = await roleRepository.Get(t => t.Code, roleCode);
        if (roleResult == null || !roleResult.Any())
        {
            return null;
        }

        return await GetAssignment(fromId, toId, roleResult.First().Id);
    }

    /// <inheritdoc/>
    public async Task<bool> AddPackageToAssignment(Guid userId, Guid assignmentId, Guid packageId)
    {
        /*
        [X] Check if user is TS
        [X] Check if user assignment.roles has packages
        [X] Check if user assignment.assignmentpackages has package
        [?] Check if users has packages delegated?

        [ ] Check if package can be delegated
        */

        var user = await entityRepository.Get(userId);

        var assignment = await assignmentRepository.Get(assignmentId);

        /* TODO: Future Sjekk om bruker er Tilgangsstyrer */

        var package = await packageRepository.Get(packageId);

        var userAssignmentFilter = assignmentRepository.CreateFilterBuilder();
        userAssignmentFilter.Equal(t => t.FromId, assignment.FromId);
        userAssignmentFilter.Equal(t => t.ToId, userId);
        var userAssignments = await assignmentRepository.Get(userAssignmentFilter);

        bool hasPackage = false;

        foreach (var userAssignment in userAssignments)
        {
            var assignmentPackages = await assignmentPackageRepository.GetB(userAssignment.Id);
            if (assignmentPackages != null && assignmentPackages.Count(t => t.Id == packageId) > 0)
            {
                hasPackage = true;
                break;
            }
        }

        if (!hasPackage)
        {
            // Check if AssigmentRole=>RolePackage has package
            foreach (var roleId in userAssignments.Select(t => t.RoleId).Distinct())
            {
                var rolePackResult = await rolePackageRepository.Get(t => t.RoleId, roleId);
                if (rolePackResult != null && rolePackResult.Count(t => t.PackageId == packageId) > 0)
                {
                    hasPackage = true;
                    break;
                }
            }
        }

        if (!hasPackage)
        {
            throw new Exception(string.Format("User '{0}' does not have package '{1}'", user.Name, package.Name));
        }

        await assignmentPackageRepository.Create(new AssignmentPackage()
        {
            AssignmentId = assignmentId,
            PackageId = packageId
        });

        return true;
    }

    /// <inheritdoc/>
    public Task<bool> AddResourceToAssignment(Guid userId, Guid assignmentId, Guid resourceId)
    {
        /*
        [ ] Check if user is TS
        [ ] Check if resource can be delegated
        [ ] Check if user assignment.assignmentpackages has resources
        [ ] Check if user assignment.roles has packages
        [ ] Check if users has packages delegated?
        */

        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task<Result<Assignment>> GetOrCreateAssignmen2(Guid fromEntityId, Guid toEntityId, string roleCode)
    {
        var roleResult = await roleRepository.Get(t => t.Name, roleCode);
        if (roleResult == null || !roleResult.Any())
        {
            return CoreErrors.MissingRoleCode(roleCode);
        }

        return await GetOrCreateAssignment2(fromEntityId, toEntityId, roleResult.First().Id);
    }

    private async Task<Result<Assignment>> GetOrCreateAssignment2(Guid fromEntityId, Guid toEntityId, Guid roleId)
    {
        var assignment = await GetAssignment(fromEntityId, toEntityId, roleId);
        if (assignment != null)
        {
            return assignment;
        }

        var role = await roleRepository.Get(roleId);
        if (role == null)
        {
            return CoreErrors.MissingRoleId(roleId);
        }

        var inheritedAssignments = await GetInheritedAssignment(fromEntityId, toEntityId, role.Id);
        if (inheritedAssignments != null && inheritedAssignments.Any())
        {
            return CoreErrors.AssignmentExists(fromEntityId, toEntityId, roleId);
        }

        await assignmentRepository.Create(new Assignment()
        {
            FromId = fromEntityId,
            ToId = toEntityId,
            RoleId = role.Id
        });

        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task<Assignment> GetOrCreateAssignment(Guid fromEntityId, Guid toEntityId, string roleCode)
    {
        var roleResult = await roleRepository.Get(t => t.Name, roleCode);
        if (roleResult == null || !roleResult.Any())
        {
            throw new Exception(string.Format("Role '{0}' not found", roleCode));
        }

        return await GetOrCreateAssignment(fromEntityId, toEntityId, roleResult.First().Id);
    }

    /// <inheritdoc/>
    public async Task<Assignment> GetOrCreateAssignment(Guid fromEntityId, Guid toEntityId, Guid roleId)
    {
        var assignment = await GetAssignment(fromEntityId, toEntityId, roleId);
        if (assignment != null)
        {
            return assignment;
        }

        var role = await roleRepository.Get(roleId);
        if (role == null)
        {
            throw new Exception(string.Format("Role '{0}' not found", roleId));
        }

        var inheritedAssignments = await GetInheritedAssignment(fromEntityId, toEntityId, role.Id);
        if (inheritedAssignments != null && inheritedAssignments.Any())
        {
            if (inheritedAssignments.Count() == 1)
            {
                throw new Exception(string.Format("An inheirited assignment exists From:'{0}.FromName' Via:'{0}.ViaName' To:'{}.ToName'. Use Force = true to create anyway.", inheritedAssignments.First()));
            }

            throw new Exception(string.Format("Multiple inheirited assignment exists. Use Force = true to create anyway."));
        }

        await assignmentRepository.Create(new Assignment()
        {
            FromId = fromEntityId,
            ToId = toEntityId,
            RoleId = role.Id
        });

        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InheritedAssignment>> GetInheritedAssignment(Guid fromId, Guid toId, Guid roleId)
    {
        var filter = inheritedAssignmentRepository.CreateFilterBuilder();
        filter.Equal(t => t.FromId, fromId);
        filter.Equal(t => t.ToId, toId);
        filter.Equal(t => t.RoleId, roleId);

        return await inheritedAssignmentRepository.Get(filter);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InheritedAssignment>> GetInheritedAssignment(Guid fromId, Guid toId, string roleCode)
    {
        var roleResult = await roleRepository.Get(t => t.Code, roleCode);
        if (roleResult == null || !roleResult.Any())
        {
            throw new Exception(string.Format("Role not found '{0}'", roleCode));
        }

        var roleId = roleResult.First().Id;
        return await GetInheritedAssignment(fromId, toId, roleId);
    }
}
