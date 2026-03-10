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
public class RequestService(AppDbContext db, IAssignmentService assignmentService) : IRequestService
{
    /// <inheritdoc/>
    public async Task<RequestDto> GetRequest(Guid requestId, CancellationToken ct = default)
    {
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

        return null;
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<RequestDto>> GetRequests(Guid? fromId, Guid? toId, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct = default)
    {
        if (!fromId.HasValue && !toId.HasValue)
        {
            throw new ArgumentException("At least one of fromId, toId or requestedBy must be provided");
        }

        bool usePackages = true; // Future feature

        var requestResources = await GetRequestAssignmentResource(fromId, toId, status, after, ct);
        var requestPackages = usePackages ? await GetRequestAssignmentPackage(fromId, toId, status, after, ct) : null;

        return usePackages
            ? requestResources.Select(DtoMapper.Convert).Union(requestPackages.Select(DtoMapper.Convert))
            : requestResources.Select(DtoMapper.Convert);
    }

    /// <inheritdoc/>
    public async Task<Result<RequestDto>> CreateRequest(CreateRequestDto request, CancellationToken ct = default)
    {
        if (request.Resource.HasValue && request.Package.HasValue)
        {
            throw new ArgumentException();
        }

        if (request.Resource.HasValue)
        {
            return await CreateRequestAssignmentResource(request.From, request.To, request.Role, request.Resource.Value, request.Status, ct);
        }

        if (request.Package.HasValue)
        {
            return await CreateRequestAssignmentPackage(request.From, request.To, request.Role, request.Package.Value, request.Status, ct);
        }

        throw new ArgumentException();
    }

    /// <inheritdoc/>
    public async Task<Result<RequestDto>> UpdateRequest(Guid requestId, RequestStatus status, CancellationToken ct = default)
    {
        var resourceRequest = await db.RequestAssignmentResources.FirstOrDefaultAsync(t => t.Id == requestId);
        if (resourceRequest is { })
        {
            resourceRequest.Status = status;
            await db.SaveChangesAsync();
            return await GetRequest(requestId);
        }

        var packageRequest = await db.RequestAssignmentPackages.FirstOrDefaultAsync(t => t.Id == requestId);
        if (packageRequest is { })
        {
            packageRequest.Status = status;
            await db.SaveChangesAsync();
            return await GetRequest(requestId);
        }

        /*
        //// TODO : Validate Status states
        
        Draft->Pending
        Draft/Pending->Withdrawn
        Pending->Accepted/Rejected
        */

        return await GetRequest(requestId);
    }

    #region privates
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

    private async Task<Result<RequestDto>> CreateRequestAssignmentResource(Guid fromId, Guid toId, Guid roleId, Guid resourceId, RequestStatus initialStatus = RequestStatus.Pending, CancellationToken ct = default)
    {
        var assignment = await assignmentService.GetOrCreateAssignment(fromId, toId, roleId, cancellationToken: ct);

        var request = await db.RequestAssignmentResources.FirstOrDefaultAsync(r => r.AssignmentId == assignment.Id && r.ResourceId == resourceId && r.Status == initialStatus);
        if (request == null)
        {
            request = new RequestAssignmentResource
            {
                Id = Guid.NewGuid(),
                Status = initialStatus,
                AssignmentId = assignment.Id,
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

    private async Task<Result<RequestDto>> CreateRequestAssignmentPackage(Guid fromId, Guid toId, Guid roleId, Guid packageId, RequestStatus initialStatus = RequestStatus.Pending, CancellationToken ct = default)
    {
        var assignment = await assignmentService.GetOrCreateAssignment(fromId, toId, roleId, cancellationToken: ct);

        var request = await db.RequestAssignmentPackages.FirstOrDefaultAsync(r => r.AssignmentId == assignment.Id && r.PackageId == packageId && r.Status == initialStatus, cancellationToken: ct);
        if (request is null)
        {
            request = new RequestAssignmentPackage
            {
                Id = Guid.NewGuid(),
                Status = initialStatus,
                AssignmentId = assignment.Id,
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
