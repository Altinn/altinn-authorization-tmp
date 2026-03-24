using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Altinn.AccessManagement.Api.Enduser.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.TestUtils;
using Altinn.AccessManagement.TestUtils.Data;
using Altinn.AccessManagement.TestUtils.Fixtures;
using Altinn.AccessManagement.TestUtils.Mocks;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessManagement.Enduser.Api.Tests.Controllers;

public partial class ConnectionsControllerTest
{
    /// <summary>
    /// <see cref="ConnectionsController.GetResources(Guid, Guid?, Guid?, AccessManagement.Api.Enduser.Models.PagingInput, string, CancellationToken)"/>
    /// </summary>
    public class GetResources : IClassFixture<ApiFixture>
    {
        public GetResources(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnduserControllerConnections);
            Fixture.ConfiureServices(services =>
            {
                services.AddSingleton<IAltinn2RightsClient, Altinn2RightsClientMock>();
            });
            Fixture.EnsureSeedOnce(db =>
            {
                var resourceType = db.ResourceTypes.FirstOrDefault(t => t.Id == Guid.Parse("0195efb8-7c80-7f26-817a-50893176320d"));
                if (resourceType is null)
                {
                    resourceType = new ResourceType() { Id = Guid.Parse("0195efb8-7c80-7f26-817a-50893176320d"), Name = "Test" };
                    db.ResourceTypes.Add(resourceType);
                }

                var skattResource = new Resource()
                {
                    Name = "Skattemelding",
                    Description = "Innlevering av skattemelding for næringsdrivende",
                    RefId = "app_skd_skattemelding",
                    TypeId = resourceType.Id,
                    ProviderId = ProviderConstants.Altinn3.Id,
                };

                var mvaResource = new Resource()
                {
                    Name = "MVA-melding",
                    Description = "Innlevering av merverdiavgiftsmelding",
                    RefId = "app_skd_mva-melding",
                    TypeId = resourceType.Id,
                    ProviderId = ProviderConstants.Altinn3.Id,
                };

                db.Resources.Add(skattResource);
                db.Resources.Add(mvaResource);

                var rightholderFromDumboToMille = new Assignment()
                {
                    FromId = TestData.DumboAdventures.Id,
                    ToId = TestData.MilleHundefrisor.Id,
                    RoleId = RoleConstants.Rightholder,
                };

                db.Assignments.Add(rightholderFromDumboToMille);
                db.SaveChanges();

                db.AssignmentResources.Add(new AssignmentResource()
                {
                    AssignmentId = rightholderFromDumboToMille.Id,
                    ResourceId = skattResource.Id,
                });

                db.AssignmentResources.Add(new AssignmentResource()
                {
                    AssignmentId = rightholderFromDumboToMille.Id,
                    ResourceId = mvaResource.Id,
                });

                db.SaveChanges();
            });
        }

        public ApiFixture Fixture { get; }

        private HttpClient CreateClient(Guid partyUuid, params string[] scopes)
        {
            var client = Fixture.Server.CreateClient();
            var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
            {
                claims.Add(new Claim(AltinnCoreClaimTypes.PartyUuid, partyUuid.ToString()));
                claims.Add(new Claim("scope", string.Join(" ", scopes)));
            });
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            return client;
        }

        [Fact]
        public async Task GetResources_AsMalinForDumboToMille_WithToOthersScope_ReturnsOk()
        {
            HttpClient client = CreateClient(TestData.MalinEmilie.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_READ);

            HttpResponseMessage response = await client.GetAsync($"{Route}/resources?party={TestData.DumboAdventures.Id}&from={TestData.DumboAdventures.Id}&to={TestData.MilleHundefrisor.Id}", TestContext.Current.CancellationToken);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK but got {response.StatusCode}. Response body: {responseContent}");

            List<ResourcePermissionDto> result = JsonSerializer.Deserialize<List<ResourcePermissionDto>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.True(result.Count >= 2, $"Expected at least 2 resources but got {result.Count}. Response body: {responseContent}");
            Assert.Contains(result, r => r.Resource.RefId == "app_skd_skattemelding");
            Assert.Contains(result, r => r.Resource.RefId == "app_skd_mva-melding");
        }

        [Fact]
        public async Task GetResources_AsMalinForDumboFromMille_WithFromOthersScope_ReturnsOk()
        {
            HttpClient client = CreateClient(TestData.MalinEmilie.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_FROMOTHERS_READ);

            HttpResponseMessage response = await client.GetAsync($"{Route}/resources?party={TestData.DumboAdventures.Id}&to={TestData.DumboAdventures.Id}&from={TestData.MilleHundefrisor.Id}", TestContext.Current.CancellationToken);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK but got {response.StatusCode}. Response body: {responseContent}");
        }

        [Fact]
        public async Task GetResources_AsTheaForMilleFromDumbo_WithFromOthersScope_ReturnsOk()
        {
            HttpClient client = CreateClient(TestData.Thea.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_FROMOTHERS_READ);

            HttpResponseMessage response = await client.GetAsync($"{Route}/resources?party={TestData.MilleHundefrisor.Id}&to={TestData.MilleHundefrisor.Id}&from={TestData.DumboAdventures.Id}", TestContext.Current.CancellationToken);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK but got {response.StatusCode}. Response body: {responseContent}");

            List<ResourcePermissionDto> result = JsonSerializer.Deserialize<List<ResourcePermissionDto>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.True(result.Count >= 2, $"Expected at least 2 resources but got {result.Count}. Response body: {responseContent}");
            Assert.Contains(result, r => r.Resource.RefId == "app_skd_skattemelding");
            Assert.Contains(result, r => r.Resource.RefId == "app_skd_mva-melding");
        }

        [Fact]
        public async Task GetResources_AsTheaForMilleToDumbo_WithToOthersScope_ReturnsOk()
        {
            HttpClient client = CreateClient(TestData.Thea.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_READ);

            HttpResponseMessage response = await client.GetAsync($"{Route}/resources?party={TestData.MilleHundefrisor.Id}&from={TestData.MilleHundefrisor.Id}&to={TestData.DumboAdventures.Id}", TestContext.Current.CancellationToken);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK but got {response.StatusCode}. Response body: {responseContent}");
        }

        [Fact]
        public async Task GetResources_AsTheaForMilleFromDumbo_WithToOthersScope_ReturnsForbidden()
        {
            HttpClient client = CreateClient(TestData.Thea.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_READ);

            HttpResponseMessage response = await client.GetAsync($"{Route}/resources?party={TestData.MilleHundefrisor.Id}&to={TestData.MilleHundefrisor.Id}&from={TestData.DumboAdventures.Id}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetResources_ToOthersDirection_WithFromOthersScope_ReturnsForbidden()
        {
            HttpClient client = CreateClient(TestData.MalinEmilie.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_FROMOTHERS_READ);

            HttpResponseMessage response = await client.GetAsync($"{Route}/resources?party={TestData.DumboAdventures.Id}&from={TestData.DumboAdventures.Id}&to={TestData.MilleHundefrisor.Id}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetResources_FromOthersDirection_WithToOthersScope_ReturnsForbidden()
        {
            HttpClient client = CreateClient(TestData.MalinEmilie.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_READ);

            HttpResponseMessage response = await client.GetAsync($"{Route}/resources?party={TestData.DumboAdventures.Id}&to={TestData.DumboAdventures.Id}&from={TestData.MilleHundefrisor.Id}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetResources_WithWriteScope_ReturnsForbidden()
        {
            HttpClient client = CreateClient(TestData.MalinEmilie.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_WRITE);

            HttpResponseMessage response = await client.GetAsync($"{Route}/resources?party={TestData.DumboAdventures.Id}&from={TestData.DumboAdventures.Id}&to={TestData.MilleHundefrisor.Id}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
