using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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

namespace Altinn.AccessManagement.Api.Internal.IntegrationTests.Controllers
{
    [Collection("Internal PartyController Test")]
    public class PartyControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public PartyControllerTests(WebApplicationFactory<Program> factory)
        {
            // TODO: Set up a correct WebApplicationFactory that spins up a default db to run the test in, that can be scrapped afterwards to ensure equal result each time. WebApplicationFactory//CustomWebApplicationFactory
            _factory = factory;
            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                    services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        [Fact]
        public async Task AddParty_AuthorizationFail_InvalidIssuer()
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/accessmanagement/api/v1/internal/party")
            {
                Content = JsonContent.Create(new PartyBaseDto
                {
                    PartyUuid = Guid.NewGuid(),
                    EntityType = "Systembruker",
                    EntityVariantType = "System",
                    DisplayName = "Test User"
                }),
                Headers =
                {
                    { "PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "authentication") } // Invalid issuer for this endpoint
                }
            };

            // Act
            HttpResponseMessage response = await _client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AddParty_AuthorizationFail_InvalidAppClaim()
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/accessmanagement/api/v1/internal/party")
            {
                Content = JsonContent.Create(new PartyBaseDto
                {
                    PartyUuid = Guid.NewGuid(),
                    EntityType = "Organization",
                    EntityVariantType = "System",
                    DisplayName = "Test User"
                }),
                Headers =
                {
                    { "PlatformAccessToken", PrincipalUtil.GetAccessToken("platform", "unittest") } // Valid issuer, but invalid app claim for this endpoint
                }
            };

            // Act
            HttpResponseMessage response = await _client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AddParty_AuthorizationOk_InvalidEntityType()
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/accessmanagement/api/v1/internal/party")
            {
                Content = JsonContent.Create(new PartyBaseDto
                {
                    PartyUuid = Guid.NewGuid(),
                    EntityType = "Organization", // Invalid entity type (at this time only allows for creating type: SystemUser)
                    EntityVariantType = "System",
                    DisplayName = "Test User"
                }),
                Headers =
                {
                    { "PlatformAccessToken", PrincipalUtil.GetAccessToken("platform", "authentication") }
                }
            };

            // Act
            HttpResponseMessage response = await _client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            AltinnProblemDetails actual = JsonSerializer.Deserialize<AltinnProblemDetails>(await response.Content.ReadAsStringAsync(), options);
            Assert.Equal("The Entitytype is not supported", actual.Detail);
        }

        [Fact]
        public async Task AddParty_AuthorizationOk_InvalidEntityVariantType()
        {
            // Arrange
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/accessmanagement/api/v1/internal/party")
            {
                Content = JsonContent.Create(new PartyBaseDto
                {
                    PartyUuid = Guid.NewGuid(),
                    EntityType = "Systembruker",
                    EntityVariantType = "BEDR", // Invalid variant type for SystemUser
                    DisplayName = "Test User"
                }),
                Headers =
                {
                    { "PlatformAccessToken", PrincipalUtil.GetAccessToken("platform", "authentication") }
                }
            };

            // Act
            HttpResponseMessage response = await _client.SendAsync(httpRequestMessage);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            AltinnProblemDetails actual = JsonSerializer.Deserialize<AltinnProblemDetails>(await response.Content.ReadAsStringAsync(), options);
            Assert.Equal("The EntityVariant is not found or not valid for the given EntityType", actual.Detail);
        }

        /* ToDo: Revisit these tests when factory and test container database is set up correctly.
        [Fact]
        public async Task AddParty_ValidParty_ReturnsOkAndTrue()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("platform", "authentication"));
            var party = new PartyBaseDto
            {
                PartyUuid = Guid.NewGuid(),
                EntityType = "Systembruker",
                EntityVariantType = "System",
                DisplayName = "Test User"
            };

            // Act
            var response = await client.PostAsJsonAsync("/accessmanagement/api/v1/internal/party", party);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<AddPartyResultDto>();
            Assert.True(result.PartyCreated);
        }

        [Fact]
        public async Task AddParty_ValidParty_PartyUuidExists_ReturnsOkAndFalse()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("platform", "authentication"));
            var party = new PartyBaseDto
            {
                PartyUuid = Guid.NewGuid(),
                EntityType = "Systembruker",
                EntityVariantType = "System",
                DisplayName = "Test User"
            };

            // Act
            var response = await client.PostAsJsonAsync("/accessmanagement/api/v1/internal/party", party);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<AddPartyResultDto>();
            Assert.False(result.PartyCreated);
        }
        */
    }
}
