using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Altinn.AccessManagement.Api.Enterprise.Models.Consent;
using Altinn.AccessManagement.Api.Maskinporten.Models.Concent;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Tests;
using Altinn.Authorization.ProblemDetails;
using Altinn.Register.Core.Parties;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace AccessMgmt.Tests.Controllers.Enterprise
{

    /// <summary>
    /// Tests for maskinporten controller for consent
    /// </summary>
    public class ConcentControllerTestEnterprise : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Concent controller test
        /// </summary>
        public ConcentControllerTestEnterprise(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Test get consent. Expect a consent in response
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateConsentRequest()
        {
            ConsentRequestExternal consentRequest = new ConsentRequestExternal
            {
                From = Altinn.AccessManagement.Api.Enterprise.Models.Consent.ConsentPartyUrnExternal2.PersonId.Create(PersonIdentifier.Parse("01025602168")),
                To = Altinn.AccessManagement.Api.Enterprise.Models.Consent.ConsentPartyUrnExternal2.OrganizationId.Create(OrganizationNumber.Parse("910194143")),
                ValidTo = DateTimeOffset.Now.AddDays(1),
                ConsentRights = new List<Altinn.AccessManagement.Api.Enterprise.Models.Consent.ConsentRightExternal2>
                {
                    new Altinn.AccessManagement.Api.Enterprise.Models.Consent.ConsentRightExternal2
                    {
                        Action = new List<string> { "read" },
                        Resource = new List<Altinn.AccessManagement.Api.Enterprise.Models.Consent.ConsentResourceAttributeExternal2>
                        {
                            new Altinn.AccessManagement.Api.Enterprise.Models.Consent.ConsentResourceAttributeExternal2
                            {
                                Type = "urn:altinn:resource",
                                Value = "skd_inntektsopplsyniung"
                            }
                        }
                    }
                },
                Requestmessage = new Dictionary<string, string>
                {
                    { "en", "Please approve this consent request" }
                }
            };

            HttpClient client = GetTestClient();
            string url = $"/accessmanagement/api/v1/enterprise/consent/request/";

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.PostAsync(url, new StringContent(JsonSerializer.Serialize(consentRequest, _jsonOptions), Encoding.UTF8, "application/json"));
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(responseContent);
            ConsentRequestDetailsExternal consentInfo = JsonSerializer.Deserialize<ConsentRequestDetailsExternal>(responseContent, _jsonOptions);
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
