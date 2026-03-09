using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.TestUtils;
using Altinn.AccessManagement.TestUtils.Data;
using Altinn.AccessManagement.TestUtils.Fixtures;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;

namespace Altinn.AccessManagement.ServiceOwner.Api.Tests.Controllers;

public class RequestControllerTest
{
    public const string Route = "accessmanagement/api/v1/serviceowner/delegationrequests";

    private static HttpClient CreateClient(ApiFixture fixture, Guid partyUuid)
    {
        var client = fixture.Server.CreateClient();
        var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
        {
            claims.Add(new Claim(AltinnCoreClaimTypes.PartyUuid, partyUuid.ToString()));
            claims.Add(new Claim("scope", $"{AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_WRITE}"));
        });
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return client;
    }

    private static HttpClient CreateReadOnlyClient(ApiFixture fixture)
    {
        var client = fixture.Server.CreateClient();
        var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
        {
            claims.Add(new Claim(AltinnCoreClaimTypes.PartyUuid, TestEntities.OrganizationNordisAS.Id.ToString()));
            claims.Add(new Claim("scope", AuthzConstants.SCOPE_PORTAL_SERVICEOWNER));
            claims.Add(new Claim("scope", AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_READ));
        });
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return client;
    }

    private static HttpClient CreateUnauthenticatedClient(ApiFixture fixture)
    {
        return fixture.Server.CreateClient();
    }

    #region GET _meta/urns/party

    public class GetValidUrnsTest : IClassFixture<ApiFixture>
    {
        private readonly ApiFixture _fixture;

        public GetValidUrnsTest(ApiFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetValidUrns_ReturnsOk_WithExpectedUrnPrefixes()
        {
            var client = _fixture.Server.CreateClient();

            var response = await client.GetAsync(
                $"{Route}/_meta/urns/party",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var doc = JsonDocument.Parse(json);
            var urns = doc.RootElement.EnumerateArray().Select(e => e.GetString()).ToList();

            Assert.Contains("urn:altinn:person:identifier-no", urns);
            Assert.Contains("urn:altinn:organization:identifier-no", urns);
            Assert.Contains("urn:altinn:systemuser:uuid", urns);
            Assert.Contains("urn:altinn:party:uuid", urns);
        }

        [Fact]
        public async Task GetValidUrns_ReturnsExactlyFourUrns()
        {
            var client = _fixture.Server.CreateClient();

            var response = await client.GetAsync(
                $"{Route}/_meta/urns/party",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var doc = JsonDocument.Parse(json);
            var count = doc.RootElement.GetArrayLength();

            Assert.Equal(4, count);
        }
    }

    #endregion

    #region POST resource

    public class CreateResourceRequestTest : IClassFixture<ApiFixture>
    {
        private static readonly ResourceType TestResourceType = new()
        {
            Id = Guid.Parse("01960001-0000-0000-0000-000000000099"),
            Name = "ServiceOwnerTestResourceType",
        };

        private readonly ApiFixture _fixture;
        private readonly string _testResourceRefId = $"test-resource-{Guid.NewGuid():N}";

        public CreateResourceRequestTest(ApiFixture fixture)
        {
            _fixture = fixture;
            _fixture.EnsureSeedOnce(db =>
            {
                db.ResourceTypes.Add(TestResourceType);
                db.SaveChanges();

                db.Resources.Add(new Resource
                {
                    Id = Guid.CreateVersion7(),
                    Name = "TestResource",
                    Description = "Test resource for ServiceOwner API tests",
                    RefId = "test-resource-so-1",
                    ProviderId = ProviderConstants.ResourceRegistry,
                    TypeId = TestResourceType.Id,
                });
                db.SaveChanges();
            });
        }

        [Fact]
        public async Task CreateResourceRequest_WithValidInput_Returns202Accepted()
        {
            var client = CreateClient(_fixture, TestEntities.OrganizationNordisAS.Id);
            var from = $"urn:altinn:organization:identifier-no:{TestEntities.OrganizationNordisAS.Entity.OrganizationIdentifier}";
            var to = $"urn:altinn:person:identifier-no:{TestEntities.PersonPaula.Entity.PersonIdentifier}";

            var body = new CreateResourceRequestInput
            {
                Connection = new ConnectionRequestInputDto { From = from, To = to },
                Resource = new ResourceReferenceDto { ResourceId = "test-resource-so-1" }
            };

            var response = await client.PostAsJsonAsync(
                $"{Route}/resource",
                body,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal("resource", root.GetProperty("requestType").GetString());
            Assert.True(root.GetProperty("status").GetInt32() >= 0);
            Assert.Equal("test-resource-so-1", root.GetProperty("resource").GetProperty("resourceId").GetString());
            Assert.True(root.TryGetProperty("connection", out var conn));
            Assert.True(conn.TryGetProperty("from", out _));
            Assert.True(conn.TryGetProperty("to", out _));
            Assert.True(root.TryGetProperty("links", out var links));
            Assert.True(links.TryGetProperty("confirmLink", out _));
            Assert.True(links.TryGetProperty("statusLink", out _));
        }

        [Fact]
        public async Task CreateResourceRequest_WithInvalidFromUrn_Returns400()
        {
            var client = CreateClient(_fixture, TestEntities.OrganizationNordisAS.Id);

            var body = new CreateResourceRequestInput
            {
                Connection = new ConnectionRequestInputDto
                {
                    From = "urn:invalid:prefix:12345",
                    To = $"urn:altinn:person:identifier-no:{TestEntities.PersonPaula.Entity.PersonIdentifier}"
                },
                Resource = new ResourceReferenceDto { ResourceId = "test-resource-so-1" }
            };

            var response = await client.PostAsJsonAsync(
                $"{Route}/resource",
                body,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateResourceRequest_WithEmptyResourceId_Returns400()
        {
            var client = CreateClient(_fixture, TestEntities.OrganizationNordisAS.Id);
            var from = $"urn:altinn:organization:identifier-no:{TestEntities.OrganizationNordisAS.Entity.OrganizationIdentifier}";
            var to = $"urn:altinn:person:identifier-no:{TestEntities.PersonPaula.Entity.PersonIdentifier}";

            var body = new CreateResourceRequestInput
            {
                Connection = new ConnectionRequestInputDto { From = from, To = to },
                Resource = new ResourceReferenceDto { ResourceId = string.Empty }
            };

            var response = await client.PostAsJsonAsync(
                $"{Route}/resource",
                body,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    #endregion

    #region POST package

    public class CreatePackageRequestTest : IClassFixture<ApiFixture>
    {
        private readonly ApiFixture _fixture;

        public CreatePackageRequestTest(ApiFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CreatePackageRequest_WithValidInput_Returns202Accepted()
        {
            var client = CreateClient(_fixture, TestEntities.OrganizationNordisAS.Id);
            var from = $"urn:altinn:organization:identifier-no:{TestEntities.OrganizationNordisAS.Entity.OrganizationIdentifier}";
            var to = $"urn:altinn:person:identifier-no:{TestEntities.PersonPaula.Entity.PersonIdentifier}";

            var body = new CreatePackageRequestInput
            {
                Connection = new ConnectionRequestInputDto { From = from, To = to },
                Package = new PackageReferenceDto { Urn = PackageConstants.Agriculture.Entity.Urn }
            };

            var response = await client.PostAsJsonAsync(
                $"{Route}/package",
                body,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal("package", root.GetProperty("requestType").GetString());
            Assert.True(root.GetProperty("status").GetInt32() >= 0);
            Assert.Equal(PackageConstants.Agriculture.Entity.Urn, root.GetProperty("package").GetProperty("urn").GetString());
            Assert.True(root.TryGetProperty("connection", out var conn));
            Assert.True(conn.TryGetProperty("from", out _));
            Assert.True(conn.TryGetProperty("to", out _));
            Assert.True(root.TryGetProperty("links", out var links));
            Assert.True(links.TryGetProperty("confirmLink", out _));
            Assert.True(links.TryGetProperty("statusLink", out _));
        }

        [Fact]
        public async Task CreatePackageRequest_WithInvalidFromUrn_Returns400()
        {
            var client = CreateClient(_fixture, TestEntities.OrganizationNordisAS.Id);

            var body = new CreatePackageRequestInput
            {
                Connection = new ConnectionRequestInputDto
                {
                    From = "urn:invalid:prefix:12345",
                    To = $"urn:altinn:person:identifier-no:{TestEntities.PersonPaula.Entity.PersonIdentifier}"
                },
                Package = new PackageReferenceDto { Urn = PackageConstants.Agriculture.Entity.Urn }
            };

            var response = await client.PostAsJsonAsync(
                $"{Route}/package",
                body,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreatePackageRequest_WithEmptyPackageUrn_Returns400()
        {
            var client = CreateClient(_fixture, TestEntities.OrganizationNordisAS.Id);
            var from = $"urn:altinn:organization:identifier-no:{TestEntities.OrganizationNordisAS.Entity.OrganizationIdentifier}";
            var to = $"urn:altinn:person:identifier-no:{TestEntities.PersonPaula.Entity.PersonIdentifier}";

            var body = new CreatePackageRequestInput
            {
                Connection = new ConnectionRequestInputDto { From = from, To = to },
                Package = new PackageReferenceDto { Urn = string.Empty }
            };

            var response = await client.PostAsJsonAsync(
                $"{Route}/package",
                body,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }

    #endregion

    #region End-to-end: GetValidUrns then Create Request

    public class GetValidUrnsThenCreateRequestTest : IClassFixture<ApiFixture>
    {
        private static readonly ResourceType TestResourceType = new()
        {
            Id = Guid.Parse("01960001-0000-0000-0000-000000000098"),
            Name = "E2ETestResourceType",
        };

        private readonly ApiFixture _fixture;

        public GetValidUrnsThenCreateRequestTest(ApiFixture fixture)
        {
            _fixture = fixture;
            _fixture.EnsureSeedOnce(db =>
            {
                db.ResourceTypes.Add(TestResourceType);
                db.SaveChanges();

                db.Resources.Add(new Resource
                {
                    Id = Guid.CreateVersion7(),
                    Name = "E2ETestResource",
                    Description = "End-to-end test resource",
                    RefId = "test-resource-e2e-1",
                    ProviderId = ProviderConstants.ResourceRegistry,
                    TypeId = TestResourceType.Id,
                });
                db.SaveChanges();
            });
        }

        [Fact]
        public async Task GetValidUrns_ThenCreateResourceRequest_EndToEnd()
        {
            var client = CreateClient(_fixture, TestEntities.OrganizationNordisAS.Id);

            // Step 1: Get valid URN prefixes
            var urnsResponse = await client.GetAsync(
                $"{Route}/_meta/urns/party",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, urnsResponse.StatusCode);

            var urnsJson = await urnsResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var urnsDoc = JsonDocument.Parse(urnsJson);
            var urnPrefixes = urnsDoc.RootElement.EnumerateArray().Select(e => e.GetString()).ToList();

            var orgPrefix = urnPrefixes.First(u => u.Contains("organization"));
            var personPrefix = urnPrefixes.First(u => u.Contains("person"));

            // Step 2: Build URNs using the returned prefixes
            var from = $"{orgPrefix}:{TestEntities.OrganizationNordisAS.Entity.OrganizationIdentifier}";
            var to = $"{personPrefix}:{TestEntities.PersonPaula.Entity.PersonIdentifier}";

            // Step 3: Create resource request
            var body = new CreateResourceRequestInput
            {
                Connection = new ConnectionRequestInputDto { From = from, To = to },
                Resource = new ResourceReferenceDto { ResourceId = "test-resource-e2e-1" }
            };

            var response = await client.PostAsJsonAsync(
                $"{Route}/resource",
                body,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        [Fact]
        public async Task GetValidUrns_ThenCreatePackageRequest_EndToEnd()
        {
            var client = CreateClient(_fixture, TestEntities.OrganizationNordisAS.Id);

            // Step 1: Get valid URN prefixes
            var urnsResponse = await client.GetAsync(
                $"{Route}/_meta/urns/party",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, urnsResponse.StatusCode);

            var urnsJson = await urnsResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var urnsDoc = JsonDocument.Parse(urnsJson);
            var urnPrefixes = urnsDoc.RootElement.EnumerateArray().Select(e => e.GetString()).ToList();

            var orgPrefix = urnPrefixes.First(u => u.Contains("organization"));
            var personPrefix = urnPrefixes.First(u => u.Contains("person"));

            // Step 2: Build URNs using the returned prefixes
            var from = $"{orgPrefix}:{TestEntities.OrganizationNordisAS.Entity.OrganizationIdentifier}";
            var to = $"{personPrefix}:{TestEntities.PersonPaula.Entity.PersonIdentifier}";

            // Step 3: Create package request
            var body = new CreatePackageRequestInput
            {
                Connection = new ConnectionRequestInputDto { From = from, To = to },
                Package = new PackageReferenceDto { Urn = PackageConstants.Agriculture.Entity.Urn }
            };

            var response = await client.PostAsJsonAsync(
                $"{Route}/package",
                body,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }
    }

    #endregion
}
