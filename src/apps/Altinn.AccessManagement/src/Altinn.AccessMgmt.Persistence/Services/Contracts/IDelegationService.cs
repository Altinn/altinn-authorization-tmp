using Altinn.AccessMgmt.Core.Models;

namespace Altinn.AccessMgmt.Persistence.Services.Contracts;

/// <summary>
/// Delegation service
/// </summary>
public interface IDelegationService
{
    /// <summary>
    /// Create a new delegation betweeen two assignments
    /// </summary>
    /// <param name="userId">User/param>
    /// <param name="fromAssignmentId">From</param>
    /// <param name="toAssignmentId">To</param>
    /// <returns></returns>
    Task<ExtDelegation> CreateDelgation(Guid userId, Guid fromAssignmentId, Guid toAssignmentId);

    /// <summary>
    /// Adds a package to the delegation
    /// </summary>
    /// <returns></returns>
    Task<bool> AddPackageToDelegation(Guid userId, Guid delegationId, Guid packageId);

    /// <summary>
    /// Adds a resource to the delegation
    /// </summary>
    /// <returns></returns>
    Task<bool> AddResourceToDelegation(Guid userId, Guid delegationId, Guid resourceId);
}

/// <summary>
/// Assignment service
/// </summary>
public interface IAssignmentService
{
    /// <summary>
    /// Gets assignment and creates if not exits
    /// </summary>
    /// <returns></returns>
    Task<Assignment> GetOrCreateAssignment(Guid fromId, Guid toId, string roleCode);

    /// <summary>
    /// Gets assignment and creates if not exits
    /// </summary>
    /// <returns></returns>
    Task<Assignment> GetOrCreateAssignment(Guid fromId, Guid toId, Guid roleId);

    /// <summary>
    /// Adds a package to the delegation
    /// </summary>
    /// <returns></returns>
    Task<bool> AddPackageToAssignment(Guid userId, Guid assignmentId, Guid packageId);

    /// <summary>
    /// Adds a resource to the delegation
    /// </summary>
    /// <returns></returns>
    Task<bool> AddResourceToAssignment(Guid userId, Guid assignmentId, Guid resourceId);

    Task<Assignment> GetAssignment(Guid fromId, Guid toId, Guid roleId);
    
    Task<Assignment> GetAssignment(Guid fromId, Guid toId, string roleCode);

    Task<IEnumerable<InheritedAssignment>> GetInheritedAssignment(Guid fromId, Guid toId, Guid roleId);
    
    Task<IEnumerable<InheritedAssignment>> GetInheritedAssignment(Guid fromId, Guid toId, string roleCode);
}
