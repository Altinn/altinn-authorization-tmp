using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services;

public class RequestService(AppDbContext db, IAssignmentService assignmentService) : IRequestService
{
    public async Task<IEnumerable<RequestDto>> GetRequests(Guid? fromId, Guid? toId, Guid? requestedBy, RequestStatus? status, DateTimeOffset? after, CancellationToken ct)
    {
        if (!fromId.HasValue && !toId.HasValue && !requestedBy.HasValue)
        {
            throw new ArgumentException("At least one of fromId, toId or requestedBy must be provided");
        }

        var r1 = await GetRequestAssignment(fromId, toId, null, requestedBy, status, after, ct);
        var r3 = await GetRequestAssignmentPackage(fromId, toId, null, null, requestedBy, status, after, ct);
        var r2 = await GetRequestAssignmentResource(fromId, toId, null, null, null, requestedBy, status, after, ct);

        return r1.Select(DtoMapper.Convert)
            .Union(r2.Select(DtoMapper.Convert))
            .Union(r3.Select(DtoMapper.Convert));
    }

    public async Task<RequestAssignment> GetRequestAssignment(Guid requestId)
    {
        var request = await db.RequestAssignments
            .Include(a => a.From)
            .Include(a => a.To)
            .Include(a => a.Role)
            .Include(r => r.RequestedBy)
            .FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null)
        {
            throw new Exception("Request not found");
        }

        return request;
    }

    public async Task<IEnumerable<RequestAssignment>> GetRequestAssignment(Guid? fromId, Guid? toId, Guid? roleId, Guid? requestedBy, RequestStatus? status, DateTimeOffset? after, CancellationToken ct)
    {
        if (!fromId.HasValue && !toId.HasValue && !requestedBy.HasValue)
        {
            throw new ArgumentException("At least one of fromId, toId or requestedBy must be provided");
        }

        return await db.RequestAssignments
            .Include(a => a.From)
            .Include(a => a.To)
            .Include(a => a.Role)
            .Include(r => r.RequestedBy)
            .WhereIf(fromId.HasValue, r => r.FromId == fromId.Value)
            .WhereIf(toId.HasValue, r => r.ToId == toId.Value)
            .WhereIf(roleId.HasValue, r => r.RoleId == roleId.Value)
            .WhereIf(requestedBy.HasValue, r => r.RequestedById == requestedBy.Value)
            .WhereIf(status.HasValue, r => r.Status == status.Value)
            .WhereIf(after.HasValue, r => r.Audit_ValidFrom >= after.Value)
            .ToListAsync();
    }

    public async Task<RequestAssignment> CreateRequestAssignment(Guid fromId, Guid toId, Guid roleId, Guid requestedBy)
    {
        var request = await db.RequestAssignments.FirstOrDefaultAsync(r => r.FromId == fromId && r.ToId == toId && r.RoleId == roleId && r.RequestedById == requestedBy && r.Status == RequestStatus.Pending);
        if (request == null)
        {
            request = new RequestAssignment
            {
                Id = Guid.NewGuid(),
                Status = RequestStatus.Pending,
                FromId = fromId,
                ToId = toId,
                RoleId = roleId,
                RequestedById = requestedBy
            };
            db.RequestAssignments.Add(request);

            var res = await db.SaveChangesAsync();

            if (res == 0)
            {
                throw new Exception("Failed to create request");
            }
        }

        return await GetRequestAssignment(request.Id);
    }

    public async Task<RequestAssignment> UpdateRequestAssignment(Guid requestId, RequestStatus status)
    {
        var request = await GetRequestAssignment(requestId);

        if (request == null)
        {
            throw new Exception("Request not found");
        }

        if (request.Status != status)
        {
            request.Status = status;
            await db.SaveChangesAsync();
        }

        return request;
    }

    public async Task<RequestAssignmentResource> GetRequestAssignmentResource(Guid requestId)
    {
        var request = await db.RequestAssignmentResources
            .Include(r => r.Assignment).ThenInclude(a => a.From)
            .Include(r => r.Assignment).ThenInclude(a => a.To)
            .Include(r => r.Assignment).ThenInclude(a => a.Role)
            .Include(r => r.Resource)
            .Include(r => r.RequestedBy)
            .FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null)
        {
            throw new Exception("Request not found");
        }

        return request;
    }

    public async Task<IEnumerable<RequestAssignmentResource>> GetRequestAssignmentResource(Guid? fromId, Guid? toId, Guid? roleId, Guid? resourceId, string? action, Guid? requestedBy, RequestStatus? status, DateTimeOffset? after, CancellationToken ct)
    {
        if (!fromId.HasValue && !toId.HasValue && !requestedBy.HasValue)
        {
            throw new ArgumentException("At least one of fromId, toId or requestedBy must be provided");
        }

        return await db.RequestAssignmentResources
            .Include(r => r.Assignment).ThenInclude(a => a.From)
            .Include(r => r.Assignment).ThenInclude(a => a.To)
            .Include(r => r.Assignment).ThenInclude(a => a.Role)
            .Include(r => r.Resource)
            .Include(r => r.RequestedBy)
            .WhereIf(fromId.HasValue, r => r.Assignment.FromId == fromId.Value)
            .WhereIf(toId.HasValue, r => r.Assignment.ToId == toId.Value)
            .WhereIf(roleId.HasValue, r => r.Assignment.RoleId == roleId.Value)
            .WhereIf(resourceId.HasValue, r => r.ResourceId == resourceId.Value)
            .WhereIf(!string.IsNullOrEmpty(action), r => r.Action == action)
            .WhereIf(requestedBy.HasValue, r => r.RequestedById == requestedBy.Value)
            .WhereIf(status.HasValue, r => r.Status == status.Value)
            .WhereIf(after.HasValue, r => r.Audit_ValidFrom >= after.Value)
            .ToListAsync();
    }

    public async Task<RequestAssignmentResource> CreateRequestAssignmentResource(Guid fromId, Guid toId, Guid roleId, Guid resourceId, string action, Guid requestedBy)
    {
        var assignment = await assignmentService.GetOrCreateAssignment(fromId, toId, roleId);
        return await CreateRequestAssignmentResource(assignment.Id, resourceId, action, requestedBy);
    }

    public async Task<RequestAssignmentResource> CreateRequestAssignmentResource(Guid assignmentId, Guid resourceId, string action, Guid requestedBy)
    {
        var request = await db.RequestAssignmentResources.FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.PackageId == packageId && r.RequestedById == requestedBy && r.Status == RequestStatus.Pending);
        if (request == null)
        {
            request = new RequestAssignmentResource
            {
                Id = Guid.NewGuid(),
                Status = RequestStatus.Pending,
                AssignmentId = assignmentId,
                ResourceId = resourceId,
                Action = action,
                RequestedById = requestedBy,
            };
            db.RequestAssignmentResources.Add(request);

            var res = await db.SaveChangesAsync();

            if (res == 0)
            {
                throw new Exception("Failed to create request");
            }
        }

        return await GetRequestAssignmentResource(request.Id);
    }

    public async Task<RequestAssignmentResource> UpdateRequestAssignmentResource(Guid requestId, RequestStatus status)
    {
        var request = await GetRequestAssignmentResource(requestId);

        if (request == null)
        {
            throw new Exception("Request not found");
        }

        if (request.Status != status)
        {
            request.Status = status;
            await db.SaveChangesAsync();
        }

        return request;
    }

    public async Task<RequestAssignmentPackage> GetRequestAssignmentPackage(Guid requestId)
    {
        var request = await db.RequestAssignmentPackages
            .Include(r => r.Assignment).ThenInclude(a => a.From)
            .Include(r => r.Assignment).ThenInclude(a => a.To)
            .Include(r => r.Assignment).ThenInclude(a => a.Role)
            .Include(r => r.Package)
            .Include(r => r.RequestedBy)
            .FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null)
        {
            throw new Exception("Request not found");
        }

        return request;
    }

    public async Task<IEnumerable<RequestAssignmentPackage>> GetRequestAssignmentPackage(Guid? fromId, Guid? toId, Guid? roleId, Guid? packageId, Guid? requestedBy, RequestStatus? status, DateTimeOffset? after, CancellationToken ct)
    {
        if (!fromId.HasValue && !toId.HasValue && !requestedBy.HasValue)
        {
            throw new ArgumentException("At least one of fromId, toId or requestedBy must be provided");
        }

        return await db.RequestAssignmentPackages
            .Include(r => r.Assignment).ThenInclude(a => a.From)
            .Include(r => r.Assignment).ThenInclude(a => a.To)
            .Include(r => r.Assignment).ThenInclude(a => a.Role)
            .Include(r => r.Package)
            .Include(r => r.RequestedBy)
            .WhereIf(fromId.HasValue, r => r.Assignment.FromId == fromId.Value)
            .WhereIf(toId.HasValue, r => r.Assignment.ToId == toId.Value)
            .WhereIf(roleId.HasValue, r => r.Assignment.RoleId == roleId.Value)
            .WhereIf(packageId.HasValue, r => r.PackageId == packageId.Value)
            .WhereIf(requestedBy.HasValue, r => r.RequestedById == requestedBy.Value)
            .WhereIf(status.HasValue, r => r.Status == status.Value)
            .WhereIf(after.HasValue, r => r.Audit_ValidFrom >= after.Value)
            .ToListAsync();
    }

    public async Task<RequestAssignmentPackage> CreateRequestAssignmentPackage(Guid fromId, Guid toId, Guid roleId, Guid packageId, Guid requestedBy)
    {
        var assignment = await assignmentService.GetOrCreateAssignment(fromId, toId, roleId);
        return await CreateRequestAssignmentPackage(assignment.Id, packageId, requestedBy);
    }

    public async Task<RequestAssignmentPackage> CreateRequestAssignmentPackage(Guid assignmentId, Guid packageId, Guid requestedBy)
    {
        var request = await db.RequestAssignmentPackages.FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.PackageId == packageId && r.RequestedById == requestedBy && r.Status == RequestStatus.Pending);
        if (request == null)
        {
            request = new RequestAssignmentPackage
            {
                Id = Guid.NewGuid(),
                Status = RequestStatus.Pending,
                AssignmentId = assignmentId,
                PackageId = packageId,
                RequestedById = requestedBy
            };
            db.RequestAssignmentPackages.Add(request);

            var res = await db.SaveChangesAsync();

            if (res == 0)
            {
                throw new Exception("Failed to create request");
            }
        }

        return await GetRequestAssignmentPackage(request.Id);
    }

    public async Task<RequestAssignmentPackage> UpdateRequestAssignmentPackage(Guid requestId, RequestStatus status)
    {
        var request = await GetRequestAssignmentPackage(requestId);

        if (request == null)
        {
            throw new Exception("Request not found");
        }

        if (request.Status != status)
        {
            request.Status = status;
            await db.SaveChangesAsync();
        }

        return request;
    }
}

/// <summary>
/// Common request dto for request assignment, request assignment resource and request assignment package
/// </summary>
public class RequestDto
{
    /// <summary>
    /// Request identifier
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// The party that is the source of the request (the one that has the role assignment that is being requested for assignment)
    /// </summary>
    public CompactEntityDto From { get; set; }

    /// <summary>
    /// The party that is the target of the request (the one that is being requested to be assigned the role)
    /// </summary>
    public CompactEntityDto To { get; set; }

    /// <summary>
    /// The user that requested the assignment
    /// </summary>
    public CompactEntityDto By { get; set; }

    /// <summary>
    /// The status of the request (e.g., pending, approved, rejected)
    /// </summary>
    public RequestStatus Status { get; set; }
}
