using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Util;
using Altinn.Authorization.Api.Contracts.Party;
using Altinn.Authorization.ProblemDetails;
using Altinn.Common.AccessToken.Services;

namespace Altinn.AccessManagement.Api.Internal.IntegrationTests.Controllers
{
    [Collection("Internal PartyController Test")]
    public class PartyControllerTests : IClassFixture<SharedWebApplicationFixture>
    {
        private readonly JsonSerializerOptions _options = new() { PropertyNameCaseInsensitive = true };
        private readonly HttpClient _client;

        public PartyControllerTests(SharedWebApplicationFixture fixture)
        {
            _client = fixture.GetClient();
            if (!_client.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
            {
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
        }

        [Fact]
        public async Task AddParty_AuthorizationFail_InvalidIssuer()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/accessmanagement/api/v1/internal/party")
            {
                Content = JsonContent.Create(new PartyBaseDto
                {
                    PartyUuid = Guid.NewGuid(),
                    EntityType = "Systembruker",
                    EntityVariantType = "StandardSystem",
                    DisplayName = "Test User"
                })
            };
            httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "authentication"));

            HttpResponseMessage response = await _client.SendAsync(httpRequestMessage);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AddParty_AuthorizationFail_InvalidAppClaim()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/accessmanagement/api/v1/internal/party")
            {
                Content = JsonContent.Create(new PartyBaseDto
                {
                    PartyUuid = Guid.NewGuid(),
                    EntityType = "Organization",
                    EntityVariantType = "StandardSystem",
                    DisplayName = "Test User"
                })
            };
            httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("platform", "unittest"));

            HttpResponseMessage response = await _client.SendAsync(httpRequestMessage);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AddParty_AuthorizationOk_InvalidEntityType()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/accessmanagement/api/v1/internal/party")
            {
                Content = JsonContent.Create(new PartyBaseDto
                {
                    PartyUuid = Guid.NewGuid(),
                    EntityType = "Organization", // Invalid entity type (only Systembruker allowed currently)
                    EntityVariantType = "StandardSystem",
                    DisplayName = "Test User"
                })
            };
            httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("platform", "authentication"));

            HttpResponseMessage response = await _client.SendAsync(httpRequestMessage);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            AltinnProblemDetails actual = JsonSerializer.Deserialize<AltinnProblemDetails>(await response.Content.ReadAsStringAsync(), _options);
            Assert.Equal("The Entitytype is not supported", actual.Detail);
        }

        [Fact]
        public async Task AddParty_AuthorizationOk_InvalidEntityVariantType()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/accessmanagement/api/v1/internal/party")
            {
                Content = JsonContent.Create(new PartyBaseDto
                {
                    PartyUuid = Guid.NewGuid(),
                    EntityType = "Systembruker",
                    EntityVariantType = "BEDR", // Invalid variant type for Systembruker
                    DisplayName = "Test User"
                })
            };
            httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("platform", "authentication"));

            HttpResponseMessage response = await _client.SendAsync(httpRequestMessage);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            AltinnProblemDetails actual = JsonSerializer.Deserialize<AltinnProblemDetails>(await response.Content.ReadAsStringAsync(), _options);
            Assert.Equal("The EntityVariant is not found or not valid for the given EntityType", actual.Detail);
        }

        [Fact]
        public async Task AddParty_ValidParty_StandardSystem_ReturnsOkAndTrue()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/accessmanagement/api/v1/internal/party")
            {
                Content = JsonContent.Create(new PartyBaseDto
                {
                    PartyUuid = Guid.NewGuid(),
                    EntityType = "Systembruker",
                    EntityVariantType = "StandardSystem",
                    DisplayName = "Test User"
                })
            };
            httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("platform", "authentication"));

            HttpResponseMessage response = await _client.SendAsync(httpRequestMessage);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<AddPartyResultDto>();
            Assert.True(result.PartyCreated);
        }

        [Fact]
        public async Task AddParty_ValidParty_AgentSystem_ReturnsOkAndTrue()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/accessmanagement/api/v1/internal/party")
            {
                Content = JsonContent.Create(new PartyBaseDto
                {
                    PartyUuid = Guid.NewGuid(),
                    EntityType = "Systembruker",
                    EntityVariantType = "AgentSystem",
                    DisplayName = "Test User"
                })
            };
            httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("platform", "authentication"));

            HttpResponseMessage response = await _client.SendAsync(httpRequestMessage);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<AddPartyResultDto>();
            Assert.True(result.PartyCreated);
        }
    }
}
