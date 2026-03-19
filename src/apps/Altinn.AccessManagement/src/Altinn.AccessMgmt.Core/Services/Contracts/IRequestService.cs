using Altinn.Authorization.Api.Contracts.AccessManagement.Request;
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
    Task<Result<RequestDto>> GetRequest(Guid requestId, CancellationToken ct = default);

    /// <summary>
    /// Get requests created for party
    /// </summary>
    Task<Result<IEnumerable<RequestDto>>> GetSentRequests(Guid partyId, Guid? toId, IEnumerable<RequestStatus> status, string? type, CancellationToken ct = default);
    
    /// <summary>
    /// Get requests created at party
    /// </summary>
    Task<Result<IEnumerable<RequestDto>>> GetReceivedRequests(Guid partyId, Guid? fromId, IEnumerable<RequestStatus> status, string? type, CancellationToken ct = default);

    /// <summary>
    /// Creates a new request
    /// </summary>
    Task<Result<RequestDto>> CreateRequest(CreateRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Creates a new request
    /// </summary>
    Task<Result<RequestDto>> CreateResourceRequest(Guid atId, Guid forId, Guid byId, Guid roleId, Guid resourceId, RequestStatus status = RequestStatus.Pending, CancellationToken ct = default);

    /// <summary>
    /// Creates a new request
    /// </summary>
    Task<Result<RequestDto>> CreatePackageRequest(Guid atId, Guid forId, Guid byId, Guid roleId, Guid packageId, RequestStatus status = RequestStatus.Pending, CancellationToken ct = default);

    /// <summary>
    /// Updates the status of a request.
    /// </summary>
    Task<Result<RequestDto>> UpdateRequest(Guid partyUuid, Guid requestId, RequestStatus status, CancellationToken ct = default);
}
