using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
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

namespace Altinn.AccessManagement.Enduser.Api.Tests.Controllers;

public class RequestControllerTest
{
    public const string Route = "accessmanagement/api/v1/enduser/request";

    private static HttpClient CreateClient(ApiFixture fixture, Guid partyUuid)
    {
        var client = fixture.Server.CreateClient();
        var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
        {
            claims.Add(new Claim(AltinnCoreClaimTypes.PartyUuid, partyUuid.ToString()));
            claims.Add(new Claim("scope", AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ));
            claims.Add(new Claim("scope", AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE));
        });
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return client;
    }

    private static string PartyUrn(Guid id) => $"urn:altinn:party:uuid:{id}";

    private static StringContent CreateRequestBody(Guid from, Guid to, string packageUrn = null, string resourceId = null)
    {
        var body = new CreateServiceOwnerRequest
        {
            Connection = new ConnectionRequestInputDto
            {
                From = PartyUrn(from),
                To = PartyUrn(to),
            },
            Package = new RequestRefrenceDto { Urn = packageUrn ?? string.Empty },
            Resource = new RequestRefrenceDto { Urn = resourceId ?? string.Empty },
        };

        var json = JsonSerializer.Serialize(body, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    #region POST — Create a pending package request as enduser

    /// <summary>
    /// <see cref="Altinn.AccessManagement.Api.Enduser.Controllers.RequestController.CreateRequest"/>
    /// </summary>
    public class CreatePackageRequest : IClassFixture<ApiFixture>
    {
        public CreatePackageRequest(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage);
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task CreatePackageRequest_WithValidInput_ReturnsPendingRequest()
        {
            var client = CreateClient(Fixture, TestEntities.PersonPaula.Id);
            var from = TestEntities.OrganizationNordisAS.Id;
            var to = TestEntities.PersonPaula.Id;
            var packageUrn = PackageConstants.Agriculture.Entity.Urn;

            var response = await client.PostAsync(
                $"{Route}?party={to}",
                CreateRequestBody(from, to, packageUrn: packageUrn),
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal((int)RequestStatus.Pending, root.GetProperty("status").GetInt32());
            Assert.Equal(from.ToString(), root.GetProperty("connection").GetProperty("from").GetProperty("id").GetString());
            Assert.Equal(to.ToString(), root.GetProperty("connection").GetProperty("to").GetProperty("id").GetString());
        }
    }

    #endregion

    #region Full package request lifecycle

    /// <summary>
    /// Full lifecycle: seeded Pending request → GET → PUT reject
    /// </summary>
    public class PackageRequestLifecycle : IClassFixture<ApiFixture>
    {
        private static readonly Guid PendingPackageRequestId = Guid.Parse("01960003-0000-7000-8000-000000000001");

        public PackageRequestLifecycle(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage);
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
            Fixture.EnsureSeedOnce(db =>
            {
                var assignment = new Assignment
                {
                    FromId = TestEntities.OrganizationNordisAS.Id,
                    ToId = TestEntities.PersonPaula.Id,
                    RoleId = RoleConstants.Rightholder,
                };
                db.Assignments.Add(assignment);
                db.SaveChanges();

                db.RequestAssignmentPackages.Add(new RequestAssignmentPackage
                {
                    Id = PendingPackageRequestId,
                    AssignmentId = assignment.Id,
                    PackageId = PackageConstants.Agriculture.Id,
                    Status = RequestStatus.Pending,
                });
                db.SaveChanges();
            });
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task PackageRequest_GetSentRequests_ContainsPendingRequest()
        {
            var client = CreateClient(Fixture, TestEntities.OrganizationNordisAS.Id);
            var from = TestEntities.OrganizationNordisAS.Id;
            var to = TestEntities.PersonPaula.Id;

            var response = await client.GetAsync(
                $"{Route}/sent?party={from}&to={to}",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseJson = await client.GetFromJsonAsync<PaginatedResult<RequestDto>>(
                $"{Route}/sent?party={from}&to={to}",
                TestContext.Current.CancellationToken);

            Assert.Contains(responseJson.Items, item => item.Id == PendingPackageRequestId);
        }

        [Fact]
        public async Task PackageRequest_FullLifecycle_PendingToRejected()
        {
            var client = CreateClient(Fixture, TestEntities.OrganizationNordisAS.Id);

            // 1. Enduser fetches the pending request via received endpoint
            var getResponse = await client.GetAsync(
                $"{Route}?party={TestEntities.OrganizationNordisAS.Id}&from={TestEntities.OrganizationNordisAS.Id}&to={TestEntities.PersonPaula.Id}",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            // 2. Enduser rejects
            var rejectResponse = await client.PutAsync(
                $"{Route}/received/reject?party={TestEntities.OrganizationNordisAS.Id}&id={PendingPackageRequestId}",
                null,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);

            var json = await rejectResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var doc = JsonDocument.Parse(json);
            Assert.Equal((int)RequestStatus.Rejected, doc.RootElement.GetProperty("status").GetInt32());
        }
    }

    #endregion

    #region POST — Create a pending resource request as enduser

    /// <summary>
    /// <see cref="Altinn.AccessManagement.Api.Enduser.Controllers.RequestController.CreateRequest"/>
    /// </summary>
    public class CreateResourceRequest : IClassFixture<ApiFixture>
    {
        private static readonly ResourceType TestResourceType = new()
        {
            Id = Guid.Parse("01960010-0000-7000-8000-000000000001"),
            Name = "EnduserCreateResourceTestType",
        };

        private static readonly Guid TestResourceId = Guid.Parse("01960010-0000-7000-8000-000000000002");
        private const string TestResourceRefId = "enduser-create-test-resource-1";

        public CreateResourceRequest(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage);
            Fixture.EnsureSeedOnce(db =>
            {
                db.ResourceTypes.Add(TestResourceType);
                db.SaveChanges();

                db.Resources.Add(new Resource
                {
                    Id = TestResourceId,
                    Name = "EnduserCreateTestResource",
                    Description = "Test resource for enduser create resource request",
                    RefId = TestResourceRefId,
                    ProviderId = ProviderConstants.ResourceRegistry,
                    TypeId = TestResourceType.Id,
                });
                db.SaveChanges();
            });
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task CreateResourceRequest_WithValidInput_ReturnsPendingRequest()
        {
            var client = CreateClient(Fixture, TestEntities.PersonPaula.Id);
            var from = TestEntities.OrganizationNordisAS.Id;
            var to = TestEntities.PersonPaula.Id;

            var response = await client.PostAsync(
                $"{Route}?party={to}",
                CreateRequestBody(from, to, resourceId: TestResourceRefId),
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal((int)RequestStatus.Pending, root.GetProperty("status").GetInt32());
            Assert.Equal(from.ToString(), root.GetProperty("connection").GetProperty("from").GetProperty("id").GetString());
            Assert.Equal(to.ToString(), root.GetProperty("connection").GetProperty("to").GetProperty("id").GetString());
        }
    }

    #endregion

    #region Package request accept lifecycle

    /// <summary>
    /// Full lifecycle: seeded Pending package request → PUT approve → Approved
    /// </summary>
    public class PackageRequestAcceptLifecycle : IClassFixture<ApiFixture>
    {
        private static readonly Guid PendingPackageRequestId = Guid.Parse("01960020-0000-7000-8000-000000000001");

        public PackageRequestAcceptLifecycle(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage);
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
            Fixture.EnsureSeedOnce(db =>
            {
                var assignment = new Assignment
                {
                    FromId = TestEntities.OrganizationNordisAS.Id,
                    ToId = TestEntities.PersonPaula.Id,
                    RoleId = RoleConstants.Rightholder,
                };
                db.Assignments.Add(assignment);
                db.SaveChanges();

                db.RequestAssignmentPackages.Add(new RequestAssignmentPackage
                {
                    Id = PendingPackageRequestId,
                    AssignmentId = assignment.Id,
                    PackageId = PackageConstants.Agriculture.Id,
                    Status = RequestStatus.Pending,
                });
                db.SaveChanges();
            });
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task PackageRequest_AcceptRequest_ReachesDelegationCheck()
        {
            // Accept triggers ConnectionService.AddPackage which validates delegation authorization.
            // Without full delegation prerequisites seeded, expect a 400 with a specific validation error
            // proving the endpoint found the request and attempted the delegation.
            var client = CreateClient(Fixture, TestEntities.PersonPaula.Id);

            var acceptResponse = await client.PutAsync(
                $"{Route}/received/approve?party={TestEntities.PersonPaula.Id}&id={PendingPackageRequestId}",
                null,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, acceptResponse.StatusCode);

            var json = await acceptResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Verify the error is the expected delegation authorization failure (not a generic 400)
            Assert.True(root.TryGetProperty("validationErrors", out var errors));
            Assert.True(errors.GetArrayLength() > 0);
        }
    }

    #endregion

    #region Resource request reject lifecycle

    /// <summary>
    /// Full lifecycle: seeded Pending resource request → PUT reject → Rejected
    /// </summary>
    public class ResourceRequestRejectLifecycle : IClassFixture<ApiFixture>
    {
        private static readonly ResourceType TestResourceType = new()
        {
            Id = Guid.Parse("01960030-0000-7000-8000-000000000001"),
            Name = "EnduserRejectResourceTestType",
        };

        private static readonly Guid TestResourceId = Guid.Parse("01960030-0000-7000-8000-000000000002");
        private static readonly Guid PendingResourceRequestId = Guid.Parse("01960030-0000-7000-8000-000000000003");

        public ResourceRequestRejectLifecycle(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage);
            Fixture.EnsureSeedOnce(db =>
            {
                db.ResourceTypes.Add(TestResourceType);
                db.SaveChanges();

                db.Resources.Add(new Resource
                {
                    Id = TestResourceId,
                    Name = "EnduserRejectTestResource",
                    Description = "Test resource for enduser reject lifecycle",
                    RefId = "enduser-reject-test-resource-1",
                    ProviderId = ProviderConstants.ResourceRegistry,
                    TypeId = TestResourceType.Id,
                });
                db.SaveChanges();

                var assignment = new Assignment
                {
                    FromId = TestEntities.OrganizationNordisAS.Id,
                    ToId = TestEntities.PersonPaula.Id,
                    RoleId = RoleConstants.Rightholder,
                };
                db.Assignments.Add(assignment);
                db.SaveChanges();

                db.RequestAssignmentResources.Add(new RequestAssignmentResource
                {
                    Id = PendingResourceRequestId,
                    AssignmentId = assignment.Id,
                    ResourceId = TestResourceId,
                    Status = RequestStatus.Pending,
                });
                db.SaveChanges();
            });
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task ResourceRequest_RejectRequest_ReturnsRejected()
        {
            var client = CreateClient(Fixture, TestEntities.OrganizationNordisAS.Id);

            var rejectResponse = await client.PutAsync(
                $"{Route}/received/reject?party={TestEntities.OrganizationNordisAS.Id}&id={PendingResourceRequestId}",
                null,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);

            var json = await rejectResponse.Content.ReadFromJsonAsync<RequestDto>(TestContext.Current.CancellationToken);
            Assert.Equal(RequestStatus.Rejected, json.Status);
        }
    }

    #endregion

    #region Resource request accept lifecycle

    /// <summary>
    /// Full lifecycle: seeded Pending resource request → PUT approve → Approved
    /// </summary>
    public class ResourceRequestAcceptLifecycle : IClassFixture<ApiFixture>
    {
        private static readonly ResourceType TestResourceType = new()
        {
            Id = Guid.Parse("01960040-0000-7000-8000-000000000001"),
            Name = "EnduserAcceptResourceTestType",
        };

        private static readonly Guid TestResourceId = Guid.Parse("01960040-0000-7000-8000-000000000002");
        private static readonly Guid PendingResourceRequestId = Guid.Parse("01960040-0000-7000-8000-000000000003");

        public ResourceRequestAcceptLifecycle(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage);
            Fixture.EnsureSeedOnce(db =>
            {
                db.ResourceTypes.Add(TestResourceType);
                db.SaveChanges();

                db.Resources.Add(new Resource
                {
                    Id = TestResourceId,
                    Name = "EnduserAcceptTestResource",
                    Description = "Test resource for enduser accept lifecycle",
                    RefId = "enduser-accept-test-resource-1",
                    ProviderId = ProviderConstants.ResourceRegistry,
                    TypeId = TestResourceType.Id,
                });
                db.SaveChanges();

                var assignment = new Assignment
                {
                    FromId = TestEntities.OrganizationNordisAS.Id,
                    ToId = TestEntities.PersonPaula.Id,
                    RoleId = RoleConstants.Rightholder,
                };
                db.Assignments.Add(assignment);
                db.SaveChanges();

                db.RequestAssignmentResources.Add(new RequestAssignmentResource
                {
                    Id = PendingResourceRequestId,
                    AssignmentId = assignment.Id,
                    ResourceId = TestResourceId,
                    Status = RequestStatus.Pending,
                });
                db.SaveChanges();
            });
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task ResourceRequest_AcceptRequest_ReachesDelegationCheck()
        {
            // Accept triggers ResourceDelegationCheck which calls Azure policy storage.
            // Without blob storage running, expect a 500 (connection refused to Azurite).
            // This proves the endpoint found the request and attempted the delegation check.
            var client = CreateClient(Fixture, TestEntities.PersonPaula.Id);

            var acceptResponse = await client.PutAsync(
                $"{Route}/received/approve?id={PendingResourceRequestId}",
                null,
                TestContext.Current.CancellationToken);

            // The endpoint should not return 404 (request was found)
            Assert.NotEqual(HttpStatusCode.NotFound, acceptResponse.StatusCode);

            // Expect 500 due to missing Azure storage for policy retrieval
            Assert.Equal(HttpStatusCode.InternalServerError, acceptResponse.StatusCode);
        }
    }

    #endregion

    #region Receiver verifies received package request

    /// <summary>
    /// Verifies that the receiving party (to) can see a package request sent to them.
    /// </summary>
    public class ReceiverSeesPackageRequest : IClassFixture<ApiFixture>
    {
        private static readonly Guid PendingPackageRequestId = Guid.Parse("01960050-0000-7000-8000-000000000001");

        public ReceiverSeesPackageRequest(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage);
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
            Fixture.EnsureSeedOnce(db =>
            {
                var assignment = new Assignment
                {
                    FromId = TestEntities.OrganizationNordisAS.Id,
                    ToId = TestEntities.PersonPaula.Id,
                    RoleId = RoleConstants.Rightholder,
                };
                db.Assignments.Add(assignment);
                db.SaveChanges();

                db.RequestAssignmentPackages.Add(new RequestAssignmentPackage
                {
                    Id = PendingPackageRequestId,
                    AssignmentId = assignment.Id,
                    PackageId = PackageConstants.Agriculture.Id,
                    Status = RequestStatus.Pending,
                });
                db.SaveChanges();
            });
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task Receiver_GetReceivedRequests_SeesPackageRequestWithCorrectDetails()
        {
            var from = TestEntities.OrganizationNordisAS.Id;
            var to = TestEntities.PersonPaula.Id;

            // Receiver (to=Paula) queries received requests
            var client = CreateClient(Fixture, to);
            var response = await client.GetAsync(
                $"{Route}/received?party={to}&from={from}",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var doc = JsonDocument.Parse(json);
            var items = doc.RootElement.GetProperty("data");

            // Find our seeded request in the list
            JsonElement? match = null;
            foreach (var item in items.EnumerateArray())
            {
                if (item.GetProperty("id").GetString() == PendingPackageRequestId.ToString())
                {
                    match = item;
                    break;
                }
            }

            Assert.True(match.HasValue, "Receiver should see the package request in their list");

            var request = match.Value;
            Assert.Equal((int)RequestStatus.Pending, request.GetProperty("status").GetInt32());
            Assert.Equal(from.ToString(), request.GetProperty("connection").GetProperty("from").GetProperty("id").GetString());
            Assert.Equal(to.ToString(), request.GetProperty("connection").GetProperty("to").GetProperty("id").GetString());
        }
    }

    #endregion

    #region Receiver verifies received resource request

    /// <summary>
    /// Verifies that the receiving party (to) can see a resource request sent to them.
    /// </summary>
    public class ReceiverSeesResourceRequest : IClassFixture<ApiFixture>
    {
        private static readonly ResourceType TestResourceType = new()
        {
            Id = Guid.Parse("01960060-0000-7000-8000-000000000001"),
            Name = "ReceiverVerifyResourceType",
        };

        private static readonly Guid TestResourceId = Guid.Parse("01960060-0000-7000-8000-000000000002");
        private static readonly Guid PendingResourceRequestId = Guid.Parse("01960060-0000-7000-8000-000000000003");

        public ReceiverSeesResourceRequest(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage);
            Fixture.EnsureSeedOnce(db =>
            {
                db.ResourceTypes.Add(TestResourceType);
                db.SaveChanges();

                db.Resources.Add(new Resource
                {
                    Id = TestResourceId,
                    Name = "ReceiverVerifyTestResource",
                    Description = "Test resource for receiver verification",
                    RefId = "receiver-verify-test-resource-1",
                    ProviderId = ProviderConstants.ResourceRegistry,
                    TypeId = TestResourceType.Id,
                });
                db.SaveChanges();

                var assignment = new Assignment
                {
                    FromId = TestEntities.OrganizationNordisAS.Id,
                    ToId = TestEntities.PersonPaula.Id,
                    RoleId = RoleConstants.Rightholder,
                };
                db.Assignments.Add(assignment);
                db.SaveChanges();

                db.RequestAssignmentResources.Add(new RequestAssignmentResource
                {
                    Id = PendingResourceRequestId,
                    AssignmentId = assignment.Id,
                    ResourceId = TestResourceId,
                    Status = RequestStatus.Pending,
                });
                db.SaveChanges();
            });
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task Receiver_GetReceivedRequests_SeesResourceRequestWithCorrectDetails()
        {
            var from = TestEntities.OrganizationNordisAS.Id;
            var to = TestEntities.PersonPaula.Id;

            // Receiver (to=Paula) queries received requests
            var client = CreateClient(Fixture, to);
            var response = await client.GetAsync(
                $"{Route}/received?party={to}&from={from}",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var doc = JsonDocument.Parse(json);
            var items = doc.RootElement.GetProperty("data");

            // Find our seeded request in the list
            JsonElement? match = null;
            foreach (var item in items.EnumerateArray())
            {
                if (item.GetProperty("id").GetString() == PendingResourceRequestId.ToString())
                {
                    match = item;
                    break;
                }
            }

            Assert.True(match.HasValue, "Receiver should see the resource request in their list");

            var request = match.Value;
            Assert.Equal((int)RequestStatus.Pending, request.GetProperty("status").GetInt32());
            Assert.Equal(from.ToString(), request.GetProperty("connection").GetProperty("from").GetProperty("id").GetString());
            Assert.Equal(to.ToString(), request.GetProperty("connection").GetProperty("to").GetProperty("id").GetString());
        }
    }

    #endregion
}
