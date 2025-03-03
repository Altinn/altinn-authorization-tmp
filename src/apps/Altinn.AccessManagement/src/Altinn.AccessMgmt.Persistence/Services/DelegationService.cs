using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Contracts;

namespace Altinn.AccessMgmt.Persistence.Services;

/// <inheritdoc/>
public class DelegationService(
    IRoleRepository roleRepository,
    IInheritedAssignmentRepository inheritedAssignmentRepository,
    IAssignmentRepository assignmentRepository,
    IAssignmentPackageRepository assignmentPackageRepository,
    IAssignmentResourceRepository assignmentResourceRepository,
    IRolePackageRepository rolePackageRepository,
    IRoleResourceRepository roleResourceRepository,
    IDelegationRepository delegationRepository, 
    IPackageRepository packageRepository,
    IResourceRepository resourceRepository,
    IDelegationPackageRepository delegationPackageRepository,
    IDelegationResourceRepository delegationResourceRepository
    ) : IDelegationService
{
    private readonly IRoleRepository roleRepository = roleRepository;
    private readonly IInheritedAssignmentRepository inheritedAssignmentRepository = inheritedAssignmentRepository;
    private readonly IAssignmentRepository assignmentRepository = assignmentRepository;
    private readonly IAssignmentPackageRepository assignmentPackageRepository = assignmentPackageRepository;
    private readonly IAssignmentResourceRepository assignmentResourceRepository = assignmentResourceRepository;
    private readonly IRolePackageRepository rolePackageRepository = rolePackageRepository;
    private readonly IRoleResourceRepository roleResourceRepository = roleResourceRepository;
    private readonly IDelegationRepository delegationRepository = delegationRepository;
    private readonly IPackageRepository packageRepository = packageRepository;
    private readonly IResourceRepository resourceRepository = resourceRepository;
    private readonly IDelegationPackageRepository delegationPackageRepository = delegationPackageRepository;
    private readonly IDelegationResourceRepository delegationResourceRepository = delegationResourceRepository;

    private async Task<bool> CheckIfEntityHasRole(string roleCode, Guid fromId, Guid toId)
    {
        var role = (await roleRepository.Get(t => t.Code, roleCode)).First();

        var filter = inheritedAssignmentRepository.CreateFilterBuilder();
        filter.Equal(t => t.FromId, fromId);
        filter.Equal(t => t.ToId, toId);
        filter.Equal(t => t.RoleId, role.Id);

        var userAssignments = await assignmentRepository.Get();

        if (userAssignments == null || !userAssignments.Any())
        {
            return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public async Task<ExtDelegation> CreateDelgation(Guid fromAssignmentId, Guid toAssignmentId)
    {
        var fromAssignment = await assignmentRepository.Get(fromAssignmentId);
        var toAssignment = await assignmentRepository.Get(toAssignmentId);

        if (fromAssignment.ToId != toAssignment.FromId) 
        {
            throw new InvalidOperationException("Assignments are not connected. FromAssignment.ToId != ToAssignment.FromId");
        }

        var delegation = new Delegation()
        {
            Id = Guid.NewGuid(),
            FromId = fromAssignmentId,
            ToId = toAssignmentId
        };

        var res = await delegationRepository.Create(delegation);
        if (res == 0)
        {
            throw new Exception("Failed to create delegation");
        }

        return await delegationRepository.GetExtended(delegation.Id);
    }

    /// <inheritdoc/>
    public async Task<bool> AddPackageToDelegation(Guid userId, Guid delegationId, Guid packageId)
    {
        /*
        [ ] Check i Pacakge is Delegable
        */

        /* 
        [X] Check if user is DelegationAdmin on ViaId 
        [X] Check if assignment has the package
        [X] Check if the assignment role has the package
        */

        var package = await packageRepository.Get(packageId);

        var delegation = await delegationRepository.GetExtended(delegationId);
        var fromAssignment = await assignmentRepository.GetExtended(delegation.FromId);
        var toAssignment = await assignmentRepository.GetExtended(delegation.ToId);

        var userHasTS = await CheckIfEntityHasRole("TS", toAssignment.FromId, userId);

        if (!userHasTS) 
        {
            throw new Exception($"{toAssignment.To.Name}' is not TS on '{toAssignment.From.Name}'");
        }

        var assignmentPackages = await assignmentPackageRepository.GetB(fromAssignment.Id);
        var rolePackages = await rolePackageRepository.Get(t => t.RoleId, fromAssignment.RoleId);

        if (assignmentPackages.Count(t => t.Id == packageId) == 0 && rolePackages.Count(t => t.Id == packageId) == 0) 
        {
            throw new Exception($"The source assignment does not have the package '{package.Name}'");
        }

        var res = await delegationPackageRepository.Create(new DelegationPackage() { 
            Id = Guid.NewGuid(),
            DelegationId = delegationId,
            PackageId = packageId
        });

        return res > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> AddResourceToDelegation(Guid userId, Guid delegationId, Guid resourceId)
    {
        /*
        [ ] Check i Pacakge is Delegable (?)
        [ ] Check i Resource is Delegable
        */

        /* 
        [X] Check if user is DelegationAdmin on FromId 
        [X] Check if assignment has the resource
        [X] Check if the assignment role has the resource
        [X] Check if the assignment packages has the resource
        */

        var resource = await resourceRepository.Get(resourceId);

        var delegation = await delegationRepository.GetExtended(delegationId);
        var fromAssignment = await assignmentRepository.GetExtended(delegation.FromId);
        var toAssignment = await assignmentRepository.GetExtended(delegation.ToId);

        var userHasTS = await CheckIfEntityHasRole("TS", toAssignment.FromId, userId);

        if (!userHasTS)
        {
            throw new Exception($"{toAssignment.To.Name}' is not TS on '{toAssignment.From.Name}'");
        }

        var assignmentResources = await assignmentResourceRepository.GetB(fromAssignment.Id);
        var roleResources = await roleResourceRepository.GetB(fromAssignment.RoleId);
        var rolePackages = await rolePackageRepository.Get(t => t.RoleId, fromAssignment.RoleId);
        var rolePackageResources = new Dictionary<Guid, List<Resource>>();
        foreach (var package in rolePackages)
        {
            rolePackageResources.Add(package.Id, [.. await roleResourceRepository.GetB(resourceId)]);
        }

        if (assignmentResources.Count(t => t.Id == resourceId) == 0 
            && roleResources.Count(t => t.Id == resourceId) == 0
            && rolePackageResources.SelectMany(t => t.Value).Count(t => t.Id == resourceId) == 0
            )
        {
            throw new Exception($"The source assignment does not have the resource '{resource.Name}'");
        }


        var res = await delegationResourceRepository.Create(new DelegationResource()
        {
            Id = Guid.NewGuid(),
            DelegationId = delegationId,
            ResourceId = resourceId
        });

        return res > 0;
    }
}

/// <inheritdoc/>
public class AssignmentService(
    IInheritedAssignmentRepository inheritedAssignmentRepository,
    IAssignmentRepository assignmentRepository,
    IPackageRepository packageRepository,
    IAssignmentPackageRepository assignmentPackageRepository,
    IAssignmentResourceRepository assignmentResourceRepository,
    IRoleRepository roleRepository,
    IRolePackageRepository rolePackageRepository
    ) : IAssignmentService
{
    private readonly IAssignmentRepository assignmentRepository = assignmentRepository;
    private readonly IInheritedAssignmentRepository inheritedAssignmentRepository = inheritedAssignmentRepository;
    private readonly IPackageRepository packageRepository = packageRepository;
    private readonly IAssignmentPackageRepository assignmentPackageRepository = assignmentPackageRepository;
    private readonly IAssignmentResourceRepository assignmentResourceRepository = assignmentResourceRepository;
    private readonly IRoleRepository roleRepository = roleRepository;
    private readonly IRolePackageRepository rolePackageRepository = rolePackageRepository;

    /// <inheritdoc/>
    public async Task<Assignment> GetAssignment(Guid fromId, Guid toId, Guid roleId)
    {
        var result = await assignmentRepository.Get();
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



        var assignment = await assignmentRepository.Get(assignmentId);
        var res = await GetAssignment(assignment.FromId, userId, "TS");
        if (res == null)
        {
            throw new Exception("User is not TS");
        }

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
            throw new Exception("User '' does not have package");
        }

        await assignmentPackageRepository.Create(new AssignmentPackage()
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            PackageId = packageId
        });

        return true;
    }

    /*
    
    THE VIEW

    From,To,Via,FromAss,ToAss,FromAssRole,ToAssRole,

    */

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
    public async Task<Assignment> GetOrCreateAssignment(Guid fromEntityId, Guid toEntityId, string roleCode)
    {
        var assignment = await GetAssignment(fromEntityId, toEntityId, roleCode);
        if (assignment != null)
        {
            return assignment;
        }

        var roleResult = await roleRepository.Get(t => t.Code, roleCode);
        if (roleResult == null)
        {
            throw new Exception("Role '' not found");
        }


        //// here ....

        await assignmentRepository.Create(new Assignment()
        {
            Id= Guid.NewGuid(),
            FromId= fromEntityId,
            ToId= toEntityId,
            RoleId = roleResult.First().Id
        });

        throw new NotImplementedException();
    }
}
