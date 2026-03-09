using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessMgmt.Core.Services.Contracts;

/// <summary>
/// Defines a contract for managing request assignments, including creation, retrieval, and updating of assignment packages and resources.
/// </summary>
public interface IRequestService
{
    /// <summary>
    /// Retrieves the request associated with the specified request identifier.
    /// </summary>
    Task<RequestDto> GetRequest(Guid requestId, CancellationToken ct);

    /// <summary>
    /// Retrieves a collection of request DTOs matching the specified filtering criteria.
    /// </summary>
    Task<IEnumerable<RequestDto>> GetRequests(Guid? fromId, Guid? toId, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct);

    /// <summary>
    /// Retrieves the request assignment associated with the specified request identifier.
    /// </summary>
    Task<RequestAssignment> GetRequestAssignment(Guid requestId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a collection of request assignments matching the specified filtering criteria.
    /// </summary>
    Task<IEnumerable<RequestAssignment>> GetRequestAssignment(Guid? fromId, Guid? toId, Guid? roleId, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct);

    /// <summary>
    /// Creates a new request assignment linking the specified entities with the given role.
    /// </summary>
    Task<Result<RequestAssignment>> CreateRequestAssignment(Guid fromId, Guid toId, Guid roleId, CancellationToken ct = default);

    /// <summary>
    /// Updates the status of a request assignment.
    /// </summary>
    Task<Result<RequestAssignment>> UpdateRequestAssignment(Guid requestId, RequestStatus status, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the assignment package associated with the specified request identifier.
    /// </summary>
    Task<RequestAssignmentPackage> GetRequestAssignmentPackage(Guid requestId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a collection of request assignment packages matching the specified filtering criteria.
    /// </summary>
    Task<IEnumerable<RequestAssignmentPackage>> GetRequestAssignmentPackage(Guid? fromId, Guid? toId, Guid? roleId, Guid? packageId, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct);

    /// <summary>
    /// Creates a new request assignment package for the given from/to/role/package combination.
    /// </summary>
    Task<Result<RequestAssignmentPackage>> CreateRequestAssignmentPackage(Guid fromId, Guid toId, Guid roleId, Guid packageId, RequestStatus initialStatus = RequestStatus.Draft, CancellationToken ct = default);

    /// <summary>
    /// Creates a new request assignment package attached to an existing assignment.
    /// </summary>
    Task<Result<RequestAssignmentPackage>> CreateRequestAssignmentPackage(Guid assignmentId, Guid packageId, RequestStatus initialStatus = RequestStatus.Draft, CancellationToken ct = default);

    /// <summary>
    /// Updates the status of a request assignment package.
    /// </summary>
    Task<Result<RequestAssignmentPackage>> UpdateRequestAssignmentPackage(Guid requestId, RequestStatus status, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the assignment resource associated with the specified request identifier.
    /// </summary>
    Task<RequestAssignmentResource> GetRequestAssignmentResource(Guid requestId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a collection of request assignment resources matching the specified filtering criteria.
    /// </summary>
    Task<IEnumerable<RequestAssignmentResource>> GetRequestAssignmentResource(Guid? fromId, Guid? toId, Guid? roleId, Guid? resourceId, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct);

    /// <summary>
    /// Creates a new request assignment resource for the given from/to/role/resource combination.
    /// </summary>
    Task<Result<RequestAssignmentResource>> CreateRequestAssignmentResource(Guid fromId, Guid toId, Guid roleId, Guid resourceId, RequestStatus initialStatus = RequestStatus.Draft, CancellationToken ct = default);

    /// <summary>
    /// Creates a new request assignment resource attached to an existing assignment.
    /// </summary>
    Task<Result<RequestAssignmentResource>> CreateRequestAssignmentResource(Guid assignmentId, Guid resourceId, RequestStatus initialStatus = RequestStatus.Draft, CancellationToken ct = default);

    /// <summary>
    /// Updates the status of a request assignment resource.
    /// </summary>
    Task<Result<RequestAssignmentResource>> UpdateRequestAssignmentResource(Guid requestId, RequestStatus status, CancellationToken ct = default);
}
