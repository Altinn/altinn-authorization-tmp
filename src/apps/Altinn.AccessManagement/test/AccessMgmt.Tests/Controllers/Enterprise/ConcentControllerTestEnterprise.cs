using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Tests;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Core.Models.Register;
using Altinn.Authorization.ProblemDetails;
using Altinn.Common.AccessToken.Services;
using AltinnCore.Authentication.JwtCookie;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AccessMgmt.Tests.Controllers.Enterprise
{
    /// <summary>
    /// Tests for maskinporten controller for consent
    /// </summary>
    public class ConcentControllerTestEnterprise(WebApplicationFixture fixture) : IClassFixture<WebApplicationFixture>
    {
        private WebApplicationFactory<Program> Fixture { get; } = fixture.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<IPartiesClient, PartiesClientMock>();
                services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();
                services.AddSingleton<IResourceRegistryClient, ResourceRegistryClientMock>();
                services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
                services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
            });
        });

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Test get consent. Expect a consent in response
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateConsentRequest_Valid()
        {
            ConsentRequestExternal consentRequest = new ConsentRequestExternal
            {
                From = ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse("27099450067")),
                To = ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse("810419512")),
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
                                Value = "ttd_inntektsopplysninger"
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

        /// <summary>
        /// Test get consent. Expect a consent in response
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateConsentRequest_ValidTwin()
        {
            ConsentRequestExternal consentRequest = new ConsentRequestExternal
            {
                From = ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse("27099450067")),
                To = ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse("810419512")),
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
                                Value = "ttd_inntektsopplysninger"
                            }
                        },
                        MetaData = new Dictionary<string, string>
                        {
                            { "INNTEKTSAAR", "ADSF" }
                        }
                    },
                    new ConsentRightExternal
                    {
                        Action = new List<string> { "read" },
                        Resource = new List<ConsentResourceAttributeExternal>
                        {
                            new ConsentResourceAttributeExternal
                            {
                                Type = "urn:altinn:resource",
                                Value = "ttd_skattegrunnlag"
                            }
                        },
                        MetaData = new Dictionary<string, string>
                        {
                            { "fraOgMed", "ADSF" },
                            { "tilOgMed", "ADSF" }
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
            Assert.Equal(2, consentInfo.ConsentRights.Count);
            Assert.Single(consentInfo.ConsentRights[0].MetaData);
            Assert.Equal(consentRequest.ValidTo.Minute, consentInfo.ValidTo.Minute);
            Assert.Equal(consentRequest.ValidTo.Second, consentInfo.ValidTo.Second);
            Assert.Equal(consentRequest.ConsentRights[0].Action.Count(), consentInfo.ConsentRights[0].Action.Count());
            Assert.Equal(consentRequest.ConsentRights[0].Action[0], consentInfo.ConsentRights[0].Action[0]);
            Assert.Equal(consentRequest.ConsentRights[0].MetaData["INNTEKTSAAR"], consentInfo.ConsentRights[0].MetaData["INNTEKTSAAR"]);
        }

        /// <summary>
        /// Scenario: Enterprise tries to add consentrequest with missing metadata for the resource
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateConsentRequest_MissingMetadata()
        {
            ConsentRequestExternal consentRequest = new ConsentRequestExternal
            {
                From = ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse("27099450067")),
                To = ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse("810419512")),
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
                                Value = "ttd_inntektsopplysninger"
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
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(responseContent);
            AltinnValidationProblemDetails problemDetails = JsonSerializer.Deserialize<AltinnValidationProblemDetails>(responseContent, _jsonOptions);

            Assert.Equal(StdProblemDescriptors.ErrorCodes.ValidationError, problemDetails.ErrorCode);
            Assert.Single(problemDetails.Errors);
            Assert.Equal("AM.VLD-00013", problemDetails.Errors.ToList()[0].ErrorCode.ToString());
            Assert.Equal("Missing required metadata for consentright", problemDetails.Errors.ToList()[0].Detail.ToString());
            Assert.Equal("/consentRight/0/Metadata/inntektsaar", problemDetails.Errors.ToList()[0].Paths[0]);
        }


        /// <summary>
        /// T
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateConsentRequest_WrongNamingMetadata()
        {
            ConsentRequestExternal consentRequest = new ConsentRequestExternal
            {
                From = ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse("27099450067")),
                To = ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse("810419512")),
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
                                Value = "ttd_inntektsopplysninger"
                            }
                        },
                        MetaData = new Dictionary<string, string>
                        {
                            { "INNTEKTSAAR", "ADSF" }
                        }
                    },
                    new ConsentRightExternal
                    {
                        Action = new List<string> { "read" },
                        Resource = new List<ConsentResourceAttributeExternal>
                        {
                            new ConsentResourceAttributeExternal
                            {
                                Type = "urn:altinn:resource",
                                Value = "ttd_skattegrunnlag"
                            }
                        },
                        MetaData = new Dictionary<string, string>
                        {
                            { "fraOgMed", "ADSF" },
                            { "tilOgMedwrong", "ADSF" }
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
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(responseContent);
            AltinnValidationProblemDetails problemDetails = JsonSerializer.Deserialize<AltinnValidationProblemDetails>(responseContent, _jsonOptions);

            Assert.Equal(StdProblemDescriptors.ErrorCodes.ValidationError, problemDetails.ErrorCode);
            Assert.Equal(2, problemDetails.Errors.Count);
            Assert.Equal("AM.VLD-00011", problemDetails.Errors.ToList()[0].ErrorCode.ToString());
            Assert.Equal("Unknown consent metaddata.", problemDetails.Errors.ToList()[0].Detail.ToString());
            Assert.Equal("/consentRight/1/Metadata/tilogmedwrong", problemDetails.Errors.ToList()[0].Paths[0]);
            Assert.Equal("AM.VLD-00013", problemDetails.Errors.ToList()[1].ErrorCode.ToString());
            Assert.Equal("Missing required metadata for consentright", problemDetails.Errors.ToList()[1].Detail.ToString());
            Assert.Equal("/consentRight/1/Metadata/tilogmed", problemDetails.Errors.ToList()[1].Paths[0]);
        }

        /// <summary>
        /// Scenario: Tries to 
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateConsentRequest_MissingRights()
        {
            ConsentRequestExternal consentRequest = new ConsentRequestExternal
            {
                From = ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse("27099450067")),
                To = ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse("810419512")),
                ValidTo = DateTimeOffset.UtcNow.AddDays(1),
                ConsentRights = new List<ConsentRightExternal>
                {
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
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(responseContent);
            AltinnValidationProblemDetails problemDetails = JsonSerializer.Deserialize<AltinnValidationProblemDetails>(responseContent, _jsonOptions);

            Assert.Equal(StdProblemDescriptors.ErrorCodes.ValidationError, problemDetails.ErrorCode);
            Assert.Single(problemDetails.Errors);
            Assert.Equal("AM.VLD-00009", problemDetails.Errors.ToList()[0].ErrorCode.ToString());
        }



        /// <summary>
        /// Scenario: Enterprise uses valid personidentifer but the person does not exist. It is not in register
        /// Expected: 400 Bad Request after validation in consent controller. There is one validation erorr with given code.
        /// </summary>
        [Fact]
        public async Task CreateConsentRequest_FromIsNonExistingPerson()
        {
            ConsentRequestExternal consentRequest = new ConsentRequestExternal
            {
                From = ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse("01014922047")),
                To = ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse("810419512")),
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
                                Value = "ttd_inntektsopplysninger"
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
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(responseContent);
            AltinnValidationProblemDetails problemDetails = JsonSerializer.Deserialize<AltinnValidationProblemDetails>(responseContent, _jsonOptions);

            Assert.Equal(StdProblemDescriptors.ErrorCodes.ValidationError, problemDetails.ErrorCode);
            Assert.Single(problemDetails.Errors);
            Assert.Equal("AM.VLD-00006", problemDetails.Errors.ToList()[0].ErrorCode.ToString());
        }

        private HttpClient GetTestClient()
        {
            HttpClient client = Fixture.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}
