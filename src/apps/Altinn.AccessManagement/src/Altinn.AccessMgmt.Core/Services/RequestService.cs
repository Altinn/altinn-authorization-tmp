using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement.Request;
using Altinn.Authorization.ProblemDetails;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc/>
public class RequestService(AppDbContext db) : IRequestService
{
    /// <inheritdoc/>
    public async Task<Result<RequestDto>> GetRequest(Guid requestId, CancellationToken ct = default)
    {
        ValidationErrorBuilder error = default;

        var requestResource = await db.RequestAssignmentResources.AsNoTracking()
            .Include(r => r.Assignment).ThenInclude(a => a.From)
            .Include(r => r.Assignment).ThenInclude(a => a.To)
            .Include(r => r.Assignment).ThenInclude(a => a.Role)
            .Include(r => r.Resource)
            .FirstOrDefaultAsync(t => t.Id == requestId, ct);

        if (requestResource != null)
        {
            return DtoMapper.Convert(requestResource);
        }

        var requestPackage = await db.RequestAssignmentPackages.AsNoTracking()
            .Include(r => r.Assignment).ThenInclude(a => a.From)
            .Include(r => r.Assignment).ThenInclude(a => a.To)
            .Include(r => r.Assignment).ThenInclude(a => a.Role)
            .Include(r => r.Package)
            .FirstOrDefaultAsync(t => t.Id == requestId, ct);

        if (requestPackage != null)
        {
            return DtoMapper.Convert(requestPackage);
        }

        error.Add(ValidationErrors.RequestNotFound);
        error.TryBuild(out var problems);

        return problems;
    }
    
    /// <inheritdoc/>
    public async Task<Result<IEnumerable<RequestDto>>> GetRequests(Guid? fromId, Guid? toId, IEnumerable<RequestStatus> status, string? type, CancellationToken ct = default)
    {
        ValidationErrorBuilder error = default;

        if (!fromId.HasValue && !toId.HasValue)
        {
            error.Add(ValidationErrors.RequestMissingFromOrTo);
        }

        var requestResources = string.IsNullOrEmpty(type) || type == "resource" ? await GetRequestAssignmentResource(fromId, toId, status, ct) : default;
        var requestPackages = string.IsNullOrEmpty(type) || type == "package" ? await GetRequestAssignmentPackage(fromId, toId, status, ct) : default;

        if (!requestResources.Any() && !requestPackages.Any())
        {
            error.Add(ValidationErrors.RequestNotFound);
        }

        if (error.TryBuild(out var problems))
        {
            return problems;
        }

        var result = requestResources.Select(DtoMapper.Convert).Union(requestPackages.Select(DtoMapper.Convert));
        return result.ToList();
    }

    /// <inheritdoc/>
    public async Task<Result<RequestDto>> CreateRequest(CreateRequestDto request, CancellationToken ct = default)
    {
        ValidationErrorBuilder error = default;

        if (!request.Resource.HasValue && !request.Package.HasValue)
        {
            error.Add(ValidationErrors.RequestMissingResourceOrPackage);
            error.TryBuild(out var inputProblems);
            return inputProblems;
        }

        var requestAssignmentResult = await GetOrCreateRequestAssignment(request.From, request.To, request.Role, ct);
        var requestAssignment = requestAssignmentResult.Value;

        if (request.Resource.HasValue)
        {
            return await CreateRequestAssignmentResource(requestAssignment.Id, request.Resource.Value, request.Status, ct);
        }

        if (request.Package.HasValue)
        {
            return await CreateRequestAssignmentPackage(requestAssignment.Id, request.Package.Value, request.Status, ct);
        }

        error.Add(ValidationErrors.RequestFailedToCreateRequest);
        error.TryBuild(out var problems);
        return problems;
    }

    /// <inheritdoc/>
    public async Task<Result<RequestDto>> UpdateRequest(Guid partyUuid, Guid requestId, RequestStatus status, CancellationToken ct = default)
    {
        ValidationErrorBuilder errorBuilder = default;

        var requestResult = await GetRequest(requestId, ct);
        if (requestResult.IsProblem)
        {
            return requestResult.Problem;
        }

        var verify = VerifyRequestStatusUpdate(request: requestResult.Value, partyUuid, status);
        if (verify.IsProblem)
        {
            return verify.Problem;
        }

        switch (requestResult.Value.Type)
        {
            case "resource":
                return await UpdateResourceRequestStatus(requestId, status);
            case "package":
                return await UpdatePackageRequestStatus(requestId, status);
        }

        errorBuilder.Add(ValidationErrors.RequestNotFound);
        errorBuilder.TryBuild(out var problems);
        return problems;
    }

    #region privates

    private Result VerifyRequestStatusUpdate(RequestDto request, Guid partyUuid, RequestStatus status)
    {
        ValidationErrorBuilder errorBuilder = default;

        if (request.Status == RequestStatus.Draft)
        {
            if (status == RequestStatus.Pending || status == RequestStatus.Withdrawn)
            {
                // PartyUuid must match Request.To
                if (request.To.Id != partyUuid)
                {
                    errorBuilder.Add(ValidationErrors.RequestUnauthorizedStatusUpdate, $"$QUERY/party", [new("party", $"Unable to update {request.Id} from {request.Status.ToString()} to {status.ToString()}")]);
                }
            }

            errorBuilder.Add(ValidationErrors.RequestUnsupportedStatusUpdate, $"$QUERY/party", [new("party", $"Changing request status from {request.Status.ToString()} to {status.ToString()} is not allowed.")]);
        }

        if (request.Status == RequestStatus.Pending)
        {
            if (status == RequestStatus.Withdrawn)
            {
                // PartyUuid must match Request.To
                if (request.To.Id != partyUuid)
                {
                    errorBuilder.Add(ValidationErrors.RequestUnauthorizedStatusUpdate, $"$QUERY/party", [new("party", $"Unable to update {request.Id} from {request.Status.ToString()} to {status.ToString()}")]);
                }
            }

            if (status == RequestStatus.Approved || status == RequestStatus.Rejected)
            {
                // PartyUuid must match Request.From
                if (request.From.Id != partyUuid)
                {
                    errorBuilder.Add(ValidationErrors.RequestUnauthorizedStatusUpdate, $"$QUERY/party", [new("party", $"Unable to update {request.Id} from {request.Status.ToString()} to {status.ToString()}")]);
                }
            }

            errorBuilder.Add(ValidationErrors.RequestUnsupportedStatusUpdate, $"$QUERY/party", [new("party", $"Changing request status from {request.Status.ToString()} to {status.ToString()} is not allowed.")]);
        }

        if (request.Status == RequestStatus.Approved || request.Status == RequestStatus.Rejected || request.Status == RequestStatus.Withdrawn)
        {
            errorBuilder.Add(ValidationErrors.RequestUnsupportedStatusUpdate, $"$QUERY/party", [new("party", $"Changing request status from {request.Status.ToString()} to {status.ToString()} is not allowed.")]);
        }

        errorBuilder.TryBuild(out var problems);

        return problems;
    }

    private async Task<Result<RequestDto>> UpdatePackageRequestStatus(Guid id, RequestStatus status)
    {
        ValidationErrorBuilder errorBuilder = default;

        var request = await db.RequestAssignmentPackages.FirstOrDefaultAsync(t => t.Id == id);
        if (request is not { })
        {
            errorBuilder.Add(ValidationErrors.DbNoRowsFound, nameof(db.RequestAssignmentPackages));
        }

        request.Status = status;
        var res = await db.SaveChangesAsync();

        if (res != 1)
        {
            errorBuilder.Add(ValidationErrors.DbNoRowsAffected, nameof(db.RequestAssignmentPackages));
        }

        errorBuilder.TryBuild(out var problems);
        if (problems != null)
        {
            return problems;
        }

        return await GetRequest(id);
    }

    private async Task<Result<RequestDto>> UpdateResourceRequestStatus(Guid id, RequestStatus status)
    {
        ValidationErrorBuilder errorBuilder = default;

        var request = await db.RequestAssignmentResources.FirstOrDefaultAsync(t => t.Id == id);
        if (request is not { })
        {
            errorBuilder.Add(ValidationErrors.DbNoRowsFound, nameof(db.RequestAssignmentResources));
        }

        request.Status = status;
        var res = await db.SaveChangesAsync();

        if (res != 1)
        {
            errorBuilder.Add(ValidationErrors.DbNoRowsAffected, nameof(db.RequestAssignmentResources));
        }

        errorBuilder.TryBuild(out var problems);
        if (problems != null)
        {
            return problems;
        }

        return await GetRequest(id);
    }

    private async Task<IEnumerable<RequestAssignmentResource>> GetRequestAssignmentResource(Guid? fromId, Guid? toId, IEnumerable<RequestStatus> status, CancellationToken ct)
    {
        if (!fromId.HasValue && !toId.HasValue)
        {
            throw new ArgumentException("At least one of fromId, toId or requestedBy must be provided");
        }

        return await db.RequestAssignmentResources
            .Include(r => r.Assignment).ThenInclude(a => a.From)
            .Include(r => r.Assignment).ThenInclude(a => a.To)
            .Include(r => r.Assignment).ThenInclude(a => a.Role)
            .Include(r => r.Resource)
            .WhereIf(fromId.HasValue, r => r.Assignment.FromId == fromId.Value)
            .WhereIf(toId.HasValue, r => r.Assignment.ToId == toId.Value)
            .WhereIf(status?.Any() == true, r => status.Contains(r.Status))
            .ToListAsync(cancellationToken: ct);
    }
    
    private async Task<IEnumerable<RequestAssignmentPackage>> GetRequestAssignmentPackage(Guid? fromId, Guid? toId, IEnumerable<RequestStatus> status, CancellationToken ct)
    {
        if (!fromId.HasValue && !toId.HasValue)
        {
            throw new ArgumentException("At least one of fromId, toId or requestedBy must be provided");
        }

        return await db.RequestAssignmentPackages
            .Include(r => r.Assignment).ThenInclude(a => a.From)
            .Include(r => r.Assignment).ThenInclude(a => a.To)
            .Include(r => r.Assignment).ThenInclude(a => a.Role)
            .Include(r => r.Package)
            .WhereIf(fromId.HasValue, r => r.Assignment.FromId == fromId.Value)
            .WhereIf(toId.HasValue, r => r.Assignment.ToId == toId.Value)
            .WhereIf(status?.Any() == true, r => status.Contains(r.Status))
            .ToListAsync(cancellationToken: ct);
    }

    private async Task<Result<RequestAssignment>> GetOrCreateRequestAssignment(Guid fromId, Guid toId, Guid roleId, CancellationToken ct = default)
    {
        var request = await db.RequestAssignments.FirstOrDefaultAsync(r => r.FromId == fromId && r.ToId == toId && r.RoleId == roleId, ct);
        if (request == null)
        {
            request = new RequestAssignment
            {
                FromId = fromId,
                ToId = toId,
                RoleId = roleId,
            };
            db.RequestAssignments.Add(request);

            var res = await db.SaveChangesAsync(ct);

            if (res == 0)
            {
                return Problems.RequestCreationFailed;
            }
        }

        return request;
    }

    private async Task<Result<RequestDto>> CreateRequestAssignmentResource(Guid assignmentId, Guid resourceId, RequestStatus initialStatus = RequestStatus.Pending, CancellationToken ct = default)
    {
        var request = await db.RequestAssignmentResources.FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.ResourceId == resourceId && r.Status == initialStatus, ct);
        if (request == null)
        {
            request = new RequestAssignmentResource
            {
                Status = initialStatus,
                AssignmentId = assignmentId,
                ResourceId = resourceId
            };
            db.RequestAssignmentResources.Add(request);

            var res = await db.SaveChangesAsync(ct);

            if (res == 0)
            {
                return Problems.RequestCreationFailed;
            }
        }

        return await GetRequest(request.Id, ct);
    }

    private async Task<Result<RequestDto>> CreateRequestAssignmentPackage(Guid assignmentId, Guid packageId, RequestStatus initialStatus = RequestStatus.Pending, CancellationToken ct = default)
    {
        var request = await db.RequestAssignmentPackages.FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.PackageId == packageId && r.Status == initialStatus, cancellationToken: ct);
        if (request is null)
        {
            request = new RequestAssignmentPackage
            {
                Status = initialStatus,
                AssignmentId = assignmentId,
                PackageId = packageId,
            };
            db.RequestAssignmentPackages.Add(request);

            var res = await db.SaveChangesAsync(ct);
            if (res == 0)
            {
                return Problems.RequestCreationFailed;
            }
        }

        return await GetRequest(request.Id, ct);
    }
    #endregion
}
