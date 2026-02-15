using Altinn.AccessMgmt.PersistenceEF.Models;

namespace Altinn.AccessMgmt.Core.Services.Contracts;

/// <summary>
/// Defines a contract for managing request assignments, including creation, retrieval, and updating of assignment packages and resources.
/// </summary>
public interface IRequestService
{
    /// <summary>
    /// Retrieves a collection of request data transfer objects (DTOs) that match the specified filtering criteria.
    /// </summary>
    /// <param name="fromId">The optional identifier of the first request to include in the results. Only requests with an ID greater than or
    /// equal to this value are returned. Specify null to include all requests from the beginning.</param>
    /// <param name="toId">The optional identifier of the last request to include in the results. Only requests with an ID less than or
    /// equal to this value are returned. Specify null to include all requests up to the latest.</param>
    /// <param name="requestedBy">The optional identifier of the user who created the requests. Only requests created by this user are included if
    /// specified; otherwise, requests from all users are included.</param>
    /// <param name="status">The optional status to filter requests by. Only requests with the specified status are returned. Specify null to
    /// include requests of all statuses.</param>
    /// <param name="after">The optional timestamp to filter requests created after the specified date and time. Only requests created after
    /// this value are included. Specify null to include requests regardless of creation time.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of request
    /// DTOs that match the specified filters.</returns>
    Task<IEnumerable<RequestDto>> GetRequests(Guid? fromId, Guid? toId, Guid? requestedBy, RequestStatus? status, DateTimeOffset? after, CancellationToken ct);

    /// <summary>
    /// Retrieves the request assignment associated with the specified request identifier.
    /// </summary>
    /// <remarks>This method may return null if no assignment is found for the given request
    /// identifier.</remarks>
    /// <param name="requestId">The unique identifier of the request for which the assignment is to be retrieved.</param>
    /// <returns>A task that represents the asynchronous operation, containing the request assignment associated with the
    /// specified request identifier.</returns>
    Task<RequestAssignment> GetRequestAssignment(Guid requestId);

    /// <summary>
    /// Retrieves a collection of request assignments that match the specified filtering criteria.
    /// </summary>
    /// <param name="fromId">The optional identifier that specifies the lower bound (exclusive) for the request assignment IDs to retrieve.
    /// Only assignments with an ID greater than this value are included.</param>
    /// <param name="toId">The optional identifier that specifies the upper bound (inclusive) for the request assignment IDs to retrieve.
    /// Only assignments with an ID less than or equal to this value are included.</param>
    /// <param name="roleId">The optional identifier of the role to filter request assignments by. Only assignments associated with this role
    /// are included.</param>
    /// <param name="requestedBy">The optional identifier of the user who requested the assignments. Only assignments requested by this user are
    /// included.</param>
    /// <param name="status">The optional status to filter request assignments by. Only assignments with this status are included.</param>
    /// <param name="after">The optional timestamp to filter assignments created after the specified date and time. Only assignments created
    /// after this point are included.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of request
    /// assignments that match the specified filters.</returns>
    Task<IEnumerable<RequestAssignment>> GetRequestAssignment(Guid? fromId, Guid? toId, Guid? roleId, Guid? requestedBy, RequestStatus? status, DateTimeOffset? after, CancellationToken ct);

    /// <summary>
    /// Creates a new request assignment that links the specified entities with the given role.
    /// </summary>
    /// <remarks>This method may throw an exception if any of the provided identifiers are invalid or if
    /// business logic constraints prevent the assignment from being created.</remarks>
    /// <param name="fromId">The unique identifier of the entity initiating the request assignment.</param>
    /// <param name="toId">The unique identifier of the entity to whom the request is assigned.</param>
    /// <param name="roleId">The unique identifier of the role associated with the request assignment.</param>
    /// <param name="requestedBy">The unique identifier of the user who is requesting the assignment.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created RequestAssignment
    /// object.</returns>
    Task<RequestAssignment> CreateRequestAssignment(Guid fromId, Guid toId, Guid roleId, Guid requestedBy);

    /// <summary>
    /// Updates the assignment status of a request identified by the specified request ID.
    /// </summary>
    /// <remarks>An exception is thrown if the specified request ID does not exist or if the provided status
    /// is invalid.</remarks>
    /// <param name="requestId">The unique identifier of the request to update.</param>
    /// <param name="status">The new status to assign to the request. Must be a valid value of the RequestStatus enumeration.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated RequestAssignment
    /// object.</returns>
    Task<RequestAssignment> UpdateRequestAssignment(Guid requestId, RequestStatus status);

    /// <summary>
    /// Retrieves the assignment package associated with the specified request identifier.
    /// </summary>
    /// <remarks>This method is asynchronous and should be awaited. Throws an exception if the specified
    /// <paramref name="requestId"/> does not correspond to an existing request.</remarks>
    /// <param name="requestId">The unique identifier of the request for which to retrieve the assignment package. Must be a valid <see
    /// cref="System.Guid"/>.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see
    /// cref="RequestAssignmentPackage"/> associated with the specified request identifier.</returns>
    Task<RequestAssignmentPackage> GetRequestAssignmentPackage(Guid requestId);

    /// <summary>
    /// Retrieves a collection of request assignment packages that match the specified filtering criteria.
    /// </summary>
    /// <param name="fromId">The optional identifier that specifies the lower bound of the package ID range to include in the results. Only
    /// packages with an ID greater than or equal to this value are returned. If null, no lower bound is applied.</param>
    /// <param name="toId">The optional identifier that specifies the upper bound of the package ID range to include in the results. Only
    /// packages with an ID less than or equal to this value are returned. If null, no upper bound is applied.</param>
    /// <param name="roleId">The optional identifier of the role associated with the request assignment. Only packages linked to this role
    /// are included. If null, packages for all roles are considered.</param>
    /// <param name="packageId">The optional identifier of a specific package to filter the results. If specified, only the package with this ID
    /// is returned. If null, packages are not filtered by package ID.</param>
    /// <param name="requestedBy">The optional identifier of the user who requested the assignment package. Only packages requested by this user
    /// are included. If null, packages requested by any user are considered.</param>
    /// <param name="status">The optional status used to filter the request assignment packages by their current state. If null, packages of
    /// all statuses are included.</param>
    /// <param name="after">The optional date and time used to filter packages that were created after this timestamp. If null, no creation
    /// time filter is applied.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of request assignment
    /// packages that match the specified filters. The collection is empty if no packages meet the criteria.</returns>
    Task<IEnumerable<RequestAssignmentPackage>> GetRequestAssignmentPackage(Guid? fromId, Guid? toId, Guid? roleId, Guid? packageId, Guid? requestedBy, RequestStatus? status, DateTimeOffset? after, CancellationToken ct);

    /// <summary>
    /// Creates a new request assignment package with the specified source, target, role, and package identifiers.
    /// </summary>
    /// <remarks>This method is asynchronous and should be awaited. All parameters must be valid
    /// GUIDs.</remarks>
    /// <param name="fromId">The unique identifier of the entity initiating the request assignment.</param>
    /// <param name="toId">The unique identifier of the entity to which the request assignment is directed.</param>
    /// <param name="roleId">The unique identifier of the role associated with the request assignment.</param>
    /// <param name="packageId">The unique identifier of the package to be assigned.</param>
    /// <param name="requestedBy">The unique identifier of the user who is requesting the assignment.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created
    /// RequestAssignmentPackage.</returns>
    Task<RequestAssignmentPackage> CreateRequestAssignmentPackage(Guid fromId, Guid toId, Guid roleId, Guid packageId, Guid requestedBy);

    /// <summary>
    /// Creates a new request assignment package that associates the specified assignment with the given package on
    /// behalf of a requesting user.
    /// </summary>
    /// <remarks>This method is asynchronous and should be awaited. All provided identifiers must reference
    /// existing entities; otherwise, the operation may fail.</remarks>
    /// <param name="assignmentId">The unique identifier of the assignment to be included in the package.</param>
    /// <param name="packageId">The unique identifier of the package to associate with the assignment.</param>
    /// <param name="requestedBy">The unique identifier of the user requesting the creation of the assignment package.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created RequestAssignmentPackage
    /// instance.</returns>
    Task<RequestAssignmentPackage> CreateRequestAssignmentPackage(Guid assignmentId, Guid packageId, Guid requestedBy);

    /// <summary>
    /// Updates the assignment package for the specified request with a new status.
    /// </summary>
    /// <remarks>An exception is thrown if the specified request identifier does not exist or if the provided
    /// status is invalid.</remarks>
    /// <param name="requestId">The unique identifier of the request whose assignment package is to be updated.</param>
    /// <param name="status">The new status to assign to the request. The status determines how the request will be processed.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated request assignment
    /// package.</returns>
    Task<RequestAssignmentPackage> UpdateRequestAssignmentPackage(Guid requestId, RequestStatus status);

    /// <summary>
    /// Retrieves the assignment resource associated with the specified request identifier.
    /// </summary>
    /// <remarks>This method is asynchronous and should be awaited. Throws an exception if the request
    /// identifier is invalid or if the resource cannot be found.</remarks>
    /// <param name="requestId">The unique identifier of the request for which to retrieve the assignment resource. Must be a valid GUID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the request assignment resource
    /// associated with the specified request identifier.</returns>
    Task<RequestAssignmentResource> GetRequestAssignmentResource(Guid requestId);

    /// <summary>
    /// Retrieves a collection of request assignment resources that match the specified filtering criteria.
    /// </summary>
    /// <param name="fromId">The optional lower bound identifier. Only resources with an ID greater than or equal to this value are included.
    /// If null, no lower bound is applied.</param>
    /// <param name="toId">The optional upper bound identifier. Only resources with an ID less than or equal to this value are included. If
    /// null, no upper bound is applied.</param>
    /// <param name="roleId">The optional identifier of the role associated with the request assignment. If null, resources for all roles are
    /// included.</param>
    /// <param name="resourceId">The optional identifier of the specific resource to filter by. If null, resources for all resource IDs are
    /// included.</param>
    /// <param name="action">The action to filter request assignments by. This parameter is required.</param>
    /// <param name="requestedBy">The optional identifier of the user who requested the assignment. If null, assignments requested by any user are
    /// included.</param>
    /// <param name="status">The optional status to filter request assignments by. If null, assignments with any status are included.</param>
    /// <param name="after">The optional timestamp to filter assignments created after the specified date and time. If null, no lower time
    /// bound is applied.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of request assignment
    /// resources that match the specified criteria.</returns>
    Task<IEnumerable<RequestAssignmentResource>> GetRequestAssignmentResource(Guid? fromId, Guid? toId, Guid? roleId, Guid? resourceId, string action, Guid? requestedBy, RequestStatus? status, DateTimeOffset? after, CancellationToken ct);

    /// <summary>
    /// Creates a new request assignment resource that represents an assignment action between two resources with a
    /// specified role and action.
    /// </summary>
    /// <remarks>An exception is thrown if any of the provided identifiers are invalid or if the assignment
    /// action cannot be performed.</remarks>
    /// <param name="fromId">The unique identifier of the resource from which the assignment originates.</param>
    /// <param name="toId">The unique identifier of the resource to which the assignment is made.</param>
    /// <param name="roleId">The unique identifier of the role associated with the assignment.</param>
    /// <param name="resourceId">The unique identifier of the resource being assigned.</param>
    /// <param name="action">The action to be performed as part of the assignment. Cannot be null or empty.</param>
    /// <param name="requestedBy">The unique identifier of the user who is requesting the assignment.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created
    /// RequestAssignmentResource.</returns>
    Task<RequestAssignmentResource> CreateRequestAssignmentResource(Guid fromId, Guid toId, Guid roleId, Guid resourceId, string action, Guid requestedBy);

    /// <summary>
    /// Creates a new request assignment resource with the specified assignment, resource, action, and requesting user.
    /// </summary>
    /// <remarks>Throws an exception if any of the provided identifiers are invalid or if the specified action
    /// cannot be performed.</remarks>
    /// <param name="assignmentId">The unique identifier of the assignment to which the resource will be associated.</param>
    /// <param name="resourceId">The unique identifier of the resource to assign to the request.</param>
    /// <param name="action">The action to perform on the resource. This value determines how the resource will be processed as part of the
    /// assignment.</param>
    /// <param name="requestedBy">The unique identifier of the user requesting the assignment of the resource.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created
    /// RequestAssignmentResource.</returns>
    Task<RequestAssignmentResource> CreateRequestAssignmentResource(Guid assignmentId, Guid resourceId, string action, Guid requestedBy);

    /// <summary>
    /// Updates the assignment status of a request identified by the specified request ID.
    /// </summary>
    /// <remarks>This method may throw an exception if the specified request ID does not exist or if the
    /// provided status is invalid.</remarks>
    /// <param name="requestId">The unique identifier of the request to update.</param>
    /// <param name="status">The new status to assign to the request. Must be a valid value of the RequestStatus enumeration.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated
    /// RequestAssignmentResource.</returns>
    Task<RequestAssignmentResource> UpdateRequestAssignmentResource(Guid requestId, RequestStatus status);
}
