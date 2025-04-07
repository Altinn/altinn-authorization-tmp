using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Models;

namespace Altinn.AccessMgmt.Persistence.Services.Contracts;

/// <summary>
/// Assignment service
/// </summary>
public interface IAssignmentService
{
    /// <summary>
    /// Gets assignment and creates if not exits
    /// </summary>
    /// <returns></returns>
    Task<Assignment> GetOrCreateAssignment(Guid fromId, Guid toId, string roleCode, ChangeRequestOptions options);

    /// <summary>
    /// Gets assignment and creates if not exits
    /// </summary>
    /// <returns></returns>
    Task<Assignment> GetOrCreateAssignment(Guid fromId, Guid toId, Guid roleId, ChangeRequestOptions options);

    /// <summary>
    /// Adds a package to the delegation
    /// </summary>
    /// <returns></returns>
    Task<bool> AddPackageToAssignment(Guid userId, Guid assignmentId, Guid packageId, ChangeRequestOptions options);

    /// <summary>
    /// Adds a resource to the delegation
    /// </summary>
    /// <returns></returns>
    Task<bool> AddResourceToAssignment(Guid userId, Guid assignmentId, Guid resourceId, ChangeRequestOptions options);

    Task<Assignment> GetAssignment(Guid fromId, Guid toId, Guid roleId);
    
    Task<Assignment> GetAssignment(Guid fromId, Guid toId, string roleCode);

    Task<IEnumerable<InheritedAssignment>> GetInheritedAssignment(Guid fromId, Guid toId, Guid roleId);
    
    Task<IEnumerable<InheritedAssignment>> GetInheritedAssignment(Guid fromId, Guid toId, string roleCode);
}
