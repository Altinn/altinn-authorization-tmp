using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.Authorization.Api.Contracts.Party;
using Altinn.Authorization.ProblemDetails;
using Altinn.Common.AccessToken.Services;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.AccessManagement.Api.Internal.IntegrationTests.Controllers
{
    [Collection("Internal PartyController Test")]
    public class PartyControllerTests : IClassFixture<WebApplicationFixture>
    {
        private readonly JsonSerializerOptions options = new() { PropertyNameCaseInsensitive = true };
        private readonly WebApplicationFactory<Program> _fixture;

        public PartyControllerTests(WebApplicationFixture fixture)
        {
            _fixture = fixture.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                    services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();
                });
            });
        }

        private HttpClient GetClient()
        {
            var client = _fixture.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private async Task<HttpResponseMessage> SendAddPartyRequestAsync(string entityType, string entityVariantType, string displayName, string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/accessmanagement/api/v1/internal/party")
            {
                Content = JsonContent.Create(new PartyBaseDto
                {
                    PartyUuid = Guid.NewGuid(),
                    EntityType = entityType,
                    EntityVariantType = entityVariantType,
                    DisplayName = displayName
                })
            };
            request.Headers.Add("PlatformAccessToken", accessToken);
            return await GetClient().SendAsync(request);
        }

        [Theory]
        [InlineData("ttd", "authentication", "Systembruker", "StandardSystem", "Test User", HttpStatusCode.Unauthorized)]
        [InlineData("platform", "unittest", "Organization", "StandardSystem", "Test User", HttpStatusCode.Unauthorized)]
        public async Task AddParty_AuthorizationFail(string issuer, string appClaim, string entityType, string entityVariantType, string displayName, HttpStatusCode expectedStatus)
        {
            var accessToken = PrincipalUtil.GetAccessToken(issuer, appClaim);
            var response = await SendAddPartyRequestAsync(entityType, entityVariantType, displayName, accessToken);
            Assert.Equal(expectedStatus, response.StatusCode);
        }

        [Theory]
        [InlineData("Organization", "StandardSystem", "Test User", "platform", "authentication", HttpStatusCode.BadRequest, "The Entitytype is not supported")]
        [InlineData("Systembruker", "BEDR", "Test User", "platform", "authentication", HttpStatusCode.BadRequest, "The EntityVariant is not found or not valid for the given EntityType")]
        public async Task AddParty_ValidationFail(string entityType, string entityVariantType, string displayName, string issuer, string appClaim, HttpStatusCode expectedStatus, string expectedDetail)
        {
            var accessToken = PrincipalUtil.GetAccessToken(issuer, appClaim);
            var response = await SendAddPartyRequestAsync(entityType, entityVariantType, displayName, accessToken);
            Assert.Equal(expectedStatus, response.StatusCode);
            var actual = JsonSerializer.Deserialize<AltinnProblemDetails>(await response.Content.ReadAsStringAsync(), options);
            Assert.Equal(expectedDetail, actual.Detail);
        }

        [Theory]
        [InlineData("Systembruker", "StandardSystem", "Test User")]
        [InlineData("Systembruker", "AgentSystem", "Test User")]
        public async Task AddParty_ValidParty_ReturnsOkAndTrue(string entityType, string entityVariantType, string displayName)
        {
            var accessToken = PrincipalUtil.GetAccessToken("platform", "authentication");
            var response = await SendAddPartyRequestAsync(entityType, entityVariantType, displayName, accessToken);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<AddPartyResultDto>();
            Assert.True(result.PartyCreated);
        }
    }
}
