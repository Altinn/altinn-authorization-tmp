using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.TestUtils;
using Altinn.AccessManagement.TestUtils.Data;
using Altinn.AccessManagement.TestUtils.Fixtures;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;

namespace Altinn.AccessManagement.Enduser.Api.Tests.Controllers;

public class RequestControllerTest
{
    public const string Route = "accessmanagement/api/v1/enduser/request";

    // AccessManagementEnduserFeatureFlags.ControllerConnections is internal — use the string value directly.
    // private const string FeatureFlag = "AccessManagement.Enduser.Connections";

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

    #region POST /package — Create a pending package request as enduser

    /// <summary>
    /// <see cref="Altinn.AccessManagement.Api.Enduser.Controllers.RequestController.CreatePackageRequest"/>
    /// </summary>
    public class CreatePackageRequest : IClassFixture<ApiFixture>
    {
        public CreatePackageRequest(ApiFixture fixture)
        {
            Fixture = fixture;
            //Fixture.WithEnabledFeatureFlag(FeatureFlag);
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task CreatePackageRequest_WithValidInput_ReturnsPendingRequest()
        {
            var client = CreateClient(Fixture, TestEntities.PersonPaula.Id);
            var from = TestEntities.OrganizationNordisAS.Id;
            var to = TestEntities.PersonPaula.Id;
            var packageId = PackageConstants.Agriculture.Id;

            var response = await client.PostAsync(
                $"{Route}/package?party={to}&from={from}&to={to}&packageId={packageId}",
                null,
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
    /// Mirrors <c>PackageRequest_FullLifecycle_DraftToPendingToAccepted</c> from RequestServiceTests
    /// but exercises the HTTP layer via ApiFixture.
    /// </summary>
    public class PackageRequestLifecycle : IClassFixture<ApiFixture>
    {
        private static readonly Guid PendingPackageRequestId = Guid.Parse("01960003-0000-7000-8000-000000000001");

        public PackageRequestLifecycle(ApiFixture fixture)
        {
            Fixture = fixture;
            //Fixture.WithEnabledFeatureFlag(FeatureFlag);
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
        public async Task PackageRequest_GetRequests_ContainsPendingRequest()
        {
            var client = CreateClient(Fixture, TestEntities.OrganizationNordisAS.Id);
            var from = TestEntities.OrganizationNordisAS.Id;
            var to = TestEntities.PersonPaula.Id;

            var response = await client.GetAsync(
                $"{Route}?from={from}&to={to}&party={from}",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseJson = await client.GetFromJsonAsync<PaginatedResult<RequestDto>>(
                $"{Route}?from={from}&to={to}&party={from}",
                TestContext.Current.CancellationToken);

            Assert.Contains(responseJson.Items, item => item.Id == PendingPackageRequestId);
        }

        [Fact]
        public async Task PackageRequest_FullLifecycle_PendingToRejected()
        {
            var client = CreateClient(Fixture, TestEntities.OrganizationNordisAS.Id);

            // 1. Enduser fetches the pending request
            var getResponse = await client.GetAsync(
                $"{Route}?from={TestEntities.OrganizationNordisAS.Id}&to={TestEntities.PersonPaula.Id}&party={TestEntities.OrganizationNordisAS.Id}",
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            // 2. Enduser rejects
            var rejectResponse = await client.PutAsync(
                $"{Route}/{PendingPackageRequestId}/reject",
                null,
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);

            var json = await rejectResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            using var doc = JsonDocument.Parse(json);
            Assert.Equal((int)RequestStatus.Rejected, doc.RootElement.GetProperty("status").GetInt32());
        }
    }

    #endregion
}
