using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Altinn.AccessManagement.Api.Maskinporten.Models.Concent;
using Altinn.AccessManagement.Tests;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace AccessMgmt.Tests.Controllers.MaskinPorten
{
    /// <summary>
    /// Tests for maskinporten controller for consent
    /// </summary>
    public class ConcentControllerTest : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Concent controller test
        /// </summary>
        public ConcentControllerTest(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Test get consent. Expect a consent in response
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetConcent()
        {
            HttpClient client = GetTestClient();
            string url = $"/accessmanagement/api/v1/maskinporten/consent/lookup/?id={Guid.NewGuid()}&from=01017512345&to=12312432545";
            HttpResponseMessage response = await client.GetAsync(url);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responseContent);
            ConsentInfoMaskinporten consentInfo = JsonSerializer.Deserialize<ConsentInfoMaskinporten>(responseContent, _jsonOptions);
            Assert.Single(consentInfo.ConsentRights);
        }

        private HttpClient GetTestClient()
        {
            HttpClient client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                });
            }).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}
