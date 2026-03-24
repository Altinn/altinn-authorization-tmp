using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Altinn.AccessManagement.Api.Enduser.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Services.Interfaces;
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
    /// Tests for <see cref="ConnectionsController.GetResourceRights(Guid, Guid, Guid, string, AccessManagement.Api.Enduser.Models.PagingInput, CancellationToken)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Seed Data:
    /// - ResourceType "Test"
    /// - Resource "Skattemelding" (app_skd_skattemelding)
    /// - Assignment: Dumbo Adventures → Mille Hundefrisør (Rightholder)
    /// - AssignmentResource linking Skattemelding to the assignment above
    /// </para>
    /// <para>
    /// Actors:
    /// - Malin Emilie: managing director of Dumbo Adventures (views from Dumbo's perspective)
    /// - Thea: managing director of Mille Hundefrisør (views from Mille's perspective)
    /// </para>
    /// <para>
    /// The tests verify that the endpoint returns direct and indirect rights for a specific resource
    /// between two parties, and that the correct bidirectional read scope is required depending on
    /// the direction of the query (to-others vs from-others). Mismatched scopes result in HTTP 403 Forbidden.
    /// </para>
    /// </remarks>
    public class GetResourceRights : IClassFixture<ApiFixture>
    {
        public GetResourceRights(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnduserControllerConnections);
            Fixture.ConfiureServices(services =>
            {
                services.AddSingleton<IAltinn2RightsClient, Altinn2RightsClientMock>();
                services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
            });
            Fixture.EnsureSeedOnce(db =>
            {
                Resource skattResource = new()
                {
                    Name = "Skattemelding med næringsspesifikasjon 2020",
                    Description = "Skattemelding med næringsspesifikasjon 2020",
                    RefId = "app_skd_sirius-skattemelding-v1",
                    TypeId = TestData.TestResourceType.Id,
                    ProviderId = ProviderConstants.Altinn3.Id,
                };

                db.Resources.Add(skattResource);

                var rightholderFromDumboToMille = new Assignment()
                {
                    FromId = TestData.DumboAdventures.Id,
                    ToId = TestData.MilleHundefrisor.Id,
                    RoleId = RoleConstants.Rightholder
                };

                db.Assignments.Add(rightholderFromDumboToMille);
                db.SaveChanges();

                db.AssignmentResources.Add(new AssignmentResource()
                {
                    AssignmentId = rightholderFromDumboToMille.Id,
                    ResourceId = skattResource.Id,
                    PolicyPath = "sirius-skattemelding-v1/50083510/p50155461/delegationpolicy.xml"
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

        /// <summary>
        /// Malin (MD of Dumbo) queries resource rights for Skattemelding delegated to Mille in the to-others direction.
        /// Expects OK with a valid response containing the resource.
        /// </summary>
        [Fact]
        public async Task GetResourceRights_AsMalinForDumboToMille_WithToOthersScope_ReturnsOk()
        {
            HttpClient client = CreateClient(TestData.MalinEmilie.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_READ);

            HttpResponseMessage response = await client.GetAsync(
                $"{Route}/resources/rights?party={TestData.DumboAdventures.Id}&from={TestData.DumboAdventures.Id}&to={TestData.MilleHundefrisor.Id}&resource=app_skd_sirius-skattemelding-v1",
                TestContext.Current.CancellationToken);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK but got {response.StatusCode}. Response body: {responseContent}");

            using JsonDocument doc = JsonDocument.Parse(responseContent);
            JsonElement root = doc.RootElement;

            Assert.True(root.TryGetProperty("resource", out JsonElement resourceElement), "Response should contain a 'resource' property");
            Assert.True(resourceElement.TryGetProperty("refId", out JsonElement refIdElement), "Resource should contain a 'refId' property");
            Assert.Equal("app_skd_sirius-skattemelding-v1", refIdElement.GetString());
        }

        /// <summary>
        /// Thea (MD of Mille) queries resource rights for Skattemelding received from Dumbo in the from-others direction.
        /// Expects OK with a valid response containing the resource.
        /// </summary>
        [Fact]
        public async Task GetResourceRights_AsTheaForMilleFromDumbo_WithFromOthersScope_ReturnsOk()
        {
            HttpClient client = CreateClient(TestData.Thea.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_FROMOTHERS_READ);

            HttpResponseMessage response = await client.GetAsync(
                $"{Route}/resources/rights?party={TestData.MilleHundefrisor.Id}&from={TestData.DumboAdventures.Id}&to={TestData.MilleHundefrisor.Id}&resource=app_skd_sirius-skattemelding-v1",
                TestContext.Current.CancellationToken);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK but got {response.StatusCode}. Response body: {responseContent}");

            using JsonDocument doc = JsonDocument.Parse(responseContent);
            JsonElement root = doc.RootElement;

            Assert.True(root.TryGetProperty("resource", out JsonElement resourceElement), "Response should contain a 'resource' property");
            Assert.True(resourceElement.TryGetProperty("refId", out JsonElement refIdElement), "Resource should contain a 'refId' property");
            Assert.Equal("app_skd_sirius-skattemelding-v1", refIdElement.GetString());
        }

        /// <summary>
        /// Malin uses from-others read scope for a to-others direction query.
        /// Expects 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task GetResourceRights_ToOthersDirection_WithFromOthersScope_ReturnsForbidden()
        {
            HttpClient client = CreateClient(TestData.MalinEmilie.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_FROMOTHERS_READ);

            HttpResponseMessage response = await client.GetAsync(
                $"{Route}/resources/rights?party={TestData.DumboAdventures.Id}&from={TestData.DumboAdventures.Id}&to={TestData.MilleHundefrisor.Id}&resource=app_skd_sirius-skattemelding-v1",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        /// <summary>
        /// Thea uses to-others read scope for a from-others direction query.
        /// Expects 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task GetResourceRights_FromOthersDirection_WithToOthersScope_ReturnsForbidden()
        {
            HttpClient client = CreateClient(TestData.Thea.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_READ);

            HttpResponseMessage response = await client.GetAsync(
                $"{Route}/resources/rights?party={TestData.MilleHundefrisor.Id}&from={TestData.DumboAdventures.Id}&to={TestData.MilleHundefrisor.Id}&resource=app_skd_sirius-skattemelding-v1",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        /// <summary>
        /// Malin uses a write scope on the read-only GetResourceRights endpoint.
        /// Expects 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task GetResourceRights_WithWriteScope_ReturnsForbidden()
        {
            HttpClient client = CreateClient(TestData.MalinEmilie.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_WRITE);

            HttpResponseMessage response = await client.GetAsync(
                $"{Route}/resources/rights?party={TestData.DumboAdventures.Id}&from={TestData.DumboAdventures.Id}&to={TestData.MilleHundefrisor.Id}&resource=app_skd_sirius-skattemelding-v1",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
