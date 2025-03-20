using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AccessMgmt.Tests.Mocks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Tests;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.Authorization.Api.Models.Consent;
using Altinn.Common.AccessToken.Services;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AccessMgmt.Tests.Controllers.Enduser
{
    public class ConsentControllerTestEnduser : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Concent controller test
        /// </summary>
        public ConsentControllerTestEnduser(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Test case: Get consent request
        /// Scenario: User is authenticated and is the same person that has been request to accept the request
        /// User is authorized for all rights in the consent request
        /// </summary>
        [Fact]
        public async Task GetConsentRequest()
        {
            Guid requestId = Guid.Parse("e2071c55-6adf-487b-af05-9198a230ed44");

            HttpClient client = GetTestClient();
            string token = PrincipalUtil.GetToken(20001337, 50003899, 2, Guid.Parse("00000000-0000-0000-0005-000000003899"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/enduser/consent/request/{requestId.ToString()}");
            string responseText = response.Content.ReadAsStringAsync().Result;
            ConsentRequestDetailsExternal consentRequest = await response.Content.ReadFromJsonAsync<ConsentRequestDetailsExternal>();
            Assert.StartsWith("{\"id", responseText);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(requestId, consentRequest.Id);
            Assert.True(consentRequest.ConsentRights.Count > 0);
            Assert.Equal("27099450067", consentRequest.From.ValueSpan);
            Assert.Equal("810419512", consentRequest.To.ValueSpan);
            Assert.Equal("urn:altinn:resource", consentRequest.ConsentRights[0].Resource[0].Type);
        }

        private HttpClient GetTestClient()
        {
            HttpClient client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IConsentRepository, ConsentRepositoryMock>();
                    services.AddSingleton<IPartiesClient, PartiesClientMock>();
                    services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                    services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();
                    services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                    services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
                    services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}
