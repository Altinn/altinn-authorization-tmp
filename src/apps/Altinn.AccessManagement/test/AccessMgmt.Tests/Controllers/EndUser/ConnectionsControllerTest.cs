using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Xml;
using Altinn.AccessManagement.Api.Enduser.Controllers;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.TestUtils;
using Altinn.AccessManagement.TestUtils.Data;
using Altinn.AccessManagement.TestUtils.Fixtures;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.ABAC.Utils;
using Altinn.Authorization.ABAC.Xacml;
using Microsoft.Extensions.DependencyInjection;
using Moq;

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
            var result = JsonSerializer.Deserialize<PaginatedResult<ConnectionDto>>(data);

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
            var result = JsonSerializer.Deserialize<PaginatedResult<ConnectionDto>>(data);

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
            });
            Fixture.EnsureSeedOnce(db =>
            {
                var resourceType = db.ResourceTypes.FirstOrDefault(t => t.Id == Guid.Parse("0195efb8-7c80-7f26-817a-50893176320d"));
                if (resourceType is null)
                {
                    resourceType = new ResourceType() { Id = Guid.Parse("0195efb8-7c80-7f26-817a-50893176320d"), Name = "Test" };
                    db.ResourceTypes.Add(resourceType);
                }

                Resource testResource = new Resource()
                {
                    Name = "Dialogs for sickness benefits",
                    Description = "The service is used to send and receive dialogues in Dialogporten about new sick leaves, submitted applications, requests for income reports, and receipts for submitted income reports.",
                    RefId = "nav_sykepenger_dialog",
                    TypeId = resourceType.Id,
                    ProviderId = ProviderConstants.Altinn3.Id,
                };

                db.Resources.Add(testResource);

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

        private HttpClient CreateClient(params string[] scopes)
        {
            var client = Fixture.Server.CreateClient();
            var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
            {
                claims.Add(new Claim(AltinnCoreClaimTypes.PartyUuid, TestEntities.PersonPaula.Id.ToString()));
                claims.Add(new Claim("scope", string.Join(" ", scopes)));
            });
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            return client;
        }

        [Fact]
        public async Task CheckResource_WithWriteToOthersScope_ReturnsNotForbidden()
        {
            HttpClient client = CreateClient(AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_TOOTHERS_WRITE);

            HttpResponseMessage response = await client.GetAsync($"{Route}/resources/delegationcheck?party={TestEntities.OrganizationNordisAS.Id}&resource=nav_sykepenger_dialog", TestContext.Current.CancellationToken);

            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CheckResource_WithReadScope_ReturnsForbidden()
        {
            var client = CreateClient(AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_FROMOTHERS_READ);

            var response = await client.GetAsync($"{Route}/resources/delegationcheck?party={TestEntities.OrganizationNordisAS.Id}&resource=test-delegation-check-resource", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CheckResource_WithFromOthersWriteScope_ReturnsForbidden()
        {
            var client = CreateClient(AuthzConstants.SCOPE_ENDUSER_CONNECTIONS_FROMOTHERS_WRITE);

            var response = await client.GetAsync($"{Route}/resources/delegationcheck?party={TestEntities.OrganizationNordisAS.Id}&resource=test-delegation-check-resource", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private static string GetResourceRegistryPolicyPath()
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(ConnectionsControllerTest).Assembly.Location).LocalPath);
            return Path.Combine(unitTestFolder, "..", "..", "..", "..", "AccessMgmt.Tests", "Data", "Xacml", "3.0", "ResourceRegistry");
        }
    }

    #endregion
}
