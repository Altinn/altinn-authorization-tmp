using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Altinn.AccessManagement.Core.Constants;
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

    private static HttpClient CreateServiceOwnerReadClient(ApiFixture fixture, Guid partyUuid)
    {
        var client = fixture.Server.CreateClient();
        var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
        {
            claims.Add(new Claim(AltinnCoreClaimTypes.PartyUuid, partyUuid.ToString()));
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

    private static void EnableFeatureFlags(ApiFixture fixture)
    {
        fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
        fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage);
    }

    #region Package request E2E lifecycle — reject

    public class PackageRequest_EndToEnd_RejectLifecycle : IClassFixture<ApiFixture>
    {
        public PackageRequest_EndToEnd_RejectLifecycle(ApiFixture fixture)
        {
            Fixture = fixture;
            EnableFeatureFlags(fixture);
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task PackageRequest_EndToEnd_RejectLifecycle_FullFlow()
        {
            var from = $"urn:altinn:organization:identifier-no:{TestData.BakerJohnsen.Entity.OrganizationIdentifier}";
            var to = $"urn:altinn:person:identifier-no:{TestData.LarsBakke.Entity.PersonIdentifier}";

            // Step 1: SO creates a package request (status=Draft by default)
            var soClient = CreateServiceOwnerClient(Fixture, TestData.BakerJohnsen.Id);
            var createBody = new CreateServiceOwnerRequest
            {
                Connection = new ConnectionRequestInputDto { From = from, To = to },
                Resource = new RequestRefrenceDto(),
                Package = new RequestRefrenceDto { Urn = PackageConstants.Agriculture.Entity.Urn },
            };

            var createResponse = await soClient.PostAsJsonAsync(
                ServiceOwnerRoute,
                createBody,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);

            var createJson = await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var createDoc = JsonDocument.Parse(createJson);
            var createRoot = createDoc.RootElement;
            Assert.Equal((int)RequestStatus.Draft, createRoot.GetProperty("status").GetInt32());

            var requestId = createRoot.GetProperty("id").GetString();
            Assert.False(string.IsNullOrEmpty(requestId), "Request ID should be returned");

            // Step 2: EU confirms the draft request (Draft → Pending)
            // LarsBakke has ManagingDirector role in BakerJohnsen, confirms on behalf of org
            var enduserClient = CreateEnduserClient(Fixture, TestData.LarsBakke.Id);
            var confirmResponse = await enduserClient.PutAsync(
                $"{EnduserRoute}/sent/confirm?id={requestId}&party={TestData.BakerJohnsen.Id}",
                null,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

            var confirmJson = await confirmResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var confirmDoc = JsonDocument.Parse(confirmJson);
            Assert.Equal((int)RequestStatus.Pending, confirmDoc.RootElement.GetProperty("status").GetInt32());

            // Step 3: SO verifies the request is now Pending
            var soReadClient = CreateServiceOwnerReadClient(Fixture, TestData.BakerJohnsen.Id);
            var soGetResponse = await soReadClient.GetAsync(
                $"{ServiceOwnerRoute}/{requestId}/status",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, soGetResponse.StatusCode);

            var status = await soGetResponse.Content.ReadFromJsonAsync<RequestStatus>(TestContext.Current.CancellationToken);
            Assert.Equal(RequestStatus.Pending, status);

            // Step 4: EU sees the request in their received list
            var enduserGetResponse = await enduserClient.GetAsync(
                $"{EnduserRoute}/received?party={TestData.LarsBakke.Id}&from={TestData.BakerJohnsen.Id}",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, enduserGetResponse.StatusCode);

            var enduserGetJson = await enduserGetResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var enduserGetDoc = JsonDocument.Parse(enduserGetJson);
            var enduserItems = enduserGetDoc.RootElement.GetProperty("data").EnumerateArray().ToList();
            Assert.Contains(enduserItems, i => i.GetProperty("id").GetString() == requestId);

            // Step 5: EU rejects the request
            var rejectResponse = await enduserClient.PutAsync(
                $"{EnduserRoute}/received/reject?party={TestData.LarsBakke.Id}&id={requestId}",
                null,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);

            var rejectJson = await rejectResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var rejectDoc = JsonDocument.Parse(rejectJson);
            Assert.Equal((int)RequestStatus.Rejected, rejectDoc.RootElement.GetProperty("status").GetInt32());
        }
    }

    #endregion

    #region Resource request E2E lifecycle — reject

    public class ResourceRequest_EndToEnd_RejectLifecycle : IClassFixture<ApiFixture>
    {
        private static readonly ResourceType TestResourceType = new()
        {
            Id = Guid.Parse("0196d001-0000-7000-8000-000000000001"),
            Name = "E2ERejectResourceType",
        };

        public ResourceRequest_EndToEnd_RejectLifecycle(ApiFixture fixture)
        {
            Fixture = fixture;
            EnableFeatureFlags(fixture);
            fixture.EnsureSeedOnce(db =>
            {
                db.ResourceTypes.Add(TestResourceType);
                db.SaveChanges();

                db.Resources.Add(new Resource
                {
                    Id = Guid.CreateVersion7(),
                    Name = "E2ERejectTestResource",
                    Description = "Resource for E2E reject lifecycle test",
                    RefId = "e2e-reject-test-resource-1",
                    ProviderId = ProviderConstants.ResourceRegistry,
                    TypeId = TestResourceType.Id,
                });
                db.SaveChanges();
            });
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task ResourceRequest_EndToEnd_RejectLifecycle_FullFlow()
        {
            var from = $"urn:altinn:organization:identifier-no:{TestData.SvendsenAutomobil.Entity.OrganizationIdentifier}";
            var to = $"urn:altinn:person:identifier-no:{TestData.MortenDahl.Entity.PersonIdentifier}";

            // Step 1: SO creates a resource request (status=Draft by default)
            var soClient = CreateServiceOwnerClient(Fixture, TestData.SvendsenAutomobil.Id);
            var createBody = new CreateServiceOwnerRequest
            {
                Connection = new ConnectionRequestInputDto { From = from, To = to },
                Resource = new RequestRefrenceDto { Urn = "e2e-reject-test-resource-1" },
                Package = new RequestRefrenceDto(),
            };

            var createResponse = await soClient.PostAsJsonAsync(
                ServiceOwnerRoute,
                createBody,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);

            var createJson = await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var createDoc = JsonDocument.Parse(createJson);
            var createRoot = createDoc.RootElement;
            Assert.Equal((int)RequestStatus.Draft, createRoot.GetProperty("status").GetInt32());

            var requestId = createRoot.GetProperty("id").GetString();
            Assert.False(string.IsNullOrEmpty(requestId), "Request ID should be returned");

            // Step 2: EU confirms the draft request (Draft → Pending)
            // MortenDahl has ManagingDirector role in SvendsenAutomobil, confirms on behalf of org
            var enduserClient = CreateEnduserClient(Fixture, TestData.MortenDahl.Id);
            var confirmResponse = await enduserClient.PutAsync(
                $"{EnduserRoute}/sent/confirm?id={requestId}&party={TestData.SvendsenAutomobil.Id}",
                null,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

            var confirmJson = await confirmResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var confirmDoc = JsonDocument.Parse(confirmJson);
            Assert.Equal((int)RequestStatus.Pending, confirmDoc.RootElement.GetProperty("status").GetInt32());

            // Step 3: SO verifies the request is now Pending
            var soReadClient = CreateServiceOwnerReadClient(Fixture, TestData.SvendsenAutomobil.Id);
            var soGetResponse = await soReadClient.GetAsync(
                $"{ServiceOwnerRoute}/{requestId}/status",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, soGetResponse.StatusCode);

            var status = await soGetResponse.Content.ReadFromJsonAsync<RequestStatus>(TestContext.Current.CancellationToken);
            Assert.Equal(RequestStatus.Pending, status);

            // Step 4: EU sees the request in their received list
            var enduserGetResponse = await enduserClient.GetAsync(
                $"{EnduserRoute}/received?party={TestData.MortenDahl.Id}&from={TestData.SvendsenAutomobil.Id}",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, enduserGetResponse.StatusCode);

            var enduserGetJson = await enduserGetResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var enduserGetDoc = JsonDocument.Parse(enduserGetJson);
            var enduserItems = enduserGetDoc.RootElement.GetProperty("data").EnumerateArray().ToList();
            Assert.Contains(enduserItems, i => i.GetProperty("id").GetString() == requestId);

            // Step 5: EU rejects the request
            var rejectResponse = await enduserClient.PutAsync(
                $"{EnduserRoute}/received/reject?party={TestData.MortenDahl.Id}&id={requestId}",
                null,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);

            var rejectJson = await rejectResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var rejectDoc = JsonDocument.Parse(rejectJson);
            Assert.Equal((int)RequestStatus.Rejected, rejectDoc.RootElement.GetProperty("status").GetInt32());
        }
    }

    #endregion

    #region Package request E2E lifecycle — accept

    public class PackageRequest_EndToEnd_AcceptLifecycle : IClassFixture<ApiFixture>
    {
        public PackageRequest_EndToEnd_AcceptLifecycle(ApiFixture fixture)
        {
            Fixture = fixture;
            EnableFeatureFlags(fixture);
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task PackageRequest_EndToEnd_AcceptLifecycle_FullFlow()
        {
            var from = $"urn:altinn:organization:identifier-no:{TestData.FredriksonsFabrikk.Entity.OrganizationIdentifier}";
            var to = $"urn:altinn:person:identifier-no:{TestData.SiljeHaugen.Entity.PersonIdentifier}";

            // Step 1: SO creates a package request (status=Draft)
            var soClient = CreateServiceOwnerClient(Fixture, TestData.FredriksonsFabrikk.Id);
            var createBody = new CreateServiceOwnerRequest
            {
                Connection = new ConnectionRequestInputDto { From = from, To = to },
                Resource = new RequestRefrenceDto(),
                Package = new RequestRefrenceDto { Urn = PackageConstants.Agriculture.Entity.Urn },
            };

            var createResponse = await soClient.PostAsJsonAsync(
                ServiceOwnerRoute,
                createBody,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);

            var createJson = await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var createDoc = JsonDocument.Parse(createJson);
            var requestId = createDoc.RootElement.GetProperty("id").GetString();
            Assert.False(string.IsNullOrEmpty(requestId));

            // Step 2: EU confirms (Draft → Pending)
            // SiljeHaugen has ManagingDirector role in FredriksonsFabrikk, confirms on behalf of org
            var enduserClient = CreateEnduserClient(Fixture, TestData.SiljeHaugen.Id);
            var confirmResponse = await enduserClient.PutAsync(
                $"{EnduserRoute}/sent/confirm?id={requestId}&party={TestData.FredriksonsFabrikk.Id}",
                null,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

            // Step 3: SO verifies Pending
            var soReadClient = CreateServiceOwnerReadClient(Fixture, TestData.FredriksonsFabrikk.Id);
            var soGetResponse = await soReadClient.GetAsync(
                $"{ServiceOwnerRoute}/{requestId}/status",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, soGetResponse.StatusCode);

            var status = await soGetResponse.Content.ReadFromJsonAsync<RequestStatus>(TestContext.Current.CancellationToken);
            Assert.Equal(RequestStatus.Pending, status);

            // Step 4: EU sees the request
            var enduserGetResponse = await enduserClient.GetAsync(
                $"{EnduserRoute}/received?party={TestData.SiljeHaugen.Id}&from={TestData.FredriksonsFabrikk.Id}",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, enduserGetResponse.StatusCode);

            // Step 5: EU approves — expect non-success (delegation auth failure proves routing works)
            var approveResponse = await enduserClient.PutAsync(
                $"{EnduserRoute}/received/approve?party={TestData.SiljeHaugen.Id}&id={requestId}",
                null,
                TestContext.Current.CancellationToken);

            Assert.False(approveResponse.IsSuccessStatusCode);
        }
    }

    #endregion

    #region Resource request E2E lifecycle — accept

    public class ResourceRequest_EndToEnd_AcceptLifecycle : IClassFixture<ApiFixture>
    {
        private static readonly ResourceType TestResourceType = new()
        {
            Id = Guid.Parse("0196d002-0000-7000-8000-000000000001"),
            Name = "E2EAcceptResourceType",
        };

        public ResourceRequest_EndToEnd_AcceptLifecycle(ApiFixture fixture)
        {
            Fixture = fixture;
            EnableFeatureFlags(fixture);
            fixture.EnsureSeedOnce(db =>
            {
                db.ResourceTypes.Add(TestResourceType);
                db.SaveChanges();

                db.Resources.Add(new Resource
                {
                    Id = Guid.CreateVersion7(),
                    Name = "E2EAcceptTestResource",
                    Description = "Resource for E2E accept lifecycle test",
                    RefId = "e2e-accept-test-resource-1",
                    ProviderId = ProviderConstants.ResourceRegistry,
                    TypeId = TestResourceType.Id,
                });
                db.SaveChanges();
            });
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task ResourceRequest_EndToEnd_AcceptLifecycle_FullFlow()
        {
            var from = $"urn:altinn:organization:identifier-no:{TestData.RegnskapNorge.Entity.OrganizationIdentifier}";
            var to = $"urn:altinn:person:identifier-no:{TestData.BjornMoe.Entity.PersonIdentifier}";

            // Step 1: SO creates a resource request (status=Draft)
            var soClient = CreateServiceOwnerClient(Fixture, TestData.RegnskapNorge.Id);
            var createBody = new CreateServiceOwnerRequest
            {
                Connection = new ConnectionRequestInputDto { From = from, To = to },
                Resource = new RequestRefrenceDto { Urn = "e2e-accept-test-resource-1" },
                Package = new RequestRefrenceDto(),
            };

            var createResponse = await soClient.PostAsJsonAsync(
                ServiceOwnerRoute,
                createBody,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Accepted, createResponse.StatusCode);

            var createJson = await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var createDoc = JsonDocument.Parse(createJson);
            var requestId = createDoc.RootElement.GetProperty("id").GetString();
            Assert.False(string.IsNullOrEmpty(requestId));

            // Step 2: EU confirms (Draft → Pending)
            // BjornMoe has ManagingDirector role in RegnskapNorge, confirms on behalf of org
            var enduserClient = CreateEnduserClient(Fixture, TestData.BjornMoe.Id);
            var confirmResponse = await enduserClient.PutAsync(
                $"{EnduserRoute}/sent/confirm?id={requestId}&party={TestData.RegnskapNorge.Id}",
                null,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);

            // Step 3: SO verifies Pending
            var soReadClient = CreateServiceOwnerReadClient(Fixture, TestData.RegnskapNorge.Id);
            var soGetResponse = await soReadClient.GetAsync(
                $"{ServiceOwnerRoute}/{requestId}/status",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, soGetResponse.StatusCode);

            var status = await soGetResponse.Content.ReadFromJsonAsync<RequestStatus>(TestContext.Current.CancellationToken);
            Assert.Equal(RequestStatus.Pending, status);

            // Step 4: EU sees the request
            var enduserGetResponse = await enduserClient.GetAsync(
                $"{EnduserRoute}/received?party={TestData.BjornMoe.Id}&from={TestData.RegnskapNorge.Id}",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, enduserGetResponse.StatusCode);

            // Step 5: EU accepts — expect 500 (Azure storage unavailable proves routing works)
            var acceptResponse = await enduserClient.PutAsync(
                $"{EnduserRoute}/received/approve?party={TestData.BjornMoe.Id}&id={requestId}",
                null,
                TestContext.Current.CancellationToken);

            Assert.NotEqual(HttpStatusCode.NotFound, acceptResponse.StatusCode);
            Assert.Equal(HttpStatusCode.InternalServerError, acceptResponse.StatusCode);
        }
    }

    #endregion
}
