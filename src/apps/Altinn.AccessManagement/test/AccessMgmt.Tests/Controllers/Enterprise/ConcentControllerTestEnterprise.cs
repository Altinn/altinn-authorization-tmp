using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Altinn.AccessManagement.Tests;
using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Core.Models.Register;
using Altinn.Authorization.ProblemDetails;
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
                From = ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse("01025602168")),
                To = ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse("910194143")),
                ValidTo = DateTimeOffset.UtcNow.AddDays(1),
                ConsentRights = new List<ConsentRightExternal>
                {
                    new ConsentRightExternal
                    {
                        Action = new List<string> { "read" },
                        Resource = new List<ConsentResourceAttributeExternal>
                        {
                            new ConsentResourceAttributeExternal
                            {
                                Type = "urn:altinn:resource",
                                Value = "skd_inntektsopplsyniung"
                            }
                        },
                        MetaData = new Dictionary<string, string>
                        {
                            { "INNTEKTSAAR", "ADSF" }
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
            Assert.Single(consentInfo.ConsentRights[0].MetaData);
            Assert.Equal(consentRequest.ValidTo.Minute, consentInfo.ValidTo.Minute);
            Assert.Equal(consentRequest.ValidTo.Second, consentInfo.ValidTo.Second);
            Assert.Equal(consentRequest.ConsentRights[0].Action.Count(), consentInfo.ConsentRights[0].Action.Count());
            Assert.Equal(consentRequest.ConsentRights[0].Action[0], consentInfo.ConsentRights[0].Action[0]);
            Assert.Equal(consentRequest.ConsentRights[0].MetaData["INNTEKTSAAR"], consentInfo.ConsentRights[0].MetaData["INNTEKTSAAR"]);
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
