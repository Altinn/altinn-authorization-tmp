using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
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
    public async Task<IEnumerable<RequestDto>> GetRequests(Guid? fromId, Guid? toId, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct)
    {
        if (!fromId.HasValue && !toId.HasValue)
        {
            throw new ArgumentException("At least one of fromId, toId or requestedBy must be provided");
        }

        var r1 = await GetRequestAssignment(fromId, toId, null, status, after, ct);
        var r3 = await GetRequestAssignmentPackage(fromId, toId, null, null, status, after, ct);
        var r2 = await GetRequestAssignmentResource(fromId, toId, null, null, status, after, ct);

        return r1.Select(DtoMapper.Convert)
            .Union(r2.Select(DtoMapper.Convert))
            .Union(r3.Select(DtoMapper.Convert));
    }

    /// <inheritdoc/>
    public async Task<RequestAssignment> GetRequestAssignment(Guid requestId, CancellationToken ct = default)
    {
        return await db.RequestAssignments
            .Include(a => a.From)
            .Include(a => a.To)
            .Include(a => a.Role)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken: ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RequestAssignment>> GetRequestAssignment(Guid? fromId, Guid? toId, Guid? roleId, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct)
    {
        if (!fromId.HasValue && !toId.HasValue)
        {
            throw new ArgumentException("At least one of fromId, toId or requestedBy must be provided");
        }

        return await db.RequestAssignments
            .Include(a => a.From)
            .Include(a => a.To)
            .Include(a => a.Role)
            .WhereIf(fromId.HasValue, r => r.FromId == fromId.Value)
            .WhereIf(toId.HasValue, r => r.ToId == toId.Value)
            .WhereIf(roleId.HasValue, r => r.RoleId == roleId.Value)
            .WhereIf(status?.Any() == true, r => status.Contains(r.Status))
            .WhereIf(after.HasValue, r => r.Audit_ValidFrom >= after.Value)
            .ToListAsync(cancellationToken: ct);
    }

    /// <inheritdoc/>
    public async Task<Result<RequestAssignment>> CreateRequestAssignment(Guid fromId, Guid toId, Guid roleId, CancellationToken ct = default)
    {
        var request = await db.RequestAssignments.FirstOrDefaultAsync(r => r.FromId == fromId && r.ToId == toId && r.RoleId == roleId && r.Status == RequestStatus.Pending);
        if (request == null)
        {
            request = new RequestAssignment
            {
                Id = Guid.NewGuid(),
                Status = RequestStatus.Pending,
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

        return await GetRequestAssignment(request.Id, ct);
    }

    /// <inheritdoc/>
    public async Task<Result<RequestAssignment>> UpdateRequestAssignment(Guid requestId, RequestStatus status, CancellationToken ct = default)
    {
        var request = await GetRequestAssignment(requestId, ct);
        if (request is null)
        {
            return Problems.RequestNotFound;
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
        return await db.RequestAssignmentResources
            .Include(r => r.Assignment).ThenInclude(a => a.From)
            .Include(r => r.Assignment).ThenInclude(a => a.To)
            .Include(r => r.Assignment).ThenInclude(a => a.Role)
            .Include(r => r.Resource)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken: ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RequestAssignmentResource>> GetRequestAssignmentResource(Guid? fromId, Guid? toId, Guid? roleId, Guid? resourceId, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct)
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
            .WhereIf(roleId.HasValue, r => r.Assignment.RoleId == roleId.Value)
            .WhereIf(resourceId.HasValue, r => r.ResourceId == resourceId.Value)
            .WhereIf(status?.Any() == true, r => status.Contains(r.Status))
            .WhereIf(after.HasValue, r => r.Audit_ValidFrom >= after.Value)
            .ToListAsync(cancellationToken: ct);
    }

    /// <inheritdoc/>
    public async Task<Result<RequestAssignmentResource>> CreateRequestAssignmentResource(Guid fromId, Guid toId, Guid roleId, Guid resourceId, RequestStatus initialStatus = RequestStatus.Draft, CancellationToken ct = default)
    {
        var assignment = await assignmentService.GetOrCreateAssignment(fromId, toId, roleId, cancellationToken: ct);
        return await CreateRequestAssignmentResource(assignment.Id, resourceId, initialStatus, ct);
    }

    /// <inheritdoc/>
    public async Task<Result<RequestAssignmentResource>> CreateRequestAssignmentResource(Guid assignmentId, Guid resourceId, RequestStatus initialStatus = RequestStatus.Draft, CancellationToken ct = default)
    {
        var request = await db.RequestAssignmentResources.FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.ResourceId == resourceId && r.Status == initialStatus);
        if (request == null)
        {
            request = new RequestAssignmentResource
            {
                Id = Guid.NewGuid(),
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

        return await GetRequestAssignmentResource(request.Id, ct);
    }

    /// <inheritdoc/>
    public async Task<Result<RequestAssignmentResource>> UpdateRequestAssignmentResource(Guid requestId, RequestStatus status, CancellationToken ct = default)
    {
        var request = await GetRequestAssignmentResource(requestId, ct);
        if (request is null)
        {
            return Problems.RequestNotFound;
        }

        if (request.Status != status)
        {
            request.Status = status;
            await db.SaveChangesAsync(ct);
        }

        return request;
    }

    /// <inheritdoc/>
    public async Task<RequestAssignmentPackage> GetRequestAssignmentPackage(Guid requestId, CancellationToken ct = default)
    {
        return await db.RequestAssignmentPackages
            .Include(r => r.Assignment).ThenInclude(a => a.From)
            .Include(r => r.Assignment).ThenInclude(a => a.To)
            .Include(r => r.Assignment).ThenInclude(a => a.Role)
            .Include(r => r.Package)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken: ct);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RequestAssignmentPackage>> GetRequestAssignmentPackage(Guid? fromId, Guid? toId, Guid? roleId, Guid? packageId, IEnumerable<RequestStatus> status, DateTimeOffset? after, CancellationToken ct)
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
            .WhereIf(roleId.HasValue, r => r.Assignment.RoleId == roleId.Value)
            .WhereIf(packageId.HasValue, r => r.PackageId == packageId.Value)
            .WhereIf(status?.Any() == true, r => status.Contains(r.Status))
            .WhereIf(after.HasValue, r => r.Audit_ValidFrom >= after.Value)
            .ToListAsync(cancellationToken: ct);
    }

    /// <inheritdoc/>
    public async Task<Result<RequestAssignmentPackage>> CreateRequestAssignmentPackage(Guid fromId, Guid toId, Guid roleId, Guid packageId, RequestStatus initialStatus = RequestStatus.Draft, CancellationToken ct = default)
    {
        var assignment = await assignmentService.GetOrCreateAssignment(fromId, toId, roleId, cancellationToken: ct);
        return await CreateRequestAssignmentPackage(assignment.Id, packageId, initialStatus, ct);
    }

    /// <inheritdoc/>
    public async Task<Result<RequestAssignmentPackage>> CreateRequestAssignmentPackage(Guid assignmentId, Guid packageId, RequestStatus initialStatus = RequestStatus.Draft, CancellationToken ct = default)
    {
        var request = await db.RequestAssignmentPackages.FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.PackageId == packageId && r.Status == initialStatus, cancellationToken: ct);
        if (request is null)
        {
            request = new RequestAssignmentPackage
            {
                Id = Guid.NewGuid(),
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

        return await GetRequestAssignmentPackage(request.Id, ct);
    }

    /// <inheritdoc/>
    public async Task<Result<RequestAssignmentPackage>> UpdateRequestAssignmentPackage(Guid requestId, RequestStatus status, CancellationToken ct = default)
    {
        var request = await GetRequestAssignmentPackage(requestId, ct);
        if (request is null)
        {
            return Problems.RequestNotFound;
        }

        if (request.Status != status)
        {
            request.Status = status;
            await db.SaveChangesAsync(ct);
        }

        return request;
    }
}
