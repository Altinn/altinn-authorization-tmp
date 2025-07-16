using Altinn.AccessManagement.Core.Models;
using Altinn.Authorization.Shared;
using Altinn.AccessManagement.Core.Models.Connection;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Core.Services.Interfaces;

/// <summary>
/// Core service interface for managing connections between parties
/// </summary>
public interface IConnectionService
{
    /// <summary>
    /// Gets connections between parties
    /// </summary>
    /// <param name="fromId">From party identifier</param>
    /// <param name="toId">To party identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing list of connections</returns>
    Task<Result<List<CompactConnection>>> Get(Guid? fromId = null, Guid? toId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an assignment between parties
    /// </summary>
    /// <param name="fromId">From party identifier</param>
    /// <param name="toId">To party identifier</param>
    /// <param name="roleCode">Role code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the created assignment</returns>
    Task<Result<Assignment>> AddAssignment(Guid fromId, Guid toId, string roleCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an assignment between parties
    /// </summary>
    /// <param name="fromId">From party identifier</param>
    /// <param name="toId">To party identifier</param>
    /// <param name="roleCode">Role code</param>
    /// <param name="cascade">Whether to cascade the removal</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation problem if any</returns>
    Task<ValidationProblemInstance> RemoveAssignment(Guid fromId, Guid toId, string roleCode, bool cascade = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets packages available for connection
    /// </summary>
    /// <param name="fromId">From party identifier</param>
    /// <param name="toId">To party identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing list of package permissions</returns>
    Task<Result<List<PackagePermission>>> GetPackages(Guid? fromId = null, Guid? toId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a package to a connection
    /// </summary>
    /// <param name="fromId">From party identifier</param>
    /// <param name="toId">To party identifier</param>
    /// <param name="roleCode">Role code</param>
    /// <param name="packageId">Package identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the assignment package</returns>
    Task<Result<AssignmentPackage>> AddPackage(Guid fromId, Guid toId, string roleCode, Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a package to a connection by package name
    /// </summary>
    /// <param name="fromId">From party identifier</param>
    /// <param name="toId">To party identifier</param>
    /// <param name="roleCode">Role code</param>
    /// <param name="packageName">Package name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the assignment package</returns>
    Task<Result<AssignmentPackage>> AddPackage(Guid fromId, Guid toId, string roleCode, string packageName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a package from a connection
    /// </summary>
    /// <param name="fromId">From party identifier</param>
    /// <param name="toId">To party identifier</param>
    /// <param name="roleCode">Role code</param>
    /// <param name="packageId">Package identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation problem if any</returns>
    Task<ValidationProblemInstance> RemovePackage(Guid fromId, Guid toId, string roleCode, Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a package from a connection by package name
    /// </summary>
    /// <param name="fromId">From party identifier</param>
    /// <param name="toId">To party identifier</param>
    /// <param name="roleCode">Role code</param>
    /// <param name="packageName">Package name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation problem if any</returns>
    Task<ValidationProblemInstance> RemovePackage(Guid fromId, Guid toId, string roleCode, string packageName, CancellationToken cancellationToken = default);
}