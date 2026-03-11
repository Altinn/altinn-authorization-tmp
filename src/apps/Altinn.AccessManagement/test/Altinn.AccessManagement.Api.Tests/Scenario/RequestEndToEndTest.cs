using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.TestUtils;
using Altinn.AccessManagement.TestUtils.Data;
using Altinn.AccessManagement.TestUtils.Fixtures;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.Api.Contracts.AccessManagement.Request;

namespace Altinn.AccessManagement.Api.Tests.Scenario;

public class RequestEndToEndTest
{
    private const string ServiceOwnerRoute = "accessmanagement/api/v1/serviceowner/delegationrequests";
    private const string EnduserRoute = "accessmanagement/api/v1/enduser/request";

    private static HttpClient CreateServiceOwnerClient(ApiFixture fixture, Guid partyUuid)
    {
        var client = fixture.Server.CreateClient();
        var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
        {
            claims.Add(new Claim(AltinnCoreClaimTypes.PartyUuid, partyUuid.ToString()));
            claims.Add(new Claim("scope", AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_WRITE));
        });
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return client;
    }

    private static HttpClient CreateServiceOwnerReadClient(ApiFixture fixture)
    {
        var client = fixture.Server.CreateClient();
        var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
        {
            claims.Add(new Claim(AltinnCoreClaimTypes.PartyUuid, TestEntities.OrganizationNordisAS.Id.ToString()));
            claims.Add(new Claim("scope", AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_READ));
        });
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return client;
    }

    private static HttpClient CreateEnduserClient(ApiFixture fixture, Guid partyUuid)
    {
        var client = fixture.Server.CreateClient();
        var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
        {
            claims.Add(new Claim(AltinnCoreClaimTypes.PartyUuid, partyUuid.ToString()));
            claims.Add(new Claim("scope", AuthzConstants.SCOPE_PORTAL_ENDUSER));
            claims.Add(new Claim("scope", AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ));
            claims.Add(new Claim("scope", AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE));
        });
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return client;
    }

    //#region Package request E2E lifecycle — reject

    //public class PackageRequest_EndToEnd_RejectLifecycle : IClassFixture<ApiFixture>
    //{
    //    private readonly ApiFixture _fixture;

    //    public PackageRequest_EndToEnd_RejectLifecycle(ApiFixture fixture)
    //    {
    //        _fixture = fixture;
    //        _fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
    //        _fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage);
    //    }

    //    [Fact]
    //    public async Task PackageRequest_EndToEnd_RejectLifecycle_FullFlow()
    //    {
    //        var from = $"urn:altinn:organization:identifier-no:{TestEntities.OrganizationNordisAS.Entity.OrganizationIdentifier}";
    //        var to = $"urn:altinn:person:identifier-no:{TestEntities.PersonPaula.Entity.PersonIdentifier}";

    //        // Step 1: SO creates a package request (status=Draft by default)
    //        var soClient = CreateServiceOwnerClient(_fixture, TestEntities.OrganizationNordisAS.Id);
    //        var createBody = new CreateRequestInput
    //        {
    //            Package = new RequestRefrenceDto { Urn = PackageConstants.Agriculture.Entity.Urn }
    //        };

    //        var createResponse = await soClient.PostAsJsonAsync(
    //            $"{ServiceOwnerRoute}?party=&to=",
    //            createBody,
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);

    //        var createJson = await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    //        using var createDoc = JsonDocument.Parse(createJson);
    //        var createRoot = createDoc.RootElement;
    //        Assert.Equal((int)RequestStatus.Draft, createRoot.GetProperty("status").GetInt32());

    //        var requestId = createRoot.GetProperty("id").GetString();
    //        Assert.False(string.IsNullOrEmpty(requestId), "Request ID should be returned");

    //        // Step 2: EU confirms the draft request (Draft → Pending)
    //        var enduserClient = CreateEnduserClient(_fixture, TestEntities.PersonPaula.Id);
    //        var confirmResponse = await enduserClient.PutAsync(
    //            $"{EnduserRoute}/sent/confirm?id={requestId}&party={TestEntities.PersonPaula.Id}",
    //            null,
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

    //        var confirmJson = await confirmResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    //        using var confirmDoc = JsonDocument.Parse(confirmJson);
    //        Assert.Equal((int)RequestStatus.Pending, confirmDoc.RootElement.GetProperty("status").GetInt32());

    //        // Step 3: SO verifies the request is now Pending
    //        var soReadClient = CreateServiceOwnerReadClient(_fixture);
    //        var soGetResponse = await soReadClient.GetAsync(
    //            $"{ServiceOwnerRoute}/{requestId}",
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.OK, soGetResponse.StatusCode);

    //        var soGetJson = await soGetResponse.Content.ReadFromJsonAsync<RequestDto>(TestContext.Current.CancellationToken);
    //        Assert.Equal(RequestStatus.Pending, soGetJson.Status);

    //        // Step 4: EU sees the request in their list
    //        var enduserGetResponse = await enduserClient.GetAsync(
    //            $"{EnduserRoute}?from={TestEntities.OrganizationNordisAS.Id}&to={TestEntities.PersonPaula.Id}&party={TestEntities.PersonPaula.Id}",
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.OK, enduserGetResponse.StatusCode);

    //        var enduserGetJson = await enduserGetResponse.Content.ReadFromJsonAsync<PaginatedResult<RequestDto>>(TestContext.Current.CancellationToken);
    //        Assert.Contains(requestId, enduserGetJson.Items.Select(t => t.Id.ToString()));

    //        // Step 5: EU rejects the request
    //        var rejectResponse = await enduserClient.PutAsync(
    //            $"{EnduserRoute}/received/reject?id={requestId}&party={TestEntities.PersonPaula.Id}",
    //            null,
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);

    //        var rejectJson = await rejectResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    //        using var rejectDoc = JsonDocument.Parse(rejectJson);
    //        Assert.Equal((int)RequestStatus.Rejected, rejectDoc.RootElement.GetProperty("status").GetInt32());
    //    }
    //}

    //#endregion

    //#region Resource request E2E lifecycle — reject

    //public class ResourceRequest_EndToEnd_RejectLifecycle : IClassFixture<ApiFixture>
    //{
    //    private static readonly ResourceType TestResourceType = new()
    //    {
    //        Id = Guid.Parse("01960100-0000-7000-8000-000000000001"),
    //        Name = "E2ERejectResourceType",
    //    };

    //    private readonly ApiFixture _fixture;

    //    public ResourceRequest_EndToEnd_RejectLifecycle(ApiFixture fixture)
    //    {
    //        _fixture = fixture;
    //        _fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
    //        _fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage);
    //        _fixture.EnsureSeedOnce(db =>
    //        {
    //            db.ResourceTypes.Add(TestResourceType);
    //            db.SaveChanges();

    //            db.Resources.Add(new Resource
    //            {
    //                Id = Guid.CreateVersion7(),
    //                Name = "E2ERejectTestResource",
    //                Description = "Resource for E2E reject lifecycle test",
    //                RefId = "e2e-reject-test-resource-1",
    //                ProviderId = ProviderConstants.ResourceRegistry,
    //                TypeId = TestResourceType.Id,
    //            });
    //            db.SaveChanges();
    //        });
    //    }

    //    [Fact]
    //    public async Task ResourceRequest_EndToEnd_RejectLifecycle_FullFlow()
    //    {
    //        var from = $"urn:altinn:organization:identifier-no:{TestEntities.OrganizationNordisAS.Entity.OrganizationIdentifier}";
    //        var to = $"urn:altinn:person:identifier-no:{TestEntities.PersonPaula.Entity.PersonIdentifier}";

    //        // Step 1: SO creates a resource request (status=Draft by default)
    //        var soClient = CreateServiceOwnerClient(_fixture, TestEntities.OrganizationNordisAS.Id);
    //        var createBody = new CreateResourceRequestInput
    //        {
    //            Resource = new ResourceReferenceDto { ResourceId = "e2e-reject-test-resource-1" }
    //        };

    //        var createResponse = await soClient.PostAsJsonAsync(
    //            $"{ServiceOwnerRoute}?party=&to=",
    //            createBody,
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);

    //        var createJson = await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    //        using var createDoc = JsonDocument.Parse(createJson);
    //        var createRoot = createDoc.RootElement;
    //        Assert.Equal((int)RequestStatus.Draft, createRoot.GetProperty("status").GetInt32());

    //        var requestId = createRoot.GetProperty("id").GetString();
    //        Assert.False(string.IsNullOrEmpty(requestId), "Request ID should be returned");

    //        // Step 2: EU confirms the draft request (Draft → Pending)
    //        var enduserClient = CreateEnduserClient(_fixture, TestEntities.PersonPaula.Id);
    //        var confirmResponse = await enduserClient.PutAsync(
    //            $"{EnduserRoute}/confirm?party={TestEntities.PersonPaula.Id}",
    //            null,
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

    //        var confirmJson = await confirmResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    //        using var confirmDoc = JsonDocument.Parse(confirmJson);
    //        Assert.Equal((int)RequestStatus.Pending, confirmDoc.RootElement.GetProperty("status").GetInt32());

    //        // Step 3: SO verifies the request is now Pending
    //        var soReadClient = CreateServiceOwnerReadClient(_fixture);
    //        var soGetResponse = await soReadClient.GetAsync(
    //            $"{ServiceOwnerRoute}/resource?from={from}&to={to}",
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.OK, soGetResponse.StatusCode);

    //        var soGetJson = await soGetResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    //        using var soGetDoc = JsonDocument.Parse(soGetJson);
    //        var soItems = soGetDoc.RootElement.EnumerateArray().ToList();
    //        var soMatch = soItems.FirstOrDefault(i => i.GetProperty("id").GetString() == requestId);
    //        Assert.Equal((int)RequestStatus.Pending, soMatch.GetProperty("status").GetInt32());

    //        // Step 4: EU sees the request in their list
    //        var enduserGetResponse = await enduserClient.GetAsync(
    //            $"{EnduserRoute}?from={TestEntities.OrganizationNordisAS.Id}&to={TestEntities.PersonPaula.Id}&party={TestEntities.PersonPaula.Id}",
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.OK, enduserGetResponse.StatusCode);

    //        var enduserGetJson = await enduserGetResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    //        using var enduserGetDoc = JsonDocument.Parse(enduserGetJson);
    //        var enduserItems = enduserGetDoc.RootElement.GetProperty("data").EnumerateArray().ToList();
    //        Assert.Contains(enduserItems, i => i.GetProperty("id").GetString() == requestId);

    //        // Step 5: EU rejects the request
    //        var rejectResponse = await enduserClient.PutAsync(
    //            $"{EnduserRoute}/{requestId}/reject?party={TestEntities.PersonPaula.Id}",
    //            null,
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);

    //        var rejectJson = await rejectResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    //        using var rejectDoc = JsonDocument.Parse(rejectJson);
    //        Assert.Equal((int)RequestStatus.Rejected, rejectDoc.RootElement.GetProperty("status").GetInt32());
    //    }
    //}

    //#endregion

    //#region Package request E2E lifecycle — accept

    //public class PackageRequest_EndToEnd_AcceptLifecycle : IClassFixture<ApiFixture>
    //{
    //    private readonly ApiFixture _fixture;

    //    public PackageRequest_EndToEnd_AcceptLifecycle(ApiFixture fixture)
    //    {
    //        _fixture = fixture;
    //        _fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
    //        _fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage);
    //        _fixture.EnsureSeedOnce(db =>
    //        {
    //            db.Assignments.Add(new Assignment
    //            {
    //                Id = Guid.CreateVersion7(),
    //                FromId = TestEntities.OrganizationNordisAS.Id,
    //                ToId = TestEntities.PersonOrjan.Id,
    //                RoleId = RoleConstants.ManagingDirector.Id,
    //            });
    //            db.SaveChanges();
    //        });
    //    }

    //    [Fact]
    //    public async Task PackageRequest_EndToEnd_AcceptLifecycle_FullFlow()
    //    {
    //        var from = $"urn:altinn:organization:identifier-no:{TestEntities.OrganizationNordisAS.Entity.OrganizationIdentifier}";
    //        var to = $"urn:altinn:person:identifier-no:{TestEntities.PersonPaula.Entity.PersonIdentifier}";

    //        // Step 1: SO creates a package request (status=Draft)
    //        var soClient = CreateServiceOwnerClient(_fixture, TestEntities.OrganizationNordisAS.Id);
    //        var createBody = new CreateServiceOwnerRequest
    //        {
    //            Connection = new ConnectionRequestInputDto()
    //            {
    //                From = from,
    //                To = to,
    //            },
    //            Package = new RequestRefrenceDto { Urn = PackageConstants.Agriculture.Entity.Urn }
    //        };

    //        var createResponse = await soClient.PostAsJsonAsync(
    //            $"{ServiceOwnerRoute}",
    //            createBody,
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);

    //        var createJson = await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    //        using var createDoc = JsonDocument.Parse(createJson);
    //        var requestId = createDoc.RootElement.GetProperty("id").GetString();
    //        Assert.False(string.IsNullOrEmpty(requestId));

    //        // Step 2: EU confirms (Draft → Pending)
    //        var enduserClient = CreateEnduserClient(_fixture, TestEntities.PersonOrjan.Id);
    //        var confirmResponse = await enduserClient.PutAsync(
    //            $"{EnduserRoute}/sent/confirm?id={requestId}&party={TestEntities.OrganizationNordisAS.Id}",
    //            null,
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

    //        // Step 3: SO verifies Pending
    //        var soReadClient = CreateServiceOwnerReadClient(_fixture);
    //        var soGetResponse = await soReadClient.GetAsync(
    //            $"{ServiceOwnerRoute}/{requestId}/status",
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.OK, soGetResponse.StatusCode);

    //        var status = await soGetResponse.Content.ReadFromJsonAsync<RequestStatus>(TestContext.Current.CancellationToken);
    //        Assert.Equal(RequestStatus.Pending, status);

    //        // Step 4: EU sees the request
    //        var enduserGetResponse = await enduserClient.GetAsync(
    //            $"{EnduserRoute}/received?party={TestEntities.PersonPaula.Id}&from={TestEntities.OrganizationNordisAS.Id}",
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.OK, enduserGetResponse.StatusCode);

    //        // Step 5: EU approves — expect 400 (delegation auth failure proves routing works)
    //        var approveResponse = await enduserClient.PutAsync(
    //            $"{EnduserRoute}/received/approve?id={requestId}&party={TestEntities.PersonPaula.Id}",
    //            null,
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.BadRequest, approveResponse.StatusCode);
    //    }
    //}

    //#endregion

    //#region Resource request E2E lifecycle — accept

    //public class ResourceRequest_EndToEnd_AcceptLifecycle : IClassFixture<ApiFixture>
    //{
    //    private static readonly ResourceType TestResourceType = new()
    //    {
    //        Id = Guid.Parse("01960100-0000-7000-8000-000000000011"),
    //        Name = "E2EAcceptResourceType",
    //    };

    //    private readonly ApiFixture _fixture;

    //    public ResourceRequest_EndToEnd_AcceptLifecycle(ApiFixture fixture)
    //    {
    //        _fixture = fixture;
    //        _fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
    //        _fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage);
    //        _fixture.EnsureSeedOnce(db =>
    //        {
    //            db.ResourceTypes.Add(TestResourceType);
    //            db.SaveChanges();

    //            db.Resources.Add(new Resource
    //            {
    //                Id = Guid.CreateVersion7(),
    //                Name = "E2EAcceptTestResource",
    //                Description = "Resource for E2E accept lifecycle test",
    //                RefId = "e2e-accept-test-resource-1",
    //                ProviderId = ProviderConstants.ResourceRegistry,
    //                TypeId = TestResourceType.Id,
    //            });
    //            db.SaveChanges();
    //        });
    //    }

    //    [Fact]
    //    public async Task ResourceRequest_EndToEnd_AcceptLifecycle_FullFlow()
    //    {
    //        var from = $"urn:altinn:organization:identifier-no:{TestEntities.OrganizationNordisAS.Entity.OrganizationIdentifier}";
    //        var to = $"urn:altinn:person:identifier-no:{TestEntities.PersonPaula.Entity.PersonIdentifier}";

    //        // Step 1: SO creates a resource request (status=Draft)
    //        var soClient = CreateServiceOwnerClient(_fixture, TestEntities.OrganizationNordisAS.Id);
    //        var createBody = new CreateResourceRequestInput
    //        {
    //            Resource = new ResourceReferenceDto { ResourceId = "e2e-accept-test-resource-1" }
    //        };

    //        var createResponse = await soClient.PostAsJsonAsync(
    //            $"{ServiceOwnerRoute}?party=&to=",
    //            createBody,
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);

    //        var createJson = await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    //        using var createDoc = JsonDocument.Parse(createJson);
    //        var requestId = createDoc.RootElement.GetProperty("id").GetString();
    //        Assert.False(string.IsNullOrEmpty(requestId));

    //        // Step 2: EU confirms (Draft → Pending)
    //        var enduserClient = CreateEnduserClient(_fixture, TestEntities.PersonPaula.Id);
    //        var confirmResponse = await enduserClient.PutAsync(
    //            $"{EnduserRoute}/sent/confirm?id={requestId}&party={TestEntities.PersonPaula.Id}",
    //            null,
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

    //        // Step 3: SO verifies Pending
    //        var soReadClient = CreateServiceOwnerReadClient(_fixture);
    //        var soGetResponse = await soReadClient.GetAsync(
    //            $"{ServiceOwnerRoute}/{requestId}",
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.OK, soGetResponse.StatusCode);

    //        var soGetJson = await soGetResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    //        using var soGetDoc = JsonDocument.Parse(soGetJson);
    //        var soItems = soGetDoc.RootElement.EnumerateArray().ToList();
    //        var soMatch = soItems.FirstOrDefault(i => i.GetProperty("id").GetString() == requestId);
    //        Assert.Equal((int)RequestStatus.Pending, soMatch.GetProperty("status").GetInt32());

    //        // Step 4: EU sees the request
    //        var enduserGetResponse = await enduserClient.GetAsync(
    //            $"{EnduserRoute}?from={TestEntities.OrganizationNordisAS.Id}&to={TestEntities.PersonPaula.Id}&party={TestEntities.PersonPaula.Id}",
    //            TestContext.Current.CancellationToken);

    //        Assert.Equal(HttpStatusCode.OK, enduserGetResponse.StatusCode);

    //        // Step 5: EU accepts — expect 500 (Azure storage unavailable proves routing works)
    //        var acceptResponse = await enduserClient.PutAsync(
    //            $"{EnduserRoute}/received/accept?id={requestId}party={TestEntities.PersonPaula.Id}",
    //            null,
    //            TestContext.Current.CancellationToken);

    //        Assert.NotEqual(HttpStatusCode.NotFound, acceptResponse.StatusCode);
    //        Assert.Equal(HttpStatusCode.InternalServerError, acceptResponse.StatusCode);
    //    }
    //}

    //#endregion
}
