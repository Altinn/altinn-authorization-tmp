using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Xml;
using Altinn.AccessManagement.Api.Enduser.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.TestUtils;
using Altinn.AccessManagement.TestUtils.Data;
using Altinn.AccessManagement.TestUtils.Fixtures;
using Altinn.AccessManagement.TestUtils.Mocks;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.ABAC.Constants;
using Altinn.Authorization.ABAC.Utils;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessManagement.Enduser.Api.Tests.Controllers;

/// <summary>
/// Partial test class for ConnectionsController, focused on testing the UpdateInstanceRights
/// (PUT resources/instances/rights) endpoint. Jinx (as a private person) delegates instance rights
/// to Thea, then updates them. Tests cover the full lifecycle: add, verify, update, verify updated state.
/// </summary>
public partial class ConnectionsControllerTest
{
    /// <summary>
    /// Tests for <see cref="ConnectionsController.UpdateInstanceRights(Guid, Guid, string, string, RightKeyListDto, CancellationToken)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Seed Data:
    /// - Assignment: Jinx Arcane → Thea BFF (Rightholder) seeded locally
    /// - Resource: SiriusSkattemelding (app_skd_sirius-skattemelding-v1)
    /// </para>
    /// <para>
    /// Actors:
    /// - Jinx Arcane: private person delegating instance rights from herself to Thea
    /// </para>
    /// </remarks>
    public class UpdateInstanceRights : IClassFixture<ApiFixture>
    {
        private const string SiriusInstanceIdForUpdate = "urn:altinn:instance-id:50401001/d4e5f6a7-b8c9-4d0e-1f2a-3b4c5d6e7f80";

        public UpdateInstanceRights(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnduserControllerConnections);
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.InstanceDbEf);
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.ResourceDelegationEF);
            Fixture.ConfiureServices(services =>
            {
                services.AddSingleton<IAltinn2RightsClient, Altinn2RightsClientMock>();
                services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointWithWrittenPoliciesMock>();
                services.AddSingleton<IPolicyFactory, PolicyFactoryMock>();
            });
            Fixture.EnsureSeedOnce(db =>
            {
                var rightholderFromJinxToThea = new Assignment()
                {
                    FromId = TestData.JinxArcane.Id,
                    ToId = TestData.Thea.Id,
                    RoleId = RoleConstants.Rightholder,
                };

                db.Assignments.Add(rightholderFromJinxToThea);
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
        /// Helper to get valid right keys via the instance delegation check endpoint.
        /// </summary>
        private async Task<List<string>> GetDelegatableInstanceRightKeys(string resource, string instance)
        {
            HttpClient client = CreateClient(TestData.JinxArcane.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_WRITE);
            HttpResponseMessage response = await client.GetAsync(
                $"{Route}/resources/instances/delegationcheck?party={TestData.JinxArcane.Id}&resource={resource}&instance={instance}",
                TestContext.Current.CancellationToken);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"Delegation check failed: {response.StatusCode}. Response body: {responseContent}");

            InstanceCheckDto result = JsonSerializer.Deserialize<InstanceCheckDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result.Rights
                .Where(r => r.Result)
                .Select(r => r.Right.Key)
                .ToList();
        }

        /// <summary>
        /// Helper to add initial instance rights delegation.
        /// </summary>
        private async Task AddInitialInstanceRights(string resource, string instance, IEnumerable<string> rightKeys)
        {
            var body = new InstanceRightsDelegationDto { DirectRightKeys = rightKeys };
            HttpClient client = CreateClient(TestData.JinxArcane.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_WRITE);

            HttpResponseMessage response = await client.PostAsJsonAsync(
                $"{Route}/resources/instances/rights?party={TestData.JinxArcane.Id}&to={TestData.Thea.Id}&resource={resource}&instance={instance}",
                body,
                TestContext.Current.CancellationToken);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(response.StatusCode == HttpStatusCode.Created, $"Initial delegation failed: {response.StatusCode}. Response body: {responseContent}");
        }

        /// <summary>
        /// Jinx delegates initial instance rights (first right key only) to Thea, then updates
        /// to include more rights. Verifies:
        /// - Initial delegation succeeds with a subset of right keys
        /// - GetInstanceRights returns only the initially delegated rights
        /// - Update (PUT) succeeds with a larger set of right keys
        /// - GetInstanceRights returns the updated set of rights
        /// - Written XACML policy matches the updated right keys
        /// </summary>
        [Fact]
        public async Task UpdateInstanceRights_AsJinxToThea_WithMoreRightKeys_ReturnsOkAndUpdatesRights()
        {
            List<string> allRightKeys = await GetDelegatableInstanceRightKeys("app_skd_sirius-skattemelding-v1", SiriusInstanceIdForUpdate);
            Assert.True(allRightKeys.Count >= 2, $"Expected at least 2 delegatable right keys, but got {allRightKeys.Count}");

            // Step 1: Add initial delegation with just the first right key
            var initialRightKeys = new List<string> { allRightKeys.First() };
            await AddInitialInstanceRights("app_skd_sirius-skattemelding-v1", SiriusInstanceIdForUpdate, initialRightKeys);

            // Step 2: Verify initial state via GetInstanceRights
            HttpClient readClient = CreateClient(TestData.JinxArcane.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_READ);

            HttpResponseMessage initialGetResponse = await readClient.GetAsync(
                $"{Route}/resources/instances/rights?party={TestData.JinxArcane.Id}&from={TestData.JinxArcane.Id}&to={TestData.Thea.Id}&resource=app_skd_sirius-skattemelding-v1&instance={SiriusInstanceIdForUpdate}",
                TestContext.Current.CancellationToken);

            string initialGetContent = await initialGetResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(initialGetResponse.StatusCode == HttpStatusCode.OK, $"Initial GET failed: {initialGetResponse.StatusCode}. Response body: {initialGetContent}");

            ExtInstanceRightDto initialRights = await initialGetResponse.Content.ReadFromJsonAsync<ExtInstanceRightDto>(TestContext.Current.CancellationToken);
            Assert.NotNull(initialRights);
            Assert.Single(initialRights.DirectRights);
            Assert.Equal(initialRightKeys.First(), initialRights.DirectRights.First().Right.Key);

            // Step 3: Update with more right keys (first 3 or all if fewer)
            var updatedRightKeys = allRightKeys.Take(3).ToList();
            var updateBody = new RightKeyListDto { DirectRightKeys = updatedRightKeys };
            HttpClient writeClient = CreateClient(TestData.JinxArcane.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_WRITE);

            HttpResponseMessage putResponse = await writeClient.PutAsJsonAsync(
                $"{Route}/resources/instances/rights?party={TestData.JinxArcane.Id}&to={TestData.Thea.Id}&resource=app_skd_sirius-skattemelding-v1&instance={SiriusInstanceIdForUpdate}",
                updateBody,
                TestContext.Current.CancellationToken);

            string putResponseContent = await putResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(putResponse.StatusCode == HttpStatusCode.OK, $"Expected OK but got {putResponse.StatusCode}. Response body: {putResponseContent}");

            // Step 4: Verify updated state via GetInstanceRights
            HttpClient readClient2 = CreateClient(TestData.JinxArcane.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_READ);

            HttpResponseMessage updatedGetResponse = await readClient2.GetAsync(
                $"{Route}/resources/instances/rights?party={TestData.JinxArcane.Id}&from={TestData.JinxArcane.Id}&to={TestData.Thea.Id}&resource=app_skd_sirius-skattemelding-v1&instance={SiriusInstanceIdForUpdate}",
                TestContext.Current.CancellationToken);

            string updatedGetContent = await updatedGetResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(updatedGetResponse.StatusCode == HttpStatusCode.OK, $"Updated GET failed: {updatedGetResponse.StatusCode}. Response body: {updatedGetContent}");

            ExtInstanceRightDto updatedRights = await updatedGetResponse.Content.ReadFromJsonAsync<ExtInstanceRightDto>(TestContext.Current.CancellationToken);
            Assert.NotNull(updatedRights);
            Assert.Equal("app_skd_sirius-skattemelding-v1", updatedRights.Resource.RefId);
            Assert.Equal(updatedRightKeys.Count, updatedRights.DirectRights.Count);
            Assert.Empty(updatedRights.IndirectRights);

            // Verify the right keys match the updated set
            var returnedRightKeys = updatedRights.DirectRights.Select(r => r.Right.Key).OrderBy(k => k).ToList();
            var expectedRightKeys = updatedRightKeys.OrderBy(k => k).ToList();
            Assert.Equal(expectedRightKeys, returnedRightKeys);

            // Verify permission structure on updated rights
            foreach (var right in updatedRights.DirectRights)
            {
                Assert.True(right.Reason.Flag.Equals(AccessReasonFlag.Direct), $"Expected Direct but got {right.Reason.Flag}.");
                Assert.Single(right.Permissions);
                PermissionDto permission = right.Permissions[0];
                Assert.Equal(TestData.Thea.Entity.Name, permission.To.Name);
                Assert.True(permission.To.Id == TestData.Thea.Id);
                Assert.Equal(TestData.JinxArcane.Entity.Name, permission.From.Name);
                Assert.True(permission.From.Id == TestData.JinxArcane.Id);
                Assert.True(permission.Role.Id == RoleConstants.Rightholder, $"Expected Rightholder role but got {permission.Role.Id}.");
            }

            // Step 5: Verify the written XACML policy
            var policyFactory = Fixture.Server.Services.GetRequiredService<IPolicyFactory>() as PolicyFactoryMock;
            Assert.NotNull(policyFactory);
            Assert.NotEmpty(policyFactory.WrittenPolicies);

            var (_, content) = policyFactory.WrittenPolicies.Last();
            XacmlPolicy policy;
            using (XmlReader reader = XmlReader.Create(new MemoryStream(content)))
            {
                policy = XacmlParser.ParseXacmlPolicy(reader);
            }

            Assert.Equal(updatedRightKeys.Count, policy.Rules.Count);

            // Verify subject targets Thea's UUID (instance delegation uses UUID, not partyId)
            foreach (XacmlRule rule in policy.Rules)
            {
                var subjectMatches = rule.Target.AnyOf
                    .SelectMany(anyOf => anyOf.AllOf)
                    .SelectMany(allOf => allOf.Matches)
                    .Where(match => match.AttributeDesignator.Category.OriginalString == XacmlConstants.MatchAttributeCategory.Subject)
                    .ToList();
                Assert.NotEmpty(subjectMatches);

                foreach (XacmlMatch match in subjectMatches)
                {
                    Assert.Equal(TestData.Thea.Id.ToString(), match.AttributeValue.Value);
                }
            }
        }

        /// <summary>
        /// Jinx uses from-others read scope on the update endpoint.
        /// Expects 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task UpdateInstanceRights_WithReadScope_ReturnsForbidden()
        {
            var body = new RightKeyListDto { DirectRightKeys = ["some-key"] };
            HttpClient client = CreateClient(TestData.JinxArcane.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_FROMOTHERS_READ);

            HttpResponseMessage response = await client.PutAsJsonAsync(
                $"{Route}/resources/instances/rights?party={TestData.JinxArcane.Id}&to={TestData.Thea.Id}&resource=app_skd_sirius-skattemelding-v1&instance={SiriusInstanceIdForUpdate}",
                body,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        /// <summary>
        /// Jinx uses to-others read scope (not write) on the update endpoint.
        /// Expects 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task UpdateInstanceRights_WithToOthersReadScope_ReturnsForbidden()
        {
            var body = new RightKeyListDto { DirectRightKeys = ["some-key"] };
            HttpClient client = CreateClient(TestData.JinxArcane.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_READ);

            HttpResponseMessage response = await client.PutAsJsonAsync(
                $"{Route}/resources/instances/rights?party={TestData.JinxArcane.Id}&to={TestData.Thea.Id}&resource=app_skd_sirius-skattemelding-v1&instance={SiriusInstanceIdForUpdate}",
                body,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        /// <summary>
        /// Jinx uses from-others write scope on the update endpoint.
        /// Expects 403 Forbidden.
        /// </summary>
        [Fact]
        public async Task UpdateInstanceRights_WithFromOthersWriteScope_ReturnsForbidden()
        {
            var body = new RightKeyListDto { DirectRightKeys = ["some-key"] };
            HttpClient client = CreateClient(TestData.JinxArcane.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_FROMOTHERS_WRITE);

            HttpResponseMessage response = await client.PutAsJsonAsync(
                $"{Route}/resources/instances/rights?party={TestData.JinxArcane.Id}&to={TestData.Thea.Id}&resource=app_skd_sirius-skattemelding-v1&instance={SiriusInstanceIdForUpdate}",
                body,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
