using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Repositories;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Models;

namespace Altinn.AccessMgmt.Persistence.Services;

/// <summary>
/// Service for managing connections.
/// </summary>
public class ConnectionService(
    IConnectionRepository connectionRepository,
    IConnectionPackageRepository connectionPackageRepository,
    IConnectionResourceRepository connectionResourceRepository,
    IAssignmentService assignmentService,
    IRoleRepository roleRepository,
    IAssignmentRepository assignmentRepository,
    IDelegationRepository delegationRepository,
    IAssignmentPackageRepository assignmentPackageRepository,
    IDelegationPackageRepository delegationPackageRepository
    ) : IConnectionService
{
    
    /// <inheritdoc />
    public async Task<IEnumerable<ExtConnection>> GetGiven(Guid id)
    {
        var filter = connectionRepository.CreateFilterBuilder();
        filter.Equal(t => t.FromId, id);
        return await connectionRepository.GetExtended(filter);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ExtConnection>> GetRecived(Guid id)
    {
        var filter = connectionRepository.CreateFilterBuilder();
        filter.Equal(t => t.ToId, id);
        return await connectionRepository.GetExtended(filter);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ExtConnection>> GetFacilitated(Guid id)
    {
        var filter = connectionRepository.CreateFilterBuilder();
        filter.Equal(t => t.FacilitatorId, id);
        return await connectionRepository.GetExtended(filter);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ExtConnection>> GetSpecific(Guid fromId, Guid toId)
    {
        var filter = connectionRepository.CreateFilterBuilder();
        filter.Equal(t => t.FromId, fromId);
        filter.Equal(t => t.ToId, toId);
        return await connectionRepository.GetExtended(filter);
    }

    /// <inheritdoc />
    public async Task<ExtConnection> Get(Guid Id)
    {
        var filter = connectionRepository.CreateFilterBuilder();
        filter.Equal(t => t.Id, Id);
        var res = await connectionRepository.GetExtended(filter);
        return res.FirstOrDefault();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ConnectionPackage>> GetPackages(Guid? fromId, Guid? toId)
    {
        return await GetConnectionPackages(fromId, toId);
    }

    /// <inheritdoc />
    public async Task<bool> AddPackage(Guid fromId, Guid toId, string roleCode, Guid packageId, ChangeRequestOptions options)
    {
        //// Will create assignment even if GetOrCreateAssignmentPackage fails.
        var assignment = await assignmentService.GetOrCreateAssignment(fromId, toId, roleCode, options);
        var res = await GetOrCreateAssignmentPackage(assignment.Id, packageId, options);
        if (res == null)
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> RemovePackage(Guid fromId, Guid toId, string roleCode, Guid packageId, ChangeRequestOptions options)
    {
        var assignment = await assignmentService.GetAssignment(fromId, toId, roleCode);
        if (assignment == null)
        {
            Console.WriteLine("Nothing to delete");
            return true;
        }

        return await RemoveAssignmentPackage(assignment.Id, packageId, options);
    }

    /// <inheritdoc />
    public async Task<bool> AddPackage(Guid connectionId, Guid packageId, ChangeRequestOptions options)
    {
        //// Fix with new connection model

        var assignment = assignmentRepository.Get(connectionId);
        var delegation = delegationRepository.Get(connectionId);
        
        if (assignment != null)
        {
            var res = await GetOrCreateAssignmentPackage(connectionId, packageId, options);
            return true;
        }
        
        if (delegation != null)
        {
            var res = await GetOrCreateDelegationPackage(connectionId, packageId, options);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public async Task<bool> RemovePackage(Guid connectionId, Guid packageId, ChangeRequestOptions options)
    {
        //// Fix with new connection model

        var assignment = assignmentRepository.Get(connectionId);
        var delegation = delegationRepository.Get(connectionId);

        if (assignment != null)
        {
            var res = await RemoveAssignmentPackage(connectionId, packageId, options);
            return true;
        }

        if (delegation != null)
        {
            var res = await RemoveDelegationPackage(connectionId, packageId, options);
            return true;
        }

        return false;
    }
    
    private async Task<IEnumerable<ConnectionPackage>> GetConnectionPackages(Guid? fromId, Guid? toId)
    {
        /* Get packages assigned to entity from a specific entity, to see if entity can assign it to another entity */

        /*
        Get all connectionpackages
        Check if package is in list
        FUTURE: Check if connectionPackage.IsAssignable and connectionPackage.CanAssign
        */

        var filter = connectionPackageRepository.CreateFilterBuilder();
        filter.Equal(t => t.FromId, fromId);
        filter.Equal(t => t.ToId, toId);
        return await connectionPackageRepository.Get(filter);

    }

    private async Task<IEnumerable<ExtConnectionPackage>> GetConnectionPackages(Guid fromId, Guid toId, Guid packageId)
    {
        /* Get packages assigned to entity from a specific entity, to see if entity can assign it to another entity */

        var filter = connectionPackageRepository.CreateFilterBuilder();
        filter.Equal(t => t.FromId, fromId);
        filter.Equal(t => t.ToId, toId);
        filter.Equal(t => t.PackageId, packageId);
        return await connectionPackageRepository.GetExtended(filter);

    }

    private async Task<bool> RemoveAssignmentPackage(Guid assignmentId, Guid packageId, ChangeRequestOptions options)
    {
        var assignment = await assignmentRepository.Get(assignmentId);

        if (assignment == null)
        {
            throw new Exception("Assignment not found");
        }

        var userPackages = await GetConnectionPackages(assignment.FromId, options.ChangedBy, packageId);

        if (!userPackages.Any())
        {
            throw new Exception("User does not have the package assigned on this entity");
        }

        // User can also UnAssign?
        if (!userPackages.Any(t => t.CanAssign))
        {
            throw new Exception("User can't assign package");
        }

        var filter = assignmentPackageRepository.CreateFilterBuilder();
        filter.Equal(t => t.AssignmentId, assignmentId);
        filter.Equal(t => t.PackageId, packageId);
        var checkResult = await assignmentPackageRepository.Get();
        if (checkResult == null && !checkResult.Any())
        {
            Console.WriteLine("AssignmentPackage does not exist, nothing to delete.");
            return true;
        }

        var deleteResult = await assignmentPackageRepository.DeleteCross(assignmentId, packageId, options);
        if (deleteResult > 0)
        {
            return true;
        }

        return false;
    }

    private async Task<bool> RemoveDelegationPackage(Guid delegationId, Guid packageId, ChangeRequestOptions options)
    {
        var delegation = await delegationRepository.Get(delegationId);

        if (delegation == null)
        {
            throw new Exception("Delegation not found");
        }

        // other logic?
        var userPackages = await GetConnectionPackages(delegation.FromId, options.ChangedBy, packageId);

        if (!userPackages.Any())
        {
            throw new Exception("User does not have the package assigned on this entity");
        }

        //if (!userPackages.Any(t => t.CanDelegate))
        //{
        //    throw new Exception("User can't delegate package");
        //}


        var filter = delegationPackageRepository.CreateFilterBuilder();
        filter.Equal(t => t.DelegationId, delegationId);
        filter.Equal(t => t.PackageId, packageId);
        var checkResult = await delegationPackageRepository.Get();
        if (checkResult == null && !checkResult.Any())
        {
            Console.WriteLine("DelegationPackage does not exist, nothing to delete.");
            return true;
        }

        var deleteResult = await delegationPackageRepository.DeleteCross(delegationId, packageId, options);
        if (deleteResult > 0)
        {
            return true;
        }

        return false;
    }

    private async Task<AssignmentPackage> GetOrCreateAssignmentPackage(Guid assignmentId, Guid packageId, ChangeRequestOptions options)
    {
        var assignment = await assignmentRepository.Get(assignmentId);

        if (assignment == null)
        {
            throw new Exception("Assignment not found");
        }

        var userPackages = await GetConnectionPackages(assignment.FromId, options.ChangedBy, packageId);

        if (!userPackages.Any())
        {
            throw new Exception("User does not have the package assigned on this entity");
        }

        if (!userPackages.Any(t => t.CanAssign))
        {
            throw new Exception("User can't assign package");
        }

        if (!userPackages.Any(t => t.Package.IsAssignable))
        {
            throw new Exception("Package is not assignable");
        }

        var filter = assignmentPackageRepository.CreateFilterBuilder();
        filter.Equal(t => t.AssignmentId, assignmentId);
        filter.Equal(t => t.PackageId, packageId);
        var checkResult = await assignmentPackageRepository.Get();
        if (checkResult != null && checkResult.Any())
        {
            return checkResult.First();
        }

        var createResult = await assignmentPackageRepository.CreateCross(assignmentId, packageId, options);
        if (createResult > 0)
        {
            var createCheckResult = await assignmentPackageRepository.Get();
            if (createCheckResult != null && createCheckResult.Any())
            {
                return createCheckResult.First();
            }
        }

        throw new Exception(string.Format("Unable to add package to assignment ('{0}','{1}')", assignmentId, packageId));
    }

    private async Task<DelegationPackage> GetOrCreateDelegationPackage(Guid delegationId, Guid packageId, ChangeRequestOptions options)
    {
        var delegation = await delegationRepository.Get(delegationId);

        if (delegation == null)
        {
            throw new Exception("Delegation not found");
        }

        // other logic?
        var userPackages = await GetConnectionPackages(delegation.FromId, options.ChangedBy, packageId);

        if (!userPackages.Any())
        {
            throw new Exception("User does not have the package assigned on this entity");
        }

        if (!userPackages.Any(t => t.CanAssign))
        {
            throw new Exception("User can't assign package");
        }

        if (!userPackages.Any(t => t.Package.IsAssignable))
        {
            throw new Exception("Package is not assignable");
        }


        var filter = delegationPackageRepository.CreateFilterBuilder();
        filter.Equal(t => t.DelegationId, delegationId);
        filter.Equal(t => t.PackageId, packageId);
        var checkResult = await delegationPackageRepository.Get();
        if (checkResult != null && checkResult.Any())
        {
            return checkResult.First();
        }

        var createResult = await delegationPackageRepository.CreateCross(delegationId, packageId, options);
        if (createResult > 0)
        {
            var createCheckResult = await delegationPackageRepository.Get();
            if (createCheckResult != null && createCheckResult.Any())
            {
                return createCheckResult.First();
            }
        }

        throw new Exception(string.Format("Unable to add package to delegation ('{0}','{1}')", delegationId, packageId));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Resource>> GetResources(Guid id)
    {
        return await connectionResourceRepository.GetB(id);
    }
}

/// <summary>
/// Convert database models to dto models
/// </summary>
public static class ConnectionConverter
{
    /// <summary>
    /// Convert database model to response model
    /// </summary>
    public static CreateDelegationResponse ConvertToResponseModel(Connection connection)
    {
        return new CreateDelegationResponse()
        {
            DelegationId = connection.Id,
            FromEntityId = connection.FromId
        };
    }

    /// <summary>
    /// Convert database model to dto model
    /// </summary>
    public static ConnectionDto ConvertToDto(ExtConnection connection)
    {
        return new ConnectionDto()
        {
            Id = connection.Id,
            From = connection.From,
            To = connection.To,
            Facilitator = connection.Facilitator,
            Role = ConvertToDto(connection.Role),
            FacilitatorRole = ConvertToDto(connection.FacilitatorRole),
            Delegation = connection.Delegation
        };
    }

    /// <summary>
    /// Convert database model to dto model
    /// </summary>
    public static RoleDto ConvertToDto(Role role)
    {
        return new RoleDto()
        {
            Id = role.Id,
            Description = role.Description,
            Name = role.Name,
            Code = role.Code,
            Urn = role.Urn,
            IsKeyRole = role.IsKeyRole
        };
    }
}
