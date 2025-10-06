using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
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
    Task<Request> GetRequest(Guid id);
    
    Task<IEnumerable<Request>> GetAllRequests(Guid? fromId, Guid? toId, Guid? viaId);

    Task<IEnumerable<Request>> GetOpenRequests(Guid? fromId, Guid? toId, Guid? viaId);
    
    Task<IEnumerable<RequestPackage>> GetRequestPackages(Guid requestId);
    
    Task<IEnumerable<RequestResource>> GetRequestResources(Guid requestId);
    
    Task<IEnumerable<RequestMessage>> GetRequestMessages(Guid requestId);

    Task<Request> CreateRequest(Guid requestedBy, Guid fromId, Guid toId, Guid roleId, Guid? viaId, Guid? viaRoleId, List<Guid> packageIds, List<Guid> resourceIds);
    
    Task<Request> CreateRequest(Guid requestedBy, Guid fromId, Guid toId, Guid roleId, List<Guid> packageIds, List<Guid> resourceIds);

    Task<bool> AddRequestMessage(Guid requestId, string message);

    Task<bool> AddRequestPackage(Guid requestId, Guid packageId);
    
    Task<bool> AddRequestPackage(Guid requestId, List<Guid> packageIds);

    Task<bool> AddRequestResource(Guid requestId, Guid resourceId);

    Task<bool> AddRequestResource(Guid requestId, List<Guid> resourceIds);

    Task<bool> RemoveRequestPackage(Guid requestId, Guid packageId);

    Task<bool> RemoveRequestResource(Guid requestId, Guid resourceId);

    Task<bool> AcceptRequest(Guid requestId);

    Task<bool> AcceptRequestPackage(Guid requestId, Guid packageId);
    
    Task<bool> AcceptRequestResource(Guid requestId, Guid resourceId);
    
    Task<bool> RejectRequest(Guid requestId);
    
    Task<bool> RejectRequestPackage(Guid requestId, Guid packageId);
    
    Task<bool> RejectRequestResource(Guid requestId, Guid resourceId);
}

public class RequestService(
    AppDbContextFactory appDbContextFactory, 
    IAuditAccessor auditAccessor,
    IAssignmentService assignmentService
    ) : IRequestService
{
    public AppDbContext Db { get; } = appDbContextFactory.CreateDbContext();

    private readonly Guid statusAccepted = Guid.Parse("0195efb8-7c80-7c6d-aec6-5eafa8154ca1");
    private readonly Guid statusRejected = Guid.Parse("0195efb8-7c80-761b-b950-e709c703b6b1");
    private readonly Guid statusOpen = Guid.Parse("0195efb8-7c80-7239-8ee5-7156872b53d1");
    private readonly Guid statusClosed = Guid.Parse("0195efb8-7c80-7731-82a3-1f6b659ec848");

    public async Task<bool> AcceptRequest(Guid requestId)
    {
        // Loop all packages and resources connected to request and create

        var request = await Db.Requests.FirstOrDefaultAsync(t => t.Id == requestId);
        if (request == null)
        {
            return false;
        }

        var assignment = await assignmentService.GetOrCreateAssignment(request.FromId, request.ToId, request.RoleId);

        var requestPackages = await Db.RequestPackages.Where(t => t.RequestId == request.Id && t.StatusId == statusOpen).ToListAsync();
        var requestResources = await Db.RequestResources.Where(t => t.RequestId == request.Id && t.StatusId == statusOpen).ToListAsync();

        var addPackageResult = assignmentService.AddPackageToAssignment(auditAccessor.AuditValues.ChangedBy, assignment.Id, requestPackages.Select(t => t.PackageId));
        var addResourceResult = assignmentService.AddResourceToAssignment(auditAccessor.AuditValues.ChangedBy, assignment.Id, requestResources.Select(t => t.ResourceId));

        // Need return model. Dictionary<guid,bool> for Accepted posible/executed true/false.

        return await ChangeRequestStatus(requestId, statusAccepted);
    }

    public async Task<bool> AcceptRequestPackage(Guid requestId, Guid packageId)
    {
        var request = await Db.Requests.FirstOrDefaultAsync(t => t.Id == requestId);
        if (request == null)
        {
            return false;
        }

        var assignment = await assignmentService.GetOrCreateAssignment(request.FromId, request.ToId, request.RoleId);

        var requestPackage = await Db.RequestPackages.FirstOrDefaultAsync(t => t.RequestId == request.Id && t.PackageId == packageId);
        if (requestPackage == null)
        {
            return false;
        }

        var addPackageResult = assignmentService.AddPackageToAssignment(auditAccessor.AuditValues.ChangedBy, assignment.Id, requestPackage.PackageId);

        var statusResult = await ChangeRequestPackageStatus(requestId, packageId, statusAccepted);
        if (!statusResult)
        {
            return false;
        }

        // need return model
        return true;
    }

    public async Task<bool> AcceptRequestResource(Guid requestId, Guid resourceId)
    {
        var request = await Db.Requests.FirstOrDefaultAsync(t => t.Id == requestId);
        if (request == null)
        {
            return false;
        }

        var assignment = await assignmentService.GetOrCreateAssignment(request.FromId, request.ToId, request.RoleId);

        var requestResource = await Db.RequestResources.FirstOrDefaultAsync(t => t.RequestId == request.Id && t.ResourceId == resourceId);
        if (requestResource == null)
        {
            return false;
        }

        var addPackageResult = assignmentService.AddResourceToAssignment(auditAccessor.AuditValues.ChangedBy, assignment.Id, requestResource.ResourceId);

        var statusResult = await ChangeRequestResourceStatus(requestId, resourceId, statusAccepted);
        if (!statusResult)
        {
            return false;
        }

        // need return model
        return true;
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
        Db.Requests.Add(request);
        Db.RequestPackages.AddRange(packages);
        Db.RequestResources.AddRange(resources);

        var res = await Db.SaveChangesAsync();

        return request;
    }

    public async Task<IEnumerable<RequestPackage>> GetRequestPackages(Guid requestId)
    {
        return await Db.RequestPackages.Where(t => t.RequestId == requestId).ToListAsync();
    }

    public async Task<IEnumerable<RequestResource>> GetRequestResources(Guid requestId)
    {
        return await Db.RequestResources.Where(t => t.RequestId == requestId).ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetOpenRequests(Guid? fromId, Guid? toId, Guid? viaId)
    {
        if (!fromId.HasValue && !toId.HasValue && !viaId.HasValue)
        {
            return Enumerable.Empty<Request>();
        }

        var q = Db.Requests.Where(t => t.StatusId == statusOpen);
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

        var q = Db.Requests.WhereIf(fromId.HasValue, t => t.FromId == fromId.Value);
        q.WhereIf(toId.HasValue, t => t.ToId == toId.Value);
        q.WhereIf(viaId.HasValue, t => t.ViaId == viaId.Value);

        return await q.ToListAsync();
    }

    public async Task<bool> RejectRequest(Guid requestId)
    {
        return await ChangeRequestStatus(requestId, statusRejected);
    }

    public async Task<bool> RejectRequestPackage(Guid requestId, Guid packageId)
    {
        return await ChangeRequestPackageStatus(requestId, packageId, statusRejected);
    }

    public async Task<bool> RejectRequestResource(Guid requestId, Guid resourceId)
    {
        return await ChangeRequestResourceStatus(requestId, resourceId, statusRejected);
    }

    private async Task<bool> ChangeRequestStatus(Guid requestId, Guid stausId)
    {
        var packages = await Db.RequestPackages.Where(t => t.RequestId == requestId).ToListAsync();
        foreach (var package in packages)
        {
            package.StatusId = stausId;
        }

        var resources = await Db.RequestResources.Where(t => t.RequestId == requestId).ToListAsync();
        foreach (var resource in resources)
        {
            resource.StatusId = stausId;
        }

        var request = await Db.Requests.SingleAsync(t => t.Id == requestId);
        request.StatusId = stausId;

        var result = await Db.SaveChangesAsync();
        return result > 0;
    }

    private async Task<bool> ChangeRequestPackageStatus(Guid requestId, Guid packageId, Guid stausId)
    {
        var request = await Db.RequestPackages.SingleAsync(t => t.RequestId == requestId && t.PackageId == packageId);
        request.StatusId = stausId;
        var result = await Db.SaveChangesAsync();
        return result > 0;
    }

    private async Task<bool> ChangeRequestResourceStatus(Guid requestId, Guid resourceId, Guid stausId)
    {
        var request = await Db.RequestResources.SingleAsync(t => t.RequestId == requestId && t.ResourceId == resourceId);
        request.StatusId = stausId;
        var result = await Db.SaveChangesAsync();
        return result > 0;
    }

    public async Task<Request> GetRequest(Guid id)
    {
        return await Db.Requests.SingleAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<RequestMessage>> GetRequestMessages(Guid requestId)
    {
        return await Db.RequestMessages.Where(t => t.RequestId == requestId).ToListAsync();
    }

    public async Task<bool> AddRequestPackage(Guid requestId, Guid packageId)
    {
        Db.RequestPackages.Add(new RequestPackage() { Id = Guid.NewGuid(), RequestId = requestId, PackageId = packageId, StatusId = statusOpen });
        var res = await Db.SaveChangesAsync();
        return res > 0;
    }

    public async Task<bool> AddRequestPackage(Guid requestId, List<Guid> packageIds)
    {
        var packages = packageIds.Select(packageId => new RequestPackage() { RequestId = requestId, PackageId = packageId, StatusId = statusOpen });
        Db.RequestPackages.AddRange(packages);
        var res = await Db.SaveChangesAsync();
        return res > 0;
    }

    public async Task<bool> AddRequestResource(Guid requestId, Guid resourceId)
    {
        Db.RequestResources.Add(new RequestResource() { Id = Guid.NewGuid(), RequestId = requestId, ResourceId = resourceId, StatusId = statusOpen });
        var res = await Db.SaveChangesAsync();
        return res > 0;
    }

    public async Task<bool> AddRequestResource(Guid requestId, List<Guid> resourceIds)
    {
        var resources = resourceIds.Select(resourceId => new RequestResource() { RequestId = requestId, ResourceId = resourceId, StatusId = statusOpen });   
        Db.RequestResources.AddRange(resources);
        var res = await Db.SaveChangesAsync();
        return res > 0;
    }

    public async Task<bool> RemoveRequestPackage(Guid requestId, Guid packageId)
    {
        var requestPackage = await Db.RequestPackages.SingleAsync(t => t.RequestId == requestId && t.PackageId == packageId);
        Db.RequestPackages.Remove(requestPackage);
        var res = await Db.SaveChangesAsync();
        return res > 0;
    }

    public async Task<bool> RemoveRequestResource(Guid requestId, Guid resourceId)
    {
        var requestResource = await Db.RequestResources.SingleAsync(t => t.RequestId == requestId && t.ResourceId == resourceId);
        Db.RequestResources.Remove(requestResource);
        var res = await Db.SaveChangesAsync();
        return res > 0;
    }

    public async Task<bool> AddRequestMessage(Guid requestId, string message)
    {
        Db.RequestMessages.Add(new RequestMessage() 
        { 
            Id = Guid.CreateVersion7(), 
            AuthorId = auditAccessor.AuditValues.ChangedBy, 
            RequestId = requestId, 
            Content = message 
        });

        var res = await Db.SaveChangesAsync();
        return res > 0;
    }
}
