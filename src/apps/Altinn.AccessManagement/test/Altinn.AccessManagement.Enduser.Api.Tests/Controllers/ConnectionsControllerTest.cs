using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Altinn.AccessManagement.Api.Enduser.Controllers;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.TestUtils;
using Altinn.AccessManagement.TestUtils.Data;
using Altinn.AccessManagement.TestUtils.Fixtures;
using Altinn.AccessManagement.TestUtils.Mocks;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.Api.Contracts.AccessManagement.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessManagement.Enduser.Api.Tests.Controllers;

/// <summary>
/// <see cref="ConnectionsControllerTest"/>
/// </summary>
public class ConnectionsControllerTest
{
    public const string Route = "accessmanagement/api/v1/enduser/connections";

    #region GET accessmanagement/api/v1/enduser/connections

    /// <summary>
    /// <see cref="ConnectionsController.GetConnections(AccessManagement.Api.Enduser.Models.ConnectionInput, AccessManagement.Api.Enduser.Models.PagingInput, bool, bool, CancellationToken)"
    /// </summary>
    public class GetConnections : IClassFixture<ApiFixture>
    {
        public GetConnections(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnduserControllerConnections);
            Fixture.EnsureSeedOnce(db =>
            {
                var rightholderFromNordisToVerdiq = new Assignment()
                {
                    FromId = TestEntities.OrganizationNordisAS.Id,
                    ToId = TestEntities.OrganizationVerdiqAS.Id,
                    RoleId = RoleConstants.Rightholder,
                };

                var accountantFromNordisToVerdiq = new Assignment()
                {
                    FromId = TestEntities.OrganizationNordisAS.Id,
                    ToId = TestEntities.OrganizationVerdiqAS.Id,
                    RoleId = RoleConstants.Accountant,
                };

                var agentFromVerdiqToPaula = new Assignment()
                {
                    FromId = TestEntities.OrganizationVerdiqAS.Id,
                    ToId = TestEntities.PersonPaula,
                    RoleId = RoleConstants.Agent,
                };
                var agentFromVerdiqToOrjan = new Assignment()
                {
                    FromId = TestEntities.OrganizationVerdiqAS.Id,
                    ToId = TestEntities.PersonOrjan,
                    RoleId = RoleConstants.Agent,
                };
                var agentFromNordisToPaula = new Assignment()
                {
                    FromId = TestEntities.OrganizationNordisAS.Id,
                    ToId = TestEntities.PersonPaula,
                    RoleId = RoleConstants.Agent,
                };

                var assignmentPackageFromNordisToVerdiq = new AssignmentPackage()
                {
                    AssignmentId = rightholderFromNordisToVerdiq.Id,
                    PackageId = PackageConstants.DocumentBasedSupervision,
                };

                db.Assignments.Add(rightholderFromNordisToVerdiq);
                db.Assignments.Add(accountantFromNordisToVerdiq);
                db.Assignments.Add(agentFromVerdiqToPaula);
                db.Assignments.Add(agentFromVerdiqToOrjan);
                db.Assignments.Add(agentFromNordisToPaula);

                db.SaveChanges();
            });
        }

        public ApiFixture Fixture { get; }

        private HttpClient CreateClient(params string[] scopes)
        {
            var client = Fixture.Server.CreateClient();
            var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
            {
                claims.Add(new Claim("scope", string.Join(" ", scopes)));
            });
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            return client;
        }
        
        [Fact]
        public async Task ListConnections_WithDirectionFromOthersUsingFromOthersScope_ReturnsOk()
        {
            var client = CreateClient(AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_FROMOTHERS_READ, "some:other/scope.read");

            var response = await client.GetAsync($"{Route}?party={TestEntities.OrganizationVerdiqAS.Id}&to={TestEntities.OrganizationVerdiqAS.Id}", TestContext.Current.CancellationToken);

            var data = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<PaginatedResult<Authorization.Api.Contracts.AccessManagement.ConnectionDto>>(data);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ListConnections_WithDirectionFromOthersUsingToOthersScope_ReturnsForbidden()
        {
            var client = CreateClient(AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_READ);

            var response = await client.GetAsync($"{Route}?party={TestEntities.OrganizationVerdiqAS.Id}&to={TestEntities.OrganizationVerdiqAS.Id}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ListConnections_WithDirectionToOthersUsingToOthersScope_ReturnsOk()
        {
            var client = CreateClient("some:other/scope.read", AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_READ);

            var response = await client.GetAsync($"{Route}?party={TestEntities.OrganizationVerdiqAS.Id}&from={TestEntities.OrganizationVerdiqAS.Id}", TestContext.Current.CancellationToken);

            var data = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = JsonSerializer.Deserialize<PaginatedResult<Authorization.Api.Contracts.AccessManagement.ConnectionDto>>(data);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task ListConnections_WithDirectionToOthersUsingFromOthersScope_ReturnsForbidden()
        {
            var client = CreateClient(AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_FROMOTHERS_READ);

            var response = await client.GetAsync($"{Route}?party={TestEntities.OrganizationVerdiqAS.Id}&from={TestEntities.OrganizationVerdiqAS.Id}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ListConnections_WithDirectionToOthersUsingToOtherScopeWithPartyNotEqualFromOrTo_ReturnsForbidden()
        {
            var client = CreateClient(AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_FROMOTHERS_READ);

            var response = await client.GetAsync($"{Route}?party={TestEntities.OrganizationVerdiqAS.Id}&from={TestEntities.PersonPaula.Id}&to={TestEntities.PersonOrjan.Id}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    #endregion

    #region GET accessmanagement/api/v1/enduser/connections/resources/delegationcheck

    /// <summary>
    /// <see cref="ConnectionsController.CheckResource(Guid, string, CancellationToken)"/>
    /// </summary>
    public class CheckResource : IClassFixture<ApiFixture>
    {
        public CheckResource(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnduserControllerConnections);
            Fixture.ConfiureServices(services =>
            {
                services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
            });
            Fixture.EnsureSeedOnce(db =>
            {
                var resourceType = db.ResourceTypes.FirstOrDefault(t => t.Id == Guid.Parse("0195efb8-7c80-7f26-817a-50893176320d"));
                if (resourceType is null)
                {
                    resourceType = new ResourceType() { Id = Guid.Parse("0195efb8-7c80-7f26-817a-50893176320d"), Name = "Test" };
                    db.ResourceTypes.Add(resourceType);
                }

                Resource navSykepengerResource = new Resource()
                {
                    Name = "Dialogs for sickness benefits",
                    Description = "The service is used to send and receive dialogues in Dialogporten about new sick leaves, submitted applications, requests for income reports, and receipts for submitted income reports.",
                    RefId = "nav_sykepenger_dialog",
                    TypeId = resourceType.Id,
                    ProviderId = ProviderConstants.Altinn3.Id,
                };

                db.Resources.Add(navSykepengerResource);

                Resource diheOmsetningsoppgaveAlkhol = new Resource()
                {
                    Name = "Omsetningsoppgave for alkohol",
                    Description = "Omsetningsoppgave for alkohol",
                    RefId = "app_dihe_omsetningsoppgave-for-alkohol",
                    TypeId = resourceType.Id,
                    ProviderId = ProviderConstants.Altinn3.Id,
                };

                db.Resources.Add(diheOmsetningsoppgaveAlkhol);

                var rightholderFromNordisToVerdiq = new Assignment()
                {
                    FromId = TestEntities.OrganizationNordisAS.Id,
                    ToId = TestEntities.OrganizationVerdiqAS.Id,
                    RoleId = RoleConstants.Rightholder,
                };

                db.Assignments.Add(rightholderFromNordisToVerdiq);
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
        /// Tests that a managing director can successfully check delegation rights for a resource when using the correct authorization scope.
        /// </summary>
        /// <remarks>
        /// This test verifies the delegation check functionality for the resource "Dialogs for sickness benefits" (nav_sykepenger_dialog).
        /// <para>
        /// Test Scenario:
        /// - Actor: Malin Emilie (managing director of Dumbo Adventures)
        /// - Authorization Scope: SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_WRITE
        /// - Resource: nav_sykepenger_dialog (Dialogs for sickness benefits)
        /// - Party: Dumbo Adventures organization
        /// </para>
        /// <para>
        /// Assertions:
        /// - HTTP response status is OK (200)
        /// - Response contains a valid ResourceCheckDto object
        /// - Response includes exactly 3 rights: "read", "access", and "subscribe"
        /// - All rights have Result = true
        /// - Each right contains ReasonCodes with both RoleAccess and PackageAccess
        /// </para>
        /// </remarks>
        [Fact]
        public async Task CheckResource_NavSykemeldingDialog_FullAccess_RolesAndPackages_ReturnsOK()
        {
            HttpClient client = CreateClient(TestData.MalinEmilie.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_WRITE);
            HttpResponseMessage response = await client.GetAsync($"{Route}/resources/delegationcheck?party={TestData.DumboAdventures.Id}&resource=nav_sykepenger_dialog", TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            ResourceCheckDto result = JsonSerializer.Deserialize<ResourceCheckDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(result);
            Assert.NotNull(result.Rights);
            Assert.DoesNotContain(result.Rights, r => r.Result == false);
            Assert.Equal(3, result.Rights.Count());

            // Check for the "read" right
            RightCheckDto readRight = result.Rights.FirstOrDefault(r => r.Right.Name.Equals("read", StringComparison.InvariantCultureIgnoreCase));
            Assert.NotNull(readRight);
            Assert.True(readRight.Result, "The 'read' right should have Result = true");
            Assert.NotEmpty(readRight.ReasonCodes);
            Assert.Contains(readRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.RoleAccess));
            Assert.Contains(readRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.PackageAccess));

            // Check for the "access" right
            RightCheckDto accessRight = result.Rights.FirstOrDefault(r => r.Right.Name.Equals("access", StringComparison.InvariantCultureIgnoreCase));
            Assert.NotNull(accessRight);
            Assert.True(accessRight.Result, "The 'access' right should have Result = true");
            Assert.NotEmpty(accessRight.ReasonCodes);
            Assert.Contains(accessRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.RoleAccess));
            Assert.Contains(accessRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.PackageAccess));

            // Check for the "subscribe" right
            RightCheckDto subscribeRight = result.Rights.FirstOrDefault(r => r.Right.Name.Equals("subscribe", StringComparison.InvariantCultureIgnoreCase));
            Assert.NotNull(subscribeRight);
            Assert.True(subscribeRight.Result, "The 'subscribe' right should have Result = true");
            Assert.NotEmpty(subscribeRight.ReasonCodes);
            Assert.Contains(subscribeRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.RoleAccess));
            Assert.Contains(subscribeRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.PackageAccess));
        }

        [Fact]
        public async Task CheckResource_NavSykemeldingDialog_FullAccess_Packages_ReturnsOK()
        {
            HttpClient client = CreateClient(TestData.Thea.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_WRITE);
            HttpResponseMessage response = await client.GetAsync($"{Route}/resources/delegationcheck?party={TestData.DumboAdventures.Id}&resource=nav_sykepenger_dialog", TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            ResourceCheckDto result = JsonSerializer.Deserialize<ResourceCheckDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(result);
            Assert.NotNull(result.Rights);
            Assert.DoesNotContain(result.Rights, r => r.Result == false);
            Assert.Equal(3, result.Rights.Count());

            // Check for the "read" right
            RightCheckDto readRight = result.Rights.FirstOrDefault(r => r.Right.Name.Equals("read", StringComparison.InvariantCultureIgnoreCase));
            Assert.NotNull(readRight);
            Assert.True(readRight.Result, "The 'read' right should have Result = true");
            Assert.NotEmpty(readRight.ReasonCodes);
            Assert.DoesNotContain(readRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.RoleAccess));
            Assert.Contains(readRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.PackageAccess));

            // Check for the "access" right
            RightCheckDto accessRight = result.Rights.FirstOrDefault(r => r.Right.Name.Equals("access", StringComparison.InvariantCultureIgnoreCase));
            Assert.NotNull(accessRight);
            Assert.True(accessRight.Result, "The 'access' right should have Result = true");
            Assert.NotEmpty(accessRight.ReasonCodes);
            Assert.DoesNotContain(accessRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.RoleAccess));
            Assert.Contains(accessRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.PackageAccess));

            // Check for the "subscribe" right
            RightCheckDto subscribeRight = result.Rights.FirstOrDefault(r => r.Right.Name.Equals("subscribe", StringComparison.InvariantCultureIgnoreCase));
            Assert.NotNull(subscribeRight);
            Assert.True(subscribeRight.Result, "The 'subscribe' right should have Result = true");
            Assert.NotEmpty(subscribeRight.ReasonCodes);
            Assert.DoesNotContain(subscribeRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.RoleAccess));
            Assert.Contains(subscribeRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.PackageAccess));
        }

        [Fact]
        public async Task CheckResource_DiheOmsettningsoppgave_PartialAccess_RolesAndPackages_ReturnsOK()
        {
            HttpClient client = CreateClient(TestData.MalinEmilie.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_WRITE);
            HttpResponseMessage response = await client.GetAsync($"{Route}/resources/delegationcheck?party={TestData.DumboAdventures.Id}&resource=app_dihe_omsetningsoppgave-for-alkohol", TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            ResourceCheckDto result = JsonSerializer.Deserialize<ResourceCheckDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(result);
            Assert.NotNull(result.Rights);
            Assert.Contains(result.Rights, r => r.Result == false);
            Assert.Contains(result.Rights, r => r.Result == true);
            Assert.Equal(17, result.Rights.Count());

            // Check for the "read" right
            RightCheckDto readRight = result.Rights.FirstOrDefault(r => r.Right.Name.Equals("read", StringComparison.InvariantCultureIgnoreCase));
            Assert.NotNull(readRight);
            Assert.True(readRight.Result, "The 'read' right should have Result = true");
            Assert.NotEmpty(readRight.ReasonCodes);
            Assert.Contains(readRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.RoleAccess));
            Assert.DoesNotContain(readRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.PackageAccess));

            // Check for the "access" right
            RightCheckDto writeRight = result.Rights.FirstOrDefault(r => r.Right.Name.Equals("Write (Task_1)", StringComparison.InvariantCultureIgnoreCase));
            Assert.NotNull(writeRight);
            Assert.True(writeRight.Result, "The 'Write' right should have Result = true");
            Assert.NotEmpty(writeRight.ReasonCodes);
            Assert.Contains(writeRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.RoleAccess));
            Assert.DoesNotContain(writeRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.PackageAccess));

            // Check for the "subscribe" right
            RightCheckDto signRight = result.Rights.FirstOrDefault(r => r.Right.Name.Contains("sign", StringComparison.InvariantCultureIgnoreCase));
            Assert.NotNull(signRight);
            Assert.False(signRight.Result, "The 'sign' right should have Result = false");
            Assert.NotEmpty(signRight.ReasonCodes);
            Assert.Contains(signRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.MissingRoleAccess));
            Assert.Contains(signRight.ReasonCodes, r => r.Equals(DelegationCheckReasonCode.MissingDelegationAccess));
        }

        [Fact]
        public async Task CheckResource_WithReadScope_ReturnsForbidden()
        {
            var client = CreateClient(TestData.MalinEmilie.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_FROMOTHERS_READ);

            var response = await client.GetAsync($"{Route}/resources/delegationcheck?party={TestData.DumboAdventures.Id}&resource=test-delegation-check-resource", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CheckResource_WithFromOthersWriteScope_ReturnsForbidden()
        {
            var client = CreateClient(TestData.MalinEmilie.Id, AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_FROMOTHERS_WRITE);

            var response = await client.GetAsync($"{Route}/resources/delegationcheck?party={TestData.DumboAdventures.Id}&resource=test-delegation-check-resource", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }

    #endregion
}
