using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.TestUtils;
using Altinn.AccessManagement.TestUtils.Data;
using Altinn.AccessManagement.TestUtils.Fixtures;
using Altinn.AccessManagement.TestUtils.Mocks;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Core.Utils.Helper;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Authorization.Api.Contracts.AccessManagement.Request;
using Altinn.Authorization.ProblemDetails;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessManagement.Enduser.Api.Tests.Integration.Controllers.V2;

/// <summary>
/// Tests for the v2 request endpoints, where a resource request can name the rights (right keys) being requested.
///
/// Uses <see cref="TestData.TestdirektoratetCorrespondenceService"/>, whose policy in the test data gives read and
/// subscribe to a role (delegable) and write only to the service owner org (exists, but not delegable).
/// The keys are calculated from that policy, so the tests do not depend on hardcoded hashes.
/// </summary>
public class RequestControllerTest
{
    public const string Route = "accessmanagement/api/v2/enduser/request";

    private static readonly string Resource = TestData.TestdirektoratetCorrespondenceService.RefId;

    private static HttpClient CreateClient(ApiFixture fixture, Guid partyUuid)
    {
        var client = fixture.Server.CreateClient();
        var token = TestTokenGenerator.CreateToken(new ClaimsIdentity("mock"), claims =>
        {
            claims.Add(new Claim(AltinnCoreClaimTypes.PartyUuid, partyUuid.ToString()));
            claims.Add(new Claim("scope", $"{AuthzConstants.SCOPE_ENDUSER_REQUESTS_READ} {AuthzConstants.SCOPE_ENDUSER_REQUESTS_WRITE}"));
        });
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return client;
    }

    // KnutVik is a board member of BakerJohnsen (seeded via TestData.Assignments), so the parties are connected.
    private static string CreateResourceRequestUrl() =>
        $"{Route}/resource?party={TestData.KnutVik.Id}&to={TestData.BakerJohnsen.Id}&resource={Resource}";

    private static async Task<(List<string> DelegableKeys, List<string> NonDelegableKeys)> GetPolicyKeys()
    {
        XacmlPolicy policy = await new PolicyRetrievalPointMock().GetPolicyAsync(Resource, TestContext.Current.CancellationToken);
        var delegableKeys = DelegationCheckHelper.CalculateRightKeys(policy.Rules.ElementAt(0), Resource).ToList();
        var nonDelegableKeys = DelegationCheckHelper.CalculateRightKeys(policy.Rules.ElementAt(1), Resource).Except(delegableKeys).ToList();
        return (delegableKeys, nonDelegableKeys);
    }

    private static async Task<AltinnValidationProblemDetails> ReadProblem(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        return JsonSerializer.Deserialize<AltinnValidationProblemDetails>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    #region POST — Create resource request with right keys

    [IntegrationTest]
    public class CreateResourceRequest : IClassFixture<ApiFixture>
    {
        public CreateResourceRequest(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.ConfigureServices(services =>
            {
                services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
            });
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableEnduserRequestV2Api);
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task WithDelegableRightKeys_ReturnsPendingWithStoredRightKeys()
        {
            var (delegableKeys, _) = await GetPolicyKeys();
            var client = CreateClient(Fixture, TestData.KnutVik.Id);

            var response = await client.PostAsJsonAsync(
                CreateResourceRequestUrl(),
                new RequestedRightKeysDto { RightKeys = delegableKeys },
                TestContext.Current.CancellationToken);

            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK but got {response.StatusCode}. Response body: {content}");

            var request = await response.Content.ReadFromJsonAsync<RequestDto>(TestContext.Current.CancellationToken);
            Assert.Equal(RequestStatus.Pending, request.Status);
            Assert.Equal(delegableKeys, request.RightKeys);

            // Reading the request back returns the stored keys
            var getResponse = await client.GetAsync($"{Route}?party={TestData.KnutVik.Id}&id={request.Id}", TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var fetched = await getResponse.Content.ReadFromJsonAsync<RequestDto>(TestContext.Current.CancellationToken);
            Assert.Equal(delegableKeys, fetched.RightKeys);
        }

        [Fact]
        public async Task WithOrganisationAsParty_ReturnsPendingWithStoredRightKeys()
        {
            // An organisation can ask for access too. MortenDahl is managing director of SvendsenAutomobil
            // and acts on its behalf; SvendsenAutomobil is rightholder for KaosMagicDesignAndArts
            // (seeded via TestData.Assignments), so the parties are connected.
            var (delegableKeys, _) = await GetPolicyKeys();
            var client = CreateClient(Fixture, TestData.MortenDahl.Id);

            var response = await client.PostAsJsonAsync(
                $"{Route}/resource?party={TestData.SvendsenAutomobil.Id}&to={TestData.KaosMagicDesignAndArts.Id}&resource={Resource}",
                new RequestedRightKeysDto { RightKeys = delegableKeys },
                TestContext.Current.CancellationToken);

            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK but got {response.StatusCode}. Response body: {content}");

            var request = await response.Content.ReadFromJsonAsync<RequestDto>(TestContext.Current.CancellationToken);
            Assert.Equal(RequestStatus.Pending, request.Status);
            Assert.Equal(TestData.SvendsenAutomobil.Id, request.From.Id);
            Assert.Equal(TestData.KaosMagicDesignAndArts.Id, request.To.Id);
            Assert.Equal(delegableKeys, request.RightKeys);

            // Reading the request back as the organisation returns the stored keys
            var getResponse = await client.GetAsync($"{Route}?party={TestData.SvendsenAutomobil.Id}&id={request.Id}", TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var fetched = await getResponse.Content.ReadFromJsonAsync<RequestDto>(TestContext.Current.CancellationToken);
            Assert.Equal(delegableKeys, fetched.RightKeys);
        }

        [Fact]
        public async Task WithoutBody_ReturnsPendingWithoutRightKeys()
        {
            // A different requester than the tests above, so this does not get back their pending request
            // (an existing pending request for the same parties and resource is returned as is).
            // HildeStrand is chair of the board of BakerJohnsen (seeded via TestData.Assignments).
            var client = CreateClient(Fixture, TestData.HildeStrand.Id);

            var response = await client.PostAsync(
                $"{Route}/resource?party={TestData.HildeStrand.Id}&to={TestData.BakerJohnsen.Id}&resource={Resource}",
                null,
                TestContext.Current.CancellationToken);

            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.True(response.StatusCode == HttpStatusCode.OK, $"Expected OK but got {response.StatusCode}. Response body: {content}");

            var request = JsonSerializer.Deserialize<RequestDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.Equal(RequestStatus.Pending, request.Status);
            Assert.Null(request.RightKeys);

            // The field is left out of the JSON instead of being written as null
            using var json = JsonDocument.Parse(content);
            Assert.False(json.RootElement.TryGetProperty("rightKeys", out _));
        }

        [Fact]
        public async Task WithUnknownRightKey_Returns400RightKeyNotFound()
        {
            var client = CreateClient(Fixture, TestData.KnutVik.Id);

            var response = await client.PostAsJsonAsync(
                CreateResourceRequestUrl(),
                new RequestedRightKeysDto { RightKeys = ["unknown-right-key"] },
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await ReadProblem(response);
            var error = Assert.Single(problem.Errors);
            Assert.Equal(ValidationErrors.RightKeyNotFound.ErrorCode, error.ErrorCode);
        }

        [Fact]
        public async Task WithNonDelegableRightKey_Returns400RightKeyNotDelegable()
        {
            var (_, nonDelegableKeys) = await GetPolicyKeys();
            var client = CreateClient(Fixture, TestData.KnutVik.Id);

            var response = await client.PostAsJsonAsync(
                CreateResourceRequestUrl(),
                new RequestedRightKeysDto { RightKeys = nonDelegableKeys },
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await ReadProblem(response);
            var error = Assert.Single(problem.Errors);
            Assert.Equal(ValidationErrors.RightKeyNotDelegable.ErrorCode, error.ErrorCode);
        }

        [Fact]
        public async Task GetSentRequests_UnderV2Route_Returns200()
        {
            var client = CreateClient(Fixture, TestData.KnutVik.Id);

            var response = await client.GetAsync($"{Route}/sent?party={TestData.KnutVik.Id}", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    #endregion

    #region Feature flag off

    [IntegrationTest]
    public class V2ApiDisabled : IClassFixture<ApiFixture>
    {
        public V2ApiDisabled(ApiFixture fixture)
        {
            Fixture = fixture;
            Fixture.WithEnabledFeatureFlag(AccessMgmtFeatureFlags.EnableRequestAssignmentResource);
        }

        public ApiFixture Fixture { get; }

        [Fact]
        public async Task CreateResourceRequest_Returns404()
        {
            var client = CreateClient(Fixture, TestData.KnutVik.Id);

            var response = await client.PostAsync(CreateResourceRequestUrl(), null, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    #endregion
}
