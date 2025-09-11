using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services;

/// <summary>
/// Defines a contract for managing and processing requests, including creating, accepting, rejecting,  and retrieving
/// requests and their associated packages and resources.
/// </summary>
/// <remarks>This interface provides methods for handling various operations related to requests, such as creating
/// new requests, accepting or rejecting requests, and retrieving details about requests, packages, and  resources. It
/// supports filtering and querying requests based on specific criteria, such as the sender,  recipient, or
/// intermediary.</remarks>
public interface IRequestService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="requestId"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    Task<bool> AcceptRequest(Guid requestId, string message);
    Task<bool> AcceptRequestPackage(Guid requestId, Guid packageId, string message);
    Task<bool> AcceptRequestResource(Guid requestId, Guid resourceId, string message);
    Task<Request> CreateRequest(Guid requestedBy, Guid fromId, Guid toId, Guid roleId, Guid? viaId, Guid? viaRoleId, List<Guid> packageIds, List<Guid> resourceIds);
    Task<Request> CreateRequest(Guid requestedBy, Guid fromId, Guid toId, Guid roleId, List<Guid> packageIds, List<Guid> resourceIds);

    Task<Request> GetRequest(Guid id);
    
    Task<IEnumerable<Request>> GetAllRequests(Guid? fromId, Guid? toId, Guid? viaId);
    Task<IEnumerable<Request>> GetOpenRequests(Guid? fromId, Guid? toId, Guid? viaId);
    Task<IEnumerable<RequestPackage>> GetRequestPackages(Guid requestId);
    Task<IEnumerable<RequestResource>> GetRequestResources(Guid requestId);
    Task<IEnumerable<RequestMessage>> GetRequestMessages(Guid requestId);
    Task<bool> RejectRequest(Guid requestId, string message);
    Task<bool> RejectRequestPackage(Guid requestId, Guid packageId, string message);
    Task<bool> RejectRequestResource(Guid requestId, Guid resourceId, string message);
}

public class RequestService(AppDbContext db) : IRequestService
{
    private readonly Guid statusAccepted = Guid.Parse("0195efb8-7c80-7c6d-aec6-5eafa8154ca1");
    private readonly Guid statusRejected = Guid.Parse("0195efb8-7c80-761b-b950-e709c703b6b1");
    private readonly Guid statusOpen = Guid.Parse("0195efb8-7c80-7239-8ee5-7156872b53d1");
    private readonly Guid statusClosed = Guid.Parse("0195efb8-7c80-7731-82a3-1f6b659ec848");

    public AuditValues AuditValues { get; set; } = new AuditValues(AuditDefaults.InternalApi, AuditDefaults.InternalApi, Guid.NewGuid().ToString());

    public async Task<bool> AcceptRequest(Guid requestId, string message)
    {
        return await ChangeRequestStatus(requestId, statusAccepted);
    }

    public async Task<bool> AcceptRequestPackage(Guid requestId, Guid packageId, string message)
    {
        return await ChangeRequestPackageStatus(requestId, packageId, statusAccepted);
    }

    public async Task<bool> AcceptRequestResource(Guid requestId, Guid resourceId, string message)
    {
        return await ChangeRequestResourceStatus(requestId, resourceId, statusAccepted);
    }

    public async Task<Request> CreateRequest(Guid requestedBy, Guid fromId, Guid toId, Guid roleId, List<Guid> packageIds, List<Guid> resourceIds)
    {
        return await CreateRequest(requestedBy, fromId, toId, roleId, null, null, packageIds, resourceIds);
    }

    public async Task<Request> CreateRequest(Guid requestedBy, Guid fromId, Guid toId, Guid roleId, Guid? viaId, Guid? viaRoleId, List<Guid> packageIds, List<Guid> resourceIds)
    {
        var newRequest = new Request()
        {
            Id = Guid.CreateVersion7(),
            FromId = fromId,
            ToId = toId,
            RoleId = roleId,
            ViaId = viaId,
            ViaRoleId = viaRoleId,
            RequestedById = requestedBy,
            StatusId = statusOpen
        };

        var packages = new List<RequestPackage>();
        foreach (var packageId in packageIds)
        {
            packages.Add(new RequestPackage()
            {
                Id = Guid.CreateVersion7(),
                RequestId = newRequest.Id,
                StatusId = statusOpen,
                PackageId = packageId
            });
        }

        var resources = new List<RequestResource>();
        foreach (var resourceId in resourceIds)
        {
            resources.Add(new RequestResource()
            {
                Id = Guid.CreateVersion7(),
                RequestId = newRequest.Id,
                StatusId = statusOpen,
                ResourceId = resourceId
            });
        }

        return await CreateRequest(newRequest, packages, resources);
    }

    private async Task<Request> CreateRequest(Request request, List<RequestPackage> packages, List<RequestResource> resources)
    {
        db.Requests.Add(request);
        db.RequestPackages.AddRange(packages);
        db.RequestResources.AddRange(resources);

        var res = await db.SaveChangesAsync(AuditValues);

        return request;
    }

    public async Task<IEnumerable<RequestPackage>> GetRequestPackages(Guid requestId)
    {
        return await db.RequestPackages.Where(t => t.RequestId == requestId).ToListAsync();
    }

    public async Task<IEnumerable<RequestResource>> GetRequestResources(Guid requestId)
    {
        return await db.RequestResources.Where(t => t.RequestId == requestId).ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetOpenRequests(Guid? fromId, Guid? toId, Guid? viaId)
    {
        if (!fromId.HasValue && !toId.HasValue && !viaId.HasValue)
        {
            return Enumerable.Empty<Request>();
        }

        var q = db.Requests.Where(t => t.StatusId == statusOpen);
        q.WhereIf(fromId.HasValue, t => t.FromId == fromId.Value);
        q.WhereIf(toId.HasValue, t => t.ToId == toId.Value);
        q.WhereIf(viaId.HasValue, t => t.ViaId == viaId.Value);

        return await q.ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetAllRequests(Guid? fromId, Guid? toId, Guid? viaId)
    {
        if (!fromId.HasValue && !toId.HasValue && !viaId.HasValue)
        {
            return Enumerable.Empty<Request>();
        }

        var q = db.Requests.WhereIf(fromId.HasValue, t => t.FromId == fromId.Value);
        q.WhereIf(toId.HasValue, t => t.ToId == toId.Value);
        q.WhereIf(viaId.HasValue, t => t.ViaId == viaId.Value);

        return await q.ToListAsync();
    }

    public async Task<bool> RejectRequest(Guid requestId, string message)
    {
        return await ChangeRequestStatus(requestId, statusRejected);
    }

    public async Task<bool> RejectRequestPackage(Guid requestId, Guid packageId, string message)
    {
        return await ChangeRequestPackageStatus(requestId, packageId, statusRejected);
    }

    public async Task<bool> RejectRequestResource(Guid requestId, Guid resourceId, string message)
    {
        return await ChangeRequestResourceStatus(requestId, resourceId, statusRejected);
    }

    private async Task<bool> ChangeRequestStatus(Guid requestId, Guid stausId)
    {
        var packages = await db.RequestPackages.Where(t => t.RequestId == requestId).ToListAsync();
        foreach (var package in packages)
        {
            package.StatusId = stausId;
        }

        var resources = await db.RequestResources.Where(t => t.RequestId == requestId).ToListAsync();
        foreach (var resource in resources)
        {
            resource.StatusId = stausId;
        }

        var request = await db.Requests.SingleAsync(t => t.Id == requestId);
        request.StatusId = stausId;

        var result = await db.SaveChangesAsync(AuditValues);
        return result > 0;
    }

    private async Task<bool> ChangeRequestPackageStatus(Guid requestId, Guid packageId, Guid stausId)
    {
        var request = await db.RequestPackages.SingleAsync(t => t.RequestId == requestId && t.PackageId == packageId);
        request.StatusId = stausId;
        var result = await db.SaveChangesAsync(AuditValues);
        return result > 0;
    }

    private async Task<bool> ChangeRequestResourceStatus(Guid requestId, Guid resourceId, Guid stausId)
    {
        var request = await db.RequestResources.SingleAsync(t => t.RequestId == requestId && t.ResourceId == resourceId);
        request.StatusId = stausId;
        var result = await db.SaveChangesAsync(AuditValues);
        return result > 0;
    }

    public async Task<Request> GetRequest(Guid id)
    {
        return await db.Requests.SingleAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<RequestMessage>> GetRequestMessages(Guid requestId)
    {
        return await db.RequestMessages.Where(t => t.RequestId == requestId).ToListAsync();
    }
}
