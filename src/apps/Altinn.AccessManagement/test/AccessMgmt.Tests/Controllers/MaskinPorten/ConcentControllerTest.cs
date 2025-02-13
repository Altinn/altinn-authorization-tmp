using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Altinn.AccessManagement.Api.Maskinporten.Controllers;
using Altinn.AccessManagement.Controllers;
using Altinn.AccessManagement.Tests;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace AccessMgmt.Tests.Controllers.MaskinPorten
{

    /// <summary>
    /// Tests for maskinporten controller for consent
    /// </summary>
    public class ConcentControllerTest : IClassFixture<CustomWebApplicationFactory<ConcentController>>
    {
        private readonly CustomWebApplicationFactory<ConcentController> _factory;

        /// <summary>
        /// Concent controller test
        /// </summary>
        public ConcentControllerTest(CustomWebApplicationFactory<ConcentController> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetConcent()
        {
            HttpClient client = GetTestClient();
            string url = $"/accessmanagment/api/maskinporten/concent/lookup/?id={Guid.NewGuid()}&from=01017512345&to=12312432545";
            HttpResponseMessage response = await client.GetAsync(url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.NotNull(responseContent);
            Assert.Contains("concent", responseContent);
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
