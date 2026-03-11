using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Altinn.Authorization.Api.Contracts.AccessManagement.Request;
using Microsoft.EntityFrameworkCore;

namespace AccessMgmt.Tests.Services;

public class RequestServiceTests : IClassFixture<PostgresFixture>
{
    private static readonly AuditValues TestAudit = new(SystemEntityConstants.StaticDataIngest, SystemEntityConstants.StaticDataIngest);

    private static readonly Entity OrgFrom = new()
    {
        Id = Guid.Parse("01960001-0000-7000-8000-000000000001"),
        TypeId = EntityTypeConstants.Organization,
        VariantId = EntityVariantConstants.AS,
        Name = "RequestTest OrgFrom",
        RefId = "REQ-TEST-ORG-01",
    };

    private static readonly Entity PersonTo = new()
    {
        Id = Guid.Parse("01960001-0000-7000-8000-000000000002"),
        TypeId = EntityTypeConstants.Person,
        VariantId = EntityVariantConstants.Person,
        Name = "RequestTest PersonTo",
        RefId = "REQ-TEST-PERSON-01",
    };

    private static readonly ResourceType TestResourceType = new()
    {
        Id = Guid.Parse("01960001-0000-0000-0000-000000000001"),
        Name = "RequestTestResourceType",
    };

    private readonly AppDbContext _db;
    private readonly RequestService _requestService;

    public RequestServiceTests(PostgresFixture fixture)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(fixture.SharedDb.Admin.ToString())
            .Options;

        _db = new AppDbContext(options);
        _db.AuditAccessor = new AuditAccessor { AuditValues = TestAudit };

        SeedSharedData(_db).GetAwaiter().GetResult();

        var connectionQuery = new ConnectionQuery(_db);
        var assignmentService = new AssignmentService(_db, connectionQuery);
        _requestService = new RequestService(_db);
    }

    private static async Task SeedSharedData(AppDbContext db)
    {
        db.Entities.AddRange(OrgFrom, PersonTo);
        db.ResourceTypes.Add(TestResourceType);

        try
        {
            await db.SaveChangesAsync(TestAudit);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        db.ChangeTracker.Clear();
    }

    /// <summary>
    /// Seeds a unique <see cref="Resource"/> per test so create/update operations don't interfere with each other.
    /// </summary>
    private async Task<Resource> SeedUniqueResource()
    {
        var resource = new Resource
        {
            Id = Guid.CreateVersion7(),
            Name = "UniqueTestResource",
            Description = "UniqueTestResourceDescription",
            RefId = Guid.CreateVersion7().ToString(),
            ProviderId = ProviderConstants.ResourceRegistry.Id,
            TypeId = TestResourceType.Id,
        };
        _db.Resources.Add(resource);
        await _db.SaveChangesAsync(TestAudit);
        _db.ChangeTracker.Clear();
        return resource;
    }

    // -----------------------------------------------------------------------
    // CreateRequestAssignmentResource
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateRequestAssignmentResource_WithValidInput_CreatesDraftRequest()
    {
        var resource = await SeedUniqueResource();

        var result = await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Resource = resource.Id, Status = RequestStatus.Draft });

        Assert.False(result.IsProblem);
        Assert.Equal(RequestStatus.Draft, result.Value.Status);
        Assert.Equal(resource.Id, result.Value.Resource.Id);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public async Task CreateRequestAssignmentResource_CalledTwice_ReturnsExistingPendingRequest()
    {
        var resource = await SeedUniqueResource();

        //

        var first = (await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Resource = resource.Id, Status = RequestStatus.Draft })).Value;
        var second = (await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Resource = resource.Id, Status = RequestStatus.Draft })).Value;

        Assert.Equal(first.Id, second.Id);
    }

    // -----------------------------------------------------------------------
    // GetRequest (aggregate)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetRequest_WithResourceRequestId_ReturnsRequestDto()
    {
        var resource = await SeedUniqueResource();
        var created = (await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Resource = resource.Id, Status = RequestStatus.Draft })).Value;

        var result = await _requestService.GetRequest(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(RequestStatus.Draft, result.Status);
    }

    [Fact]
    public async Task GetRequest_WithPackageRequestId_ReturnsRequestDto()
    {
        var created = (await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Package = PackageConstants.Agriculture.Id, Status = RequestStatus.Draft })).Value;

        var result = await _requestService.GetRequest(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(RequestStatus.Draft, result.Status);
    }

    [Fact]
    public async Task GetRequest_WithNonExistingId_ReturnsNull()
    {
        var result = await _requestService.GetRequest(Guid.CreateVersion7());

        Assert.Null(result);
    }

    // -----------------------------------------------------------------------
    // GetRequests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetRequests_FilterByFromId_ReturnsOnlyMatchingRequests()
    {
        var resource = await SeedUniqueResource();
        await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Resource = resource.Id, Status = RequestStatus.Draft });

        var results = await _requestService.GetRequests(fromId: OrgFrom.Id, toId: null, status: [], after: null, ct: default);

        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(OrgFrom.Id, r.Connection.From.Id));
    }

    [Fact]
    public async Task GetRequests_FilterByToId_ReturnsOnlyMatchingRequests()
    {
        var resource = await SeedUniqueResource();
        await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Resource = resource.Id, Status = RequestStatus.Draft });

        var results = await _requestService.GetRequests(fromId: null, toId: PersonTo.Id, status: [], after: null, ct: default);

        Assert.NotEmpty(results);
        Assert.All(results, r => Assert.Equal(PersonTo.Id, r.Connection.To.Id));
    }

    [Fact]
    public async Task GetRequests_FilterByStatus_ExcludesNonMatchingRequests()
    {
        var resource = await SeedUniqueResource();
        await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Resource = resource.Id, Status = RequestStatus.Draft });

        // Filter by Approved — newly created request is Pending, so result should be empty
        var results = await _requestService.GetRequests(fromId: OrgFrom.Id, toId: null, status: [RequestStatus.Approved], after: null, ct: default);

        Assert.DoesNotContain(results, r => r.Connection.From.Id == OrgFrom.Id
            && r.Status == RequestStatus.Pending);
    }

    [Fact]
    public async Task GetRequests_WithoutFromOrToId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _requestService.GetRequests(fromId: null, toId: null, status: [], after: null, ct: default));
    }

    // -----------------------------------------------------------------------
    // UpdateRequestAssignmentResource
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UpdateRequestAssignmentResource_ApprovesRequest()
    {
        var resource = await SeedUniqueResource();
        var created = (await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Resource = resource.Id, Status = RequestStatus.Draft })).Value;

        var updated = await _requestService.UpdateRequest(created.Id, RequestStatus.Approved);

        Assert.False(updated.IsProblem);
        Assert.Equal(RequestStatus.Approved, updated.Value.Status);
    }

    [Fact]
    public async Task UpdateRequestAssignmentResource_SameStatus_IsIdempotent()
    {
        var resource = await SeedUniqueResource();
        var created = (await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Resource = resource.Id, Status = RequestStatus.Draft })).Value;

        var updated1 = (await _requestService.UpdateRequest(created.Id, RequestStatus.Pending)).Value;
        var updated2 = (await _requestService.UpdateRequest(created.Id, RequestStatus.Pending)).Value;

        Assert.Equal(RequestStatus.Pending, updated1.Status);
        Assert.Equal(RequestStatus.Pending, updated2.Status);
    }

    // -----------------------------------------------------------------------
    // CreateRequestAssignmentPackage
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateRequestAssignmentPackage_WithValidInput_CreatesDraftRequest()
    {
        var result = await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Package = PackageConstants.Agriculture.Id, Status = RequestStatus.Draft });

        Assert.False(result.IsProblem);
        Assert.Equal(RequestStatus.Draft, result.Value.Status);
        Assert.Equal(PackageConstants.Agriculture.Id, result.Value.Package.Id);
    }

    [Fact]
    public async Task CreateRequestAssignmentPackage_CalledTwice_ReturnsExistingPendingRequest()
    {
        var first = (await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Package = PackageConstants.Agriculture.Id, Status = RequestStatus.Draft })).Value;
        var second = (await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Package = PackageConstants.Agriculture.Id, Status = RequestStatus.Draft })).Value;

        Assert.Equal(first.Id, second.Id);
    }

    // -----------------------------------------------------------------------
    // UpdateRequestAssignmentPackage
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UpdateRequestAssignmentPackage_ApprovesRequest()
    {
        var resource = await SeedUniqueResource();

        // Use a different role so this assignment is distinct from the package tests above
        var created = (await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Resource = resource.Id, Status = RequestStatus.Draft })).Value;
        
        var request = (await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Package = PackageConstants.BusinessTax.Id, Status = RequestStatus.Draft })).Value;

        var updated = await _requestService.UpdateRequest(request.Id, RequestStatus.Approved);

        Assert.False(updated.IsProblem);
        Assert.Equal(RequestStatus.Approved, updated.Value.Status);
    }

    [Fact]
    public async Task UpdateRequestAssignmentPackage_RejectsRequest()
    {
        var request = (await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Package = PackageConstants.Fishing.Id, Status = RequestStatus.Draft })).Value;

        var updated = await _requestService.UpdateRequest(request.Id, RequestStatus.Rejected);

        Assert.False(updated.IsProblem);
        Assert.Equal(RequestStatus.Rejected, updated.Value.Status);
    }

    // -----------------------------------------------------------------------
    // Full lifecycle scenarios
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ResourceRequest_EnduserCreate_CreatesPendingRequest()
    {
        var resource = await SeedUniqueResource();

        var result = await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Resource = resource.Id, Status = RequestStatus.Pending });

        Assert.False(result.IsProblem);
        Assert.Equal(RequestStatus.Pending, result.Value.Status);
    }

    [Fact]
    public async Task ResourceRequest_FullLifecycle_DraftToPendingToAccepted()
    {
        var resource = await SeedUniqueResource();

        // 1. ServiceOwner creates request — status should be Draft
        var created = (await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Resource = resource.Id, Status = RequestStatus.Draft })).Value;
        Assert.Equal(RequestStatus.Draft, created.Status);

        // 2. Enduser sets status to Pending (acknowledges the request)
        var pending = (await _requestService.UpdateRequest(created.Id, RequestStatus.Pending)).Value;
        Assert.Equal(RequestStatus.Pending, pending.Status);

        // 3. ServiceOwner checks that status has changed from Draft
        var afterPending = await _requestService.GetRequest(created.Id);
        Assert.NotEqual(RequestStatus.Draft, afterPending.Status);
        Assert.Equal(RequestStatus.Pending, afterPending.Status);

        // 4. Enduser fetches the request
        var fetched = await _requestService.GetRequest(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);

        // 5. Enduser accepts
        var accepted = (await _requestService.UpdateRequest(created.Id, RequestStatus.Approved)).Value;
        Assert.Equal(RequestStatus.Approved, accepted.Status);
    }

    [Fact]
    public async Task PackageRequest_FullLifecycle_DraftToPendingToAccepted()
    {
        // 1. ServiceOwner creates request — status should be Draft
        var created = (await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Package = PackageConstants.Agriculture.Id, Status = RequestStatus.Draft })).Value;
        Assert.Equal(RequestStatus.Draft, created.Status);

        // 2. Enduser sets status to Pending (acknowledges the request)
        var pending = (await _requestService.UpdateRequest(created.Id, RequestStatus.Pending)).Value;
        Assert.Equal(RequestStatus.Pending, pending.Status);

        // 3. ServiceOwner checks that status has changed from Draft
        var afterPending = await _requestService.GetRequest(created.Id);
        Assert.NotEqual(RequestStatus.Draft, afterPending.Status);
        Assert.Equal(RequestStatus.Pending, afterPending.Status);

        // 4. Enduser fetches the request
        var fetched = await _requestService.GetRequest(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);

        // 5. Enduser accepts
        var accepted = (await _requestService.UpdateRequest(created.Id, RequestStatus.Approved)).Value;
        Assert.Equal(RequestStatus.Approved, accepted.Status);
    }

    [Fact]
    public async Task ResourceRequest_FullLifecycle_DraftToPendingToRejected()
    {
        var resource = await SeedUniqueResource();

        // 1. ServiceOwner creates request — status should be Draft
        var created = (await _requestService.CreateRequest(new CreateRequestDto() { From = OrgFrom.Id, To = PersonTo.Id, Role = RoleConstants.Rightholder.Id, Resource = resource.Id, Status = RequestStatus.Draft })).Value;
        Assert.Equal(RequestStatus.Draft, created.Status);

        // 2. Enduser sets status to Pending (acknowledges the request)
        var pending = (await _requestService.UpdateRequest(created.Id, RequestStatus.Pending)).Value;
        Assert.Equal(RequestStatus.Pending, pending.Status);

        // 3. ServiceOwner checks that status has changed from Draft
        var afterPending = await _requestService.GetRequest(created.Id);
        Assert.NotEqual(RequestStatus.Draft, afterPending.Status);
        Assert.Equal(RequestStatus.Pending, afterPending.Status);

        // 4. Enduser fetches the request
        var fetched = await _requestService.GetRequest(created.Id);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);

        // 5. Enduser rejects
        var rejected = (await _requestService.UpdateRequest(created.Id, RequestStatus.Rejected)).Value;
        Assert.Equal(RequestStatus.Rejected, rejected.Status);
    }
}
