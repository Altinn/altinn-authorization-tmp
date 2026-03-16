using System.Diagnostics;
using System.Transactions;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.Core.Outbox;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.Api.Contracts.AccessManagement.Request;
using Altinn.Authorization.ProblemDetails;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.SwaggerGen;

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
    public async Task<Result<IEnumerable<RequestDto>>> GetRequests(Guid? fromId, Guid? toId, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct = default)
    {
        ValidationErrorBuilder error = default;

        if (!fromId.HasValue && !toId.HasValue)
        {
            error.Add(ValidationErrors.RequestMissingFromOrTo);
        }

        var requestResources = await GetRequestAssignmentResource(fromId, toId, status, after, ct);
        var requestPackages = await GetRequestAssignmentPackage(fromId, toId, status, after, ct);

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

        if (request.Resource is not { } && request.Package is not { })
        {
            error.Add(ValidationErrors.RequestMissingResourceOrPackage);
            error.TryBuild(out var inputProblems);
            return inputProblems;
        }

        var requestAssignmentResult = await GetOrCreateRequestAssignment(request.From, request.To, request.Role, ct);
        var requestAssignment = requestAssignmentResult.Value;

        if (request.Resource is { })
        {
            return await CreateRequestAssignmentResource(requestAssignment, request.Resource, request.Status, ct);
        }

        if (request.Package is { })
        {
            return await CreateRequestAssignmentPackage(requestAssignment, request.Package, request.Status, ct);
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
            return requestResult;
        }

        var request = requestResult.Value;

        if (request.Connection.From.Id != partyUuid)
        {
            errorBuilder.Add(ValidationErrors.RequestNotFound, "$QUERY/requestId", [new("RequestId", $"Request {requestId} does not exists")]);
        }

        var statusRules = new Dictionary<RequestStatus, List<RequestStatus>>
        {
            { RequestStatus.Draft, [RequestStatus.Pending, RequestStatus.Withdrawn] },
            { RequestStatus.Pending, [RequestStatus.Approved, RequestStatus.Rejected, RequestStatus.Withdrawn] },
            { RequestStatus.Approved, [] },
            { RequestStatus.Rejected, [] },
            { RequestStatus.Withdrawn, [] }
        };

        foreach (var rule in statusRules)
        {
            if (request.Status == rule.Key && !rule.Value.Contains(status))
            {
                errorBuilder.Add(ValidationErrors.RequestUnsupportedStatusUpdate, "$QUERY/status", [new("Status", $"Request cannot change from {request.Status} to {status}.")]);
            }
        }

        if (errorBuilder.TryBuild(out var problem))
        {
            return problem;
        }

        switch (request.Type)
        {
            case "resource":
                await UpdateResourceRequestStatus(requestId, status, ct);
                break;
            case "package":
                await UpdatePackageRequestStatus(requestId, status, ct);
                break;
        }

        return await GetRequest(requestId, ct);
    }

    #region privates

    private async Task<bool> UpdatePackageRequestStatus(Guid id, RequestStatus status, CancellationToken ct = default)
    {
        var request = await db.RequestAssignmentPackages.FirstOrDefaultAsync(t => t.Id == id);
        if (request is { })
        {
            request.Status = status;

            if (status == RequestStatus.Approved)
            {
                await db.UpsertOutboxAsync(
                    nameof(UpdateResourceRequestStatus),
                    nameof(PackageRequestAcceptedNotificationHandler),
                    _ => new PackageRequestAcceptedNotificationMessage
                    {
                        RefId = DateTime.UtcNow,
                        Package = request.Package.Name,
                        RecipientId = request.Assignment.FromId,
                        AcceptorId = request.Assignment.ToId
                    },
                    null,
                    ct
                );
            }

            await db.SaveChangesAsync();
            return true;
        }

        return false;
    }

    private async Task<bool> UpdateResourceRequestStatus(Guid id, RequestStatus status, CancellationToken ct = default)
    {
        var request = await db.RequestAssignmentResources
            .Include(t => t.Assignment)
            .Include(t => t.Resource)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (request is { })
        {
            request.Status = status;

            if (status == RequestStatus.Approved)
            {
                await db.UpsertOutboxAsync(
                    nameof(UpdateResourceRequestStatus),
                    nameof(ResourceRequestAcceptedNotificationHandler),
                    _ => new ResourceRequestAcceptedNotificationMessage
                    {
                        RefId = DateTime.UtcNow,
                        Resource = request.Resource.RefId,
                        RecipientId = request.Assignment.FromId,
                        AcceptorId = request.Assignment.ToId
                    },
                    null,
                    ct
                );
            }

            await db.SaveChangesAsync();
            return true;
        }

        return false;
    }

    private async Task<IEnumerable<RequestAssignmentResource>> GetRequestAssignmentResource(Guid? fromId, Guid? toId, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct)
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
            .WhereIf(after.HasValue, r => r.Audit_ValidFrom >= after.Value)
            .ToListAsync(cancellationToken: ct);
    }

    private async Task<IEnumerable<RequestAssignmentPackage>> GetRequestAssignmentPackage(Guid? fromId, Guid? toId, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct)
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
            .WhereIf(after.HasValue, r => r.Audit_ValidFrom >= after.Value)
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

    private async Task<Result<RequestDto>> CreateRequestAssignmentResource(RequestAssignment assignment, ResourceDto resource, RequestStatus initialStatus = RequestStatus.Pending, CancellationToken ct = default)
    {
        var request = await db.RequestAssignmentResources.FirstOrDefaultAsync(r => r.AssignmentId == assignment.Id && r.ResourceId == resource.Id && r.Status == initialStatus, ct);
        if (request == null)
        {
            request = new RequestAssignmentResource
            {
                Status = initialStatus,
                AssignmentId = assignment.Id,
                ResourceId = resource.Id
            };

            db.RequestAssignmentResources.Add(request);
            await db.UpsertOutboxAsync<List<ResourceRequestPendingNotificationMessage>>(
                refId: $"auth_resource_request_accept_{request.Assignment.FromId}_{request.Assignment.ToId}",
                handler: "resource_request_pending",
                addValueFactory: msg =>
                {
                    var schedule = DateTime.UtcNow.AddMinutes(15);
                    msg.Schedule = schedule;
                    msg.Timeout = TimeSpan.FromMinutes(1);

                    return [
                        new ResourceRequestPendingNotificationMessage()
                        {
                            RecipientId = request.Assignment.FromId,
                            RequesterId = request.Assignment.ToId,
                            Resource = resource.Id.ToString(),
                            ExpectedDeliveredAt = schedule,
                        }
                    ];
                },
                updateValueFactory: (msg, curr) =>
                {
                    if (curr is null)
                    {
                        Activity.Current?.AddTag("RequestService.CreateRequestAssignmentResource.UpdateOutbox", "Current outbox message data is null? Creating new list");
                        curr = [];
                    }

                    var latestExisting = curr.Count > 0
                        ? curr.Max(b => b.ExpectedDeliveredAt)
                        : DateTime.UtcNow.AddMinutes(15);

                    var candidate = DateTime.UtcNow.AddMinutes(15.0 / (curr.Count + 1));
                    var schedule = candidate > latestExisting ? latestExisting : candidate;

                    curr.Add(new ResourceRequestPendingNotificationMessage()
                    {
                        RecipientId = request.Assignment.FromId,
                        RequesterId = request.Assignment.ToId,
                        Resource = resource.Id.ToString(),
                        ExpectedDeliveredAt = schedule,
                    });

                    return curr;
                },
                cancellationToken: ct
            );

            await db.UpsertOutboxAsync(
                nameof(CreateRequestAssignmentResource),
                nameof(ResourceRequestPendingNotificationHandler),
                _ => new ResourceRequestPendingNotificationMessage
                {
                    ExpectedDeliveredAt = DateTime.UtcNow,
                    Resource = resource.RefId,
                    RecipientId = assignment.FromId,
                    RequesterId = assignment.ToId,
                },
                null,
                ct
            );

            var res = await db.SaveChangesAsync(ct);

            if (res == 0)
            {
                return Problems.RequestCreationFailed;
            }
        }

        return await GetRequest(request.Id, ct);
    }

    private async Task<Result<RequestDto>> CreateRequestAssignmentPackage(RequestAssignment assignment, PackageDto package, RequestStatus initialStatus = RequestStatus.Pending, CancellationToken ct = default)
    {
        var request = await db.RequestAssignmentPackages.FirstOrDefaultAsync(r => r.AssignmentId == assignment.Id && r.PackageId == package.Id && r.Status == initialStatus, cancellationToken: ct);
        if (request is null)
        {
            request = new RequestAssignmentPackage
            {
                Status = initialStatus,
                AssignmentId = assignment.Id,
                PackageId = package.Id,
            };
            db.RequestAssignmentPackages.Add(request);

            await db.UpsertOutboxAsync(
                nameof(CreateRequestAssignmentPackage),
                nameof(PackageRequestPendingNotificationHandler),
                _ => new PackageRequestPendingNotificationMessage
                {
                    RefId = DateTime.UtcNow,
                    Package = package.Name,
                    RecipientId = assignment.FromId,
                    RequesterId = assignment.ToId
                },
                null,
                ct
            );

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
