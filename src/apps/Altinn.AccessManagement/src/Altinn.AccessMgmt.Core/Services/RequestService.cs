using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc/>
public class RequestService(AppDbContext db, IAssignmentService assignmentService) : IRequestService
{
    /// <inheritdoc/>
    public async Task<RequestDto> GetRequest(Guid requestId, CancellationToken ct = default)
    {
        var r1 = await GetRequestAssignment(requestId, ct: ct);
        if (r1 != null)
        {
            return DtoMapper.Convert(r1);
        }

        var r2 = await GetRequestAssignmentPackage(requestId, ct: ct);
        if (r2 != null) 
        {
            return DtoMapper.Convert(r2);
        }

        var r3 = await GetRequestAssignmentResource(requestId, ct: ct);
        if (r3 != null) 
        {
            return DtoMapper.Convert(r3);
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RequestDto>> GetRequests(Guid? fromId, Guid? toId, Guid? requestedBy, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct)
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

    /// <inheritdoc/>
    public async Task<RequestAssignment> GetRequestAssignment(Guid requestId, CancellationToken ct = default)
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

    /// <inheritdoc/>
    public async Task<IEnumerable<RequestAssignment>> GetRequestAssignment(Guid? fromId, Guid? toId, Guid? roleId, Guid? requestedBy, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct)
    {
        if (!fromId.HasValue && !toId.HasValue && !requestedBy.HasValue)
        {
            throw new ArgumentException("At least one of fromId, toId or requestedBy must be provided");
        }

        HashSet<RequestStatus> statusSet = status.Select(e => (RequestStatus)e).ToHashSet();

        return await db.RequestAssignments
            .Include(a => a.From)
            .Include(a => a.To)
            .Include(a => a.Role)
            .Include(r => r.RequestedBy)
            .WhereIf(fromId.HasValue, r => r.FromId == fromId.Value)
            .WhereIf(toId.HasValue, r => r.ToId == toId.Value)
            .WhereIf(roleId.HasValue, r => r.RoleId == roleId.Value)
            .WhereIf(requestedBy.HasValue, r => r.RequestedById == requestedBy.Value)
            .WhereMatchIfSet(statusSet, r => r.Status)
            .WhereIf(after.HasValue, r => r.Audit_ValidFrom >= after.Value)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<RequestAssignment> CreateRequestAssignment(Guid fromId, Guid toId, Guid roleId, Guid requestedBy, CancellationToken ct = default)
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

    /// <inheritdoc/>
    public async Task<RequestAssignment> UpdateRequestAssignment(Guid requestId, RequestStatus status, CancellationToken ct = default)
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

    /// <inheritdoc/>
    public async Task<RequestAssignmentResource> GetRequestAssignmentResource(Guid requestId, CancellationToken ct = default)
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

    /// <inheritdoc/>
    public async Task<IEnumerable<RequestAssignmentResource>> GetRequestAssignmentResource(Guid? fromId, Guid? toId, Guid? roleId, Guid? resourceId, string? action, Guid? requestedBy, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct)
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
            .WhereIf(status.Any(), r => status.Contains(r.Status))
            .WhereIf(after.HasValue, r => r.Audit_ValidFrom >= after.Value)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<RequestAssignmentResource> CreateRequestAssignmentResource(Guid fromId, Guid toId, Guid roleId, Guid resourceId, string action, Guid requestedBy, CancellationToken ct = default)
    {
        var assignment = await assignmentService.GetOrCreateAssignment(fromId, toId, roleId);
        return await CreateRequestAssignmentResource(assignment.Id, resourceId, action, requestedBy);
    }

    /// <inheritdoc/>
    public async Task<RequestAssignmentResource> CreateRequestAssignmentResource(Guid assignmentId, Guid resourceId, string action, Guid requestedBy, CancellationToken ct = default)
    {
        var request = await db.RequestAssignmentResources.FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.ResourceId == resourceId && r.RequestedById == requestedBy && r.Status == RequestStatus.Pending);
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

    /// <inheritdoc/>
    public async Task<RequestAssignmentResource> UpdateRequestAssignmentResource(Guid requestId, RequestStatus status, CancellationToken ct = default)
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

    /// <inheritdoc/>
    public async Task<RequestAssignmentPackage> GetRequestAssignmentPackage(Guid requestId, CancellationToken ct = default)
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

    /// <inheritdoc/>
    public async Task<IEnumerable<RequestAssignmentPackage>> GetRequestAssignmentPackage(Guid? fromId, Guid? toId, Guid? roleId, Guid? packageId, Guid? requestedBy, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct)
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
            .WhereIf(status.Any(), r => status.Contains(r.Status))
            .WhereIf(after.HasValue, r => r.Audit_ValidFrom >= after.Value)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<RequestAssignmentPackage> CreateRequestAssignmentPackage(Guid fromId, Guid toId, Guid roleId, Guid packageId, Guid requestedBy, CancellationToken ct = default)
    {
        var assignment = await assignmentService.GetOrCreateAssignment(fromId, toId, roleId);
        return await CreateRequestAssignmentPackage(assignment.Id, packageId, requestedBy);
    }

    /// <inheritdoc/>
    public async Task<RequestAssignmentPackage> CreateRequestAssignmentPackage(Guid assignmentId, Guid packageId, Guid requestedBy, CancellationToken ct = default)
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

    /// <inheritdoc/>
    public async Task<RequestAssignmentPackage> UpdateRequestAssignmentPackage(Guid requestId, RequestStatus status, CancellationToken ct = default)
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
