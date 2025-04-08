using Altinn.AccessMgmt.Core.Models;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessMgmt.Persistence.Services.Contracts;

/// <summary>
/// Assignment service
/// </summary>
public interface IAssignmentService
{
    /// <summary>
    /// Gets assignment and creates if not exists.
    /// </summary>
    Task<Result<Assignment>> GetOrCreateAssignment(Guid fromEntityId, Guid toEntityId, string roleCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes and assignment.
    /// </summary>
    Task<Result<Assignment>> DeleteAssignment(Guid fromEntityId, Guid toEntityId, string roleCode, bool cascade, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets assignment and creates if not exits
    /// </summary>
    Task<Assignment> GetOrCreateAssignment(Guid fromId, Guid toId, string roleCode);

    /// <summary>
    /// Gets assignment and creates if not exits
    /// </summary>
    Task<Assignment> GetOrCreateAssignment(Guid fromId, Guid toId, Guid roleId);

    /// <summary>
    /// Adds a package to the delegation
    /// </summary>
    Task<bool> AddPackageToAssignment(Guid userId, Guid assignmentId, Guid packageId);

    /// <summary>
    /// Adds a resource to the delegation
    /// </summary>
    Task<bool> AddResourceToAssignment(Guid userId, Guid assignmentId, Guid resourceId);

    /// <summary>
    /// Fetches assignment.
    /// </summary>
    Task<Assignment> GetAssignment(Guid fromId, Guid toId, Guid roleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches assignment.
    /// </summary>
    Task<Assignment> GetAssignment(Guid fromId, Guid toId, string roleCode);

    /// <summary>
    /// Fetches inherited assignments.
    /// </summary>
    Task<IEnumerable<InheritedAssignment>> GetInheritedAssignment(Guid fromId, Guid toId, Guid roleId);

    /// <summary>
    /// Fetches inherited assignments.
    /// </summary>
    Task<IEnumerable<InheritedAssignment>> GetInheritedAssignment(Guid fromId, Guid toId, string roleCode);
}
