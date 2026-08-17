using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Altinn.AccessManagement.Api.Internal.Controllers;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.TestUtils;
using Altinn.AccessManagement.TestUtils.Data;
using Altinn.AccessManagement.TestUtils.Fixtures;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;

namespace Altinn.AccessManagement.Api.Internal.Tests.Controllers;

public class SystemUserClientDelegationControllerTest
{
    public const string Route = "accessmanagement/api/v1/internal/systemuserclientdelegation";

    public static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    #region GET accessmanagement/api/v1/internal/systemuserclientdelegation/clients

    /// <summary>
    /// <see cref="SystemUserClientDelegationController.GetClients(Guid, string[], string[], string, CancellationToken)"/>
    /// </summary>
    [IntegrationTest]
    public class GetClients : IClassFixture<ApiFixture>
    {
        /// <summary>
        /// The package identifiers the endpoint filters on are the last segment of the package URN.
        /// </summary>
        private static readonly string AccountantSalaryFilter = PackageConstants.AccountantSalary.Entity.Urn.Split(':')[^1];

        private static readonly string CustomsFilter = PackageConstants.Customs.Entity.Urn.Split(':')[^1];

        public GetClients(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.EnsureSeedOnce<GetClients>(db =>
            {
                // The accountant client holds the salary package through its role and the customs
                // package through a delegation on a separate rightholder relation.
                var accountantFromNordisToVerdiq = new Assignment()
                {
                    FromId = TestEntities.OrganizationNordisAS.Id,
                    ToId = TestEntities.OrganizationVerdiqAS.Id,
                    RoleId = RoleConstants.Accountant,
                };

                var rightholderFromNordisToVerdiq = new Assignment()
                {
                    FromId = TestEntities.OrganizationNordisAS.Id,
                    ToId = TestEntities.OrganizationVerdiqAS.Id,
                    RoleId = RoleConstants.Rightholder,
                };

                // The second client only holds the customs package.
                var rightholderFromOkernToVerdiq = new Assignment()
                {
                    FromId = TestEntities.OrganizationOkernBorettslag.Id,
                    ToId = TestEntities.OrganizationVerdiqAS.Id,
                    RoleId = RoleConstants.Rightholder,
                };

                db.Assignments.Add(accountantFromNordisToVerdiq);
                db.Assignments.Add(rightholderFromNordisToVerdiq);
                db.Assignments.Add(rightholderFromOkernToVerdiq);

                db.AssignmentPackages.Add(new()
                {
                    AssignmentId = rightholderFromNordisToVerdiq.Id,
                    PackageId = PackageConstants.Customs,
                });

                db.AssignmentPackages.Add(new()
                {
                    AssignmentId = rightholderFromOkernToVerdiq.Id,
                    PackageId = PackageConstants.Customs,
                });

                db.SaveChanges();
            });
        }

        public ApiFixture Fixture { get; }

        private HttpClient CreateClient()
        {
            var client = Fixture.Server.CreateClient();
            var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
            {
                claims.Add(new Claim("scope", AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_READ));
            });
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            return client;
        }

        private async Task<List<SystemuserClientDto>> GetClientList(HttpClient client, string query)
        {
            var response = await client.GetAsync($"{Route}/clients?party={TestEntities.OrganizationVerdiqAS.Id}{query}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var data = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            return JsonSerializer.Deserialize<List<SystemuserClientDto>>(data, SerializerOptions);
        }

        [Fact]
        public async Task ListClient_WithMultiplePackagesFilterAndPackageMatchAll_Returns200WithOnlyClientHoldingEveryFilterPackage()
        {
            var client = CreateClient();

            var clients = await GetClientList(client, $"&packages={AccountantSalaryFilter}&packages={CustomsFilter}&packageMatch=all");

            // The accountant client covers the filter through both ways in: the salary package
            // comes from the accountant role, the customs package from a delegation on the
            // rightholder relation. The other client only holds the customs package.
            var nordisClient = clients.FirstOrDefault(c => c.Party.Id == TestEntities.OrganizationNordisAS.Id);
            Assert.NotNull(nordisClient);
            Assert.DoesNotContain(clients, c => c.Party.Id == TestEntities.OrganizationOkernBorettslag.Id);

            var accountantAccess = nordisClient.Access.FirstOrDefault(a => a.Role == RoleConstants.Accountant.Entity.Code);
            Assert.NotNull(accountantAccess);
            Assert.Contains(AccountantSalaryFilter, accountantAccess.Packages);

            var rightholderAccess = nordisClient.Access.FirstOrDefault(a => a.Role == RoleConstants.Rightholder.Entity.Code);
            Assert.NotNull(rightholderAccess);
            Assert.Contains(CustomsFilter, rightholderAccess.Packages);

            // All is what this endpoint does without a packageMatch value.
            var defaultClients = await GetClientList(client, $"&packages={AccountantSalaryFilter}&packages={CustomsFilter}");

            Assert.Equal(
                defaultClients.Select(c => c.Party.Id).Order(),
                clients.Select(c => c.Party.Id).Order());
        }

        [Fact]
        public async Task ListClient_WithMultiplePackagesFilterAndPackageMatchAny_Returns200WithClientsHoldingAnyFilterPackage()
        {
            var client = CreateClient();

            var clients = await GetClientList(client, $"&packages={AccountantSalaryFilter}&packages={CustomsFilter}&packageMatch=any");

            Assert.Contains(clients, c => c.Party.Id == TestEntities.OrganizationNordisAS.Id);
            Assert.Contains(clients, c => c.Party.Id == TestEntities.OrganizationOkernBorettslag.Id);
        }

        [Fact]
        public async Task ListClient_WithSinglePackageFilterAndPackageMatchAll_Returns200WithSameClientsAsAnyMode()
        {
            var client = CreateClient();

            var allModeClients = await GetClientList(client, $"&packages={CustomsFilter}&packageMatch=all");

            Assert.Contains(allModeClients, c => c.Party.Id == TestEntities.OrganizationNordisAS.Id);
            Assert.Contains(allModeClients, c => c.Party.Id == TestEntities.OrganizationOkernBorettslag.Id);

            var anyModeClients = await GetClientList(client, $"&packages={CustomsFilter}&packageMatch=any");

            Assert.Equal(
                anyModeClients.Select(c => c.Party.Id).Order(),
                allModeClients.Select(c => c.Party.Id).Order());
        }

        [Fact]
        public async Task ListClient_WithUnknownPackageMatchValue_Returns400WithInvalidPackageMatchMessage()
        {
            var client = CreateClient();

            var response = await client.GetAsync($"{Route}/clients?party={TestEntities.OrganizationVerdiqAS.Id}&packages={CustomsFilter}&packageMatch=sometimes", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var data = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("packageMatch", data);
            Assert.Contains("any, all", data);
        }
    }
    #endregion
}
