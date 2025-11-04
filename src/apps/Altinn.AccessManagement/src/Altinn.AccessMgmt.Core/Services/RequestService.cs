using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
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

    Task<Request> CreateRequest(Guid requestedBy, Guid fromId, Guid toId, Guid roleId, Guid? viaId, Guid? viaRoleId, List<Guid> packageIds, List<Guid> resourceIds, AuditValues audit = null, CancellationToken cancellationToken = default);
    
    Task<Request> CreateRequest(Guid requestedBy, Guid fromId, Guid toId, Guid roleId, List<Guid> packageIds, List<Guid> resourceIds, AuditValues audit = null, CancellationToken cancellationToken = default);

    Task<bool> AddRequestMessage(Guid requestId, string message, AuditValues audit = null, CancellationToken cancellationToken = default);

    Task<bool> AddRequestPackage(Guid requestId, Guid packageId, AuditValues audit = null, CancellationToken cancellationToken = default);
    
    Task<bool> AddRequestPackage(Guid requestId, List<Guid> packageIds, AuditValues audit = null, CancellationToken cancellationToken = default);

    Task<bool> AddRequestResource(Guid requestId, Guid resourceId, AuditValues audit = null, CancellationToken cancellationToken = default);

    Task<bool> AddRequestResource(Guid requestId, List<Guid> resourceIds, AuditValues audit = null, CancellationToken cancellationToken = default);

    Task<bool> RemoveRequestPackage(Guid requestId, Guid packageId, AuditValues audit = null, CancellationToken cancellationToken = default);

    Task<bool> RemoveRequestResource(Guid requestId, Guid resourceId, AuditValues audit = null, CancellationToken cancellationToken = default);

    Task<bool> AcceptRequest(Guid requestId, AuditValues audit = null, CancellationToken cancellationToken = default);

    Task<bool> AcceptRequestPackage(Guid requestId, Guid packageId, AuditValues audit = null, CancellationToken cancellationToken = default);
    
    Task<bool> AcceptRequestResource(Guid requestId, Guid resourceId, AuditValues audit = null, CancellationToken cancellationToken = default);
    
    Task<bool> RejectRequest(Guid requestId, AuditValues audit = null, CancellationToken cancellationToken = default);
    
    Task<bool> RejectRequestPackage(Guid requestId, Guid packageId, AuditValues audit = null, CancellationToken cancellationToken = default);
    
    Task<bool> RejectRequestResource(Guid requestId, Guid resourceId, AuditValues audit = null, CancellationToken cancellationToken = default);
}

public class RequestService(
    AppDbContext appDbContext,
    IAssignmentService assignmentService
    ) : IRequestService
{
    public AppDbContext Db { get; } = appDbContext;

    public async Task<bool> AcceptRequest(Guid requestId, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        // Loop all packages and resources connected to request and create

        var request = await Db.Requests.FirstOrDefaultAsync(t => t.Id == requestId);
        if (request == null)
        {
            return false;
        }

        var assignment = await assignmentService.GetOrCreateAssignment(request.FromId, request.ToId, request.RoleId);

        var requestPackages = await Db.RequestPackages.Where(t => t.RequestId == request.Id && t.StatusId == RequestStatusConstants.Open).ToListAsync();
        var requestResources = await Db.RequestResources.Where(t => t.RequestId == request.Id && t.StatusId == RequestStatusConstants.Open).ToListAsync();

        var addPackageResult = assignmentService.AddPackageToAssignment(audit.ChangedBy, assignment.Id, requestPackages.Select(t => t.PackageId));
        var addResourceResult = assignmentService.AddResourceToAssignment(audit.ChangedBy, assignment.Id, requestResources.Select(t => t.ResourceId));

        // Need return model. Dictionary<guid,bool> for Accepted posible/executed true/false.

        return await ChangeRequestStatus(requestId, RequestStatusConstants.Accepted);
    }

    public async Task<bool> AcceptRequestPackage(Guid requestId, Guid packageId, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        var request = await Db.Requests.FirstOrDefaultAsync(t => t.Id == requestId);
        if (request == null)
        {
            return false;
        }

        var assignment = await assignmentService.GetOrCreateAssignment(request.FromId, request.ToId, request.RoleId);
        if (assignment == null)
        {
            return false;
        }

        var requestPackage = await Db.RequestPackages.FirstOrDefaultAsync(t => t.RequestId == request.Id && t.PackageId == packageId);
        if (requestPackage == null)
        {
            return false;
        }

        var addPackageResult = assignmentService.AddPackageToAssignment(audit.ChangedBy, assignment.Id, requestPackage.PackageId);

        var statusResult = await ChangeRequestPackageStatus(requestId, packageId, RequestStatusConstants.Accepted);
        if (!statusResult)
        {
            return false;
        }

        // need return model

        var result = GetRequestPackage(requestId, packageId);
        return true;
    }

    public async Task<bool> AcceptRequestResource(Guid requestId, Guid resourceId, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        var request = await Db.Requests.FirstOrDefaultAsync(t => t.Id == requestId, cancellationToken);
        if (request == null)
        {
            return false;
        }

        var assignment = await assignmentService.GetOrCreateAssignment(request.FromId, request.ToId, request.RoleId, audit, cancellationToken);

        var requestResource = await Db.RequestResources.FirstOrDefaultAsync(t => t.RequestId == request.Id && t.ResourceId == resourceId, cancellationToken);
        if (requestResource == null)
        {
            return false;
        }

        var addPackageResult = assignmentService.AddResourceToAssignment(audit.ChangedBy, assignment.Id, requestResource.ResourceId);

        var statusResult = await ChangeRequestResourceStatus(requestId, resourceId, RequestStatusConstants.Accepted, audit, cancellationToken);
        if (!statusResult)
        {
            return false;
        }

        // need return model
        return true;
    }

    public async Task<Request> CreateRequest(Guid requestedBy, Guid fromId, Guid toId, Guid roleId, List<Guid> packageIds, List<Guid> resourceIds, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        return await CreateRequest(requestedBy, fromId, toId, roleId, null, null, packageIds, resourceIds, audit, cancellationToken);
    }

    public async Task<Request> CreateRequest(Guid requestedBy, Guid fromId, Guid toId, Guid roleId, Guid? viaId, Guid? viaRoleId, List<Guid> packageIds, List<Guid> resourceIds, AuditValues audit = null, CancellationToken cancellationToken = default)
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
            StatusId = RequestStatusConstants.Open
        };

        var packages = new List<RequestPackage>();
        foreach (var packageId in packageIds)
        {
            packages.Add(new RequestPackage()
            {
                Id = Guid.CreateVersion7(),
                RequestId = newRequest.Id,
                StatusId = RequestStatusConstants.Open,
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
                StatusId = RequestStatusConstants.Open,
                ResourceId = resourceId
            });
        }

        return await CreateRequest(newRequest, packages, resources, audit, cancellationToken);
    }

    private async Task<Request> CreateRequest(Request request, List<RequestPackage> packages, List<RequestResource> resources, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        Db.Requests.Add(request);
        Db.RequestPackages.AddRange(packages);
        Db.RequestResources.AddRange(resources);

        var res = await Db.SaveChangesWithAuditFallbackAsync(audit, cancellationToken);
        if (res == 0)
        {
            throw new Exception("No changes saved to database");
        }

        return request;
    }

    public async Task<IEnumerable<RequestPackage>> GetRequestPackages(Guid requestId)
    {
        return await Db.RequestPackages
            .AsNoTracking()
            .Include(t => t.Request)
            .Include(t => t.Package)
            .ThenInclude(t => t.Area)
            .Where(t => t.RequestId == requestId)
            .ToListAsync();
    }

    public async Task<RequestPackage> GetRequestPackage(Guid requestId, Guid packageId)
    {
        return await Db.RequestPackages
            .AsNoTracking()
            .Include(t => t.Request)
            .Include(t => t.Package)
            .ThenInclude(t => t.Area)
            .Where(t => t.RequestId == requestId)
            .Where(t => t.PackageId == packageId)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<RequestResource>> GetRequestResources(Guid requestId)
    {
        return await Db.RequestResources
            .AsNoTracking()
            .Include(t => t.Request)
            .Include(t => t.Resource)
            .Where(t => t.RequestId == requestId)
            .ToListAsync();
    }

    public async Task<RequestResource> GetRequestResource(Guid requestId, Guid resourceId)
    {
        return await Db.RequestResources
            .AsNoTracking()
            .Include(t => t.Request)
            .Include(t => t.Resource)
            .Where(t => t.RequestId == requestId)
            .Where(t => t.RequestId == requestId)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Request>> GetOpenRequests(Guid? fromId, Guid? toId, Guid? viaId)
    {
        if (!fromId.HasValue && !toId.HasValue && !viaId.HasValue)
        {
            return Enumerable.Empty<Request>();
        }

        return await Db.Requests
            .Where(t => t.StatusId == RequestStatusConstants.Open)
            .WhereIf(fromId.HasValue, t => t.FromId == fromId.Value)
            .WhereIf(toId.HasValue, t => t.ToId == toId.Value)
            .WhereIf(viaId.HasValue, t => t.ViaId == viaId.Value)
            .ToListAsync();
    }

    public async Task<IEnumerable<Request>> GetAllRequests(Guid? fromId, Guid? toId, Guid? viaId)
    {
        if (!fromId.HasValue && !toId.HasValue && !viaId.HasValue)
        {
            return Enumerable.Empty<Request>();
        }

        return await Db.Requests
            .WhereIf(fromId.HasValue, t => t.FromId == fromId.Value)
            .WhereIf(toId.HasValue, t => t.ToId == toId.Value)
            .WhereIf(viaId.HasValue, t => t.ViaId == viaId.Value)
            .ToListAsync();
    }

    public async Task<bool> RejectRequest(Guid requestId, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        return await ChangeRequestStatus(requestId, RequestStatusConstants.Rejected, audit, cancellationToken);
    }

    public async Task<bool> RejectRequestPackage(Guid requestId, Guid packageId, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        return await ChangeRequestPackageStatus(requestId, packageId, RequestStatusConstants.Rejected, audit, cancellationToken);
    }

    public async Task<bool> RejectRequestResource(Guid requestId, Guid resourceId, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        return await ChangeRequestResourceStatus(requestId, resourceId, RequestStatusConstants.Rejected, audit, cancellationToken);
    }

    private async Task<bool> ChangeRequestStatus(Guid requestId, Guid stausId, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        var packages = await Db.RequestPackages.Where(t => t.RequestId == requestId).ToListAsync(cancellationToken);
        foreach (var package in packages)
        {
            package.StatusId = stausId;
        }

        var resources = await Db.RequestResources.Where(t => t.RequestId == requestId).ToListAsync(cancellationToken);
        foreach (var resource in resources)
        {
            resource.StatusId = stausId;
        }

        var request = await Db.Requests.SingleAsync(t => t.Id == requestId,cancellationToken);
        request.StatusId = stausId;

        return await Db.SaveChangesWithAuditFallbackAsync(audit, cancellationToken) > 0;
    }

    private async Task<bool> ChangeRequestPackageStatus(Guid requestId, Guid packageId, Guid stausId, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        var request = await Db.RequestPackages.SingleAsync(t => t.RequestId == requestId && t.PackageId == packageId);
        request.StatusId = stausId;
        return await Db.SaveChangesWithAuditFallbackAsync(audit, cancellationToken) > 0;
    }

    private async Task<bool> ChangeRequestResourceStatus(Guid requestId, Guid resourceId, Guid stausId, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        var request = await Db.RequestResources.SingleAsync(t => t.RequestId == requestId && t.ResourceId == resourceId);
        request.StatusId = stausId;
        return await Db.SaveChangesWithAuditFallbackAsync(audit, cancellationToken) > 0;
    }

    public async Task<Request> GetRequest(Guid id)
    {
        return await Db.Requests
            .Include(t => t.From).ThenInclude(t => t.Type)
            .Include(t => t.To).ThenInclude(t => t.Type)
            .Include(t => t.Via).ThenInclude(t => t.Type)
            .SingleAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<RequestMessage>> GetRequestMessages(Guid requestId)
    {
        return await Db.RequestMessages.Where(t => t.RequestId == requestId).ToListAsync();
    }

    public async Task<bool> AddRequestPackage(Guid requestId, Guid packageId, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        Db.RequestPackages.Add(new RequestPackage() { Id = Guid.NewGuid(), RequestId = requestId, PackageId = packageId, StatusId = RequestStatusConstants.Open });
        return await Db.SaveChangesWithAuditFallbackAsync(audit, cancellationToken) > 0;
    }

    public async Task<bool> AddRequestPackage(Guid requestId, List<Guid> packageIds, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        var packages = packageIds.Select(packageId => new RequestPackage() { RequestId = requestId, PackageId = packageId, StatusId = RequestStatusConstants.Open });
        Db.RequestPackages.AddRange(packages);
        return await Db.SaveChangesWithAuditFallbackAsync(audit, cancellationToken) > 0;
    }

    public async Task<bool> AddRequestResource(Guid requestId, Guid resourceId, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        Db.RequestResources.Add(new RequestResource() { Id = Guid.NewGuid(), RequestId = requestId, ResourceId = resourceId, StatusId = RequestStatusConstants.Open });
        return await Db.SaveChangesWithAuditFallbackAsync(audit, cancellationToken) > 0;
    }

    public async Task<bool> AddRequestResource(Guid requestId, List<Guid> resourceIds, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        var resources = resourceIds.Select(resourceId => new RequestResource() { RequestId = requestId, ResourceId = resourceId, StatusId = RequestStatusConstants.Open });   
        Db.RequestResources.AddRange(resources);
        return await Db.SaveChangesWithAuditFallbackAsync(audit, cancellationToken) > 0;
    }

    public async Task<bool> RemoveRequestPackage(Guid requestId, Guid packageId, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        var requestPackage = await Db.RequestPackages.SingleAsync(t => t.RequestId == requestId && t.PackageId == packageId);
        Db.RequestPackages.Remove(requestPackage);
        return await Db.SaveChangesWithAuditFallbackAsync(audit, cancellationToken) > 0;
    }

    public async Task<bool> RemoveRequestResource(Guid requestId, Guid resourceId, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        var requestResource = await Db.RequestResources.SingleAsync(t => t.RequestId == requestId && t.ResourceId == resourceId);
        Db.RequestResources.Remove(requestResource);
        return await Db.SaveChangesWithAuditFallbackAsync(audit, cancellationToken) > 0;
    }

    public async Task<bool> AddRequestMessage(Guid requestId, string message, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        Db.RequestMessages.Add(new RequestMessage() 
        { 
            Id = Guid.CreateVersion7(), 
            AuthorId = audit.ChangedBy, 
            RequestId = requestId, 
            Content = message 
        });

        return await Db.SaveChangesWithAuditFallbackAsync(audit, cancellationToken) > 0;
    }
}
