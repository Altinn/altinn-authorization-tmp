using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AccessMgmt.Tests.Mocks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Tests;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.Authorization.Api.Models.Consent;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

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

        [Fact]
        public async Task GetConsentRequest()
        {
            Guid requestId = Guid.Parse("e2071c55-6adf-487b-af05-9198a230ed44");

            HttpClient client = GetTestClient();
            HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/enduser/consent/request/{requestId.ToString()}");
            ConsentRequestDetailsExternal consentRequest = await response.Content.ReadFromJsonAsync<ConsentRequestDetailsExternal>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private HttpClient GetTestClient()
        {
            HttpClient client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IConsentRepository, ConsentRepositoryMock>();
                    services.AddSingleton<IPartiesClient, PartiesClientMock>();
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}
