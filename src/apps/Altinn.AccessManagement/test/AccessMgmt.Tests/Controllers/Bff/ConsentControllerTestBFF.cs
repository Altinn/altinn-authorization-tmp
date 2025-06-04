﻿using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Register;
using Altinn.Authorization.ProblemDetails;
using Altinn.Common.AccessToken.Services;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AccessMgmt.Tests.Controllers.Internal
{
    public class ConsentControllerTestBFF(WebApplicationFixture fixture) : IClassFixture<WebApplicationFixture>
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
                services.AddSingleton<IProfileClient, ProfileClientMock>();
            });
        });

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <summary>
        /// Test case: Get consent request
        /// Scenario: User is authenticated and is the same person that has been request to accept the request
        /// User is authorized for all rights in the consent request
        /// </summary>
        [Fact]
        public async Task GetConsentRequest()
        {
            Guid requestId = Guid.Parse("e2071c55-6adf-487b-af05-9198a230ed44");

            IConsentRepository repositgo = Fixture.Services.GetRequiredService<IConsentRepository>();
            await repositgo.CreateRequest(await GetRequest(requestId), ConsentPartyUrn.PartyUuid.Create(Guid.Parse("8ef5e5fa-94e1-4869-8635-df86b6219181")), default);
            HttpClient client = GetTestClient();
            string token = PrincipalUtil.GetToken(20001337, 50003899, 2, Guid.Parse("d5b861c8-8e3b-44cd-9952-5315e5990cf5"), AuthzConstants.SCOPE_PORTAL_ENDUSER);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/bff/consentrequests/{requestId.ToString()}");
            string responseText = await response.Content.ReadAsStringAsync();
            ConsentRequestDetailsExternal consentRequest = await response.Content.ReadFromJsonAsync<ConsentRequestDetailsExternal>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(requestId, consentRequest.Id);
            Assert.True(consentRequest.ConsentRights.Count > 0);
            Assert.Equal("01025161013", consentRequest.From.ValueSpan);
            Assert.Equal("810419512", consentRequest.To.ValueSpan);  // TODO FIx
            Assert.Equal("https:///www.urlfromsavedreqest.com", consentRequest.RedirectUrl);  // TODO FI
            Assert.Equal("urn:altinn:resource", consentRequest.ConsentRights[0].Resource[0].Type);
        }

        [Fact]
        public async Task AcceptRequest_Valid()
        {
            Guid requestId = Guid.Parse("e2071c55-6adf-487b-af05-9198a230ed44");
            IConsentRepository repositgo = Fixture.Services.GetRequiredService<IConsentRepository>();
            await repositgo.CreateRequest(await GetRequest(requestId), ConsentPartyUrn.PartyUuid.Create(Guid.Parse("8ef5e5fa-94e1-4869-8635-df86b6219181")), default);
            HttpClient client = GetTestClient();
            string token = PrincipalUtil.GetToken(20001337, 50003899, 2, Guid.Parse("d5b861c8-8e3b-44cd-9952-5315e5990cf5"), AuthzConstants.SCOPE_PORTAL_ENDUSER);

            ConsentContextExternal consentContextExternal = new ConsentContextExternal
            {
                Language = "nb",
                Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv",
                ConsentContextResources = new List<ResourceContextExternal>
               {
                   new() 
                   {
                       ResourceId = "urn:altinn:resource:ttd_skattegrunnlag",
                       Language = "nb",
                       Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv"
                   },
                   new() 
                   {
                       ResourceId = "urn:altinn:resource:ttd_inntektsopplysninger",
                       Language = "nb",
                       Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv"
                   }
               }
            };

            // Serialize the object to JSON
            string jsonContent = JsonSerializer.Serialize(consentContextExternal);

            // Create HttpContent from the JSON string
            HttpContent httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.PostAsync($"accessmanagement/api/v1/bff/consentrequests/{requestId.ToString()}/accept/", httpContent);
            string responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            ConsentRequestDetailsExternal consentInfo = await response.Content.ReadFromJsonAsync<ConsentRequestDetailsExternal>();
            Assert.Equal(2, consentInfo.ConsentRequestEvents.Count);
            Assert.Equal(ConsentRequestEventTypeExternal.Created, consentInfo.ConsentRequestEvents[0].EventType);
            Assert.Equal(ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse("810419512")), consentInfo.ConsentRequestEvents[0].PerformedBy);
            Assert.Equal(ConsentRequestEventTypeExternal.Accepted, consentInfo.ConsentRequestEvents[1].EventType);
        }

        [Fact]
        public async Task AcceptRequestWithRequiredDelegator_Valid()
        {
            Guid requestId = Guid.Parse("a4253d59-b40f-409a-a3f7-c6395f065192");
            IConsentRepository repositgo = Fixture.Services.GetRequiredService<IConsentRepository>();
            await repositgo.CreateRequest(await GetRequest(requestId), ConsentPartyUrn.PartyUuid.Create(Guid.Parse("8ef5e5fa-94e1-4869-8635-df86b6219181")), default);
            HttpClient client = GetTestClient();
            string token = PrincipalUtil.GetToken(20001337, 50003899, 2, Guid.Parse("d5b861c8-8e3b-44cd-9952-5315e5990cf5"), AuthzConstants.SCOPE_PORTAL_ENDUSER);

            ConsentContextExternal consentContextExternal = new ConsentContextExternal
            {
                Language = "nb",
                Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv",
                ConsentContextResources = new List<ResourceContextExternal>
               {
                   new()
                   {
                       ResourceId = "urn:altinn:resource:ttd_skattegrunnlag",
                       Language = "nb",
                       Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv"
                   },
                   new()
                   {
                       ResourceId = "urn:altinn:resource:ttd_inntektsopplysninger",
                       Language = "nb",
                       Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv"
                   }
               }
            };

            // Serialize the object to JSON
            string jsonContent = JsonSerializer.Serialize(consentContextExternal);

            // Create HttpContent from the JSON string
            HttpContent httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.PostAsync($"accessmanagement/api/v1/bff/consentrequests/{requestId.ToString()}/accept/", httpContent);
            string responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            ConsentRequestDetailsExternal consentInfo = await response.Content.ReadFromJsonAsync<ConsentRequestDetailsExternal>();
            Assert.Equal(2, consentInfo.ConsentRequestEvents.Count);
            Assert.Equal(ConsentRequestEventTypeExternal.Created, consentInfo.ConsentRequestEvents[0].EventType);
            Assert.Equal(ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse("810419512")), consentInfo.ConsentRequestEvents[0].PerformedBy);
            Assert.Equal(ConsentRequestEventTypeExternal.Accepted, consentInfo.ConsentRequestEvents[1].EventType);
        }

        [Fact]
        public async Task AcceptRequest_AlreadyRejected()
        {
            Guid performedBy = Guid.Parse("d5b861c8-8e3b-44cd-9952-5315e5990cf5");
            Guid requestId = Guid.Parse("e2071c55-6adf-487b-af05-9198a230ed44");
            IConsentRepository repositgo = Fixture.Services.GetRequiredService<IConsentRepository>();
            await repositgo.CreateRequest(await GetRequest(requestId), ConsentPartyUrn.PartyUuid.Create(Guid.Parse("8ef5e5fa-94e1-4869-8635-df86b6219181")), default);
            await repositgo.RejectConsentRequest(requestId,performedBy, default);
            HttpClient client = GetTestClient();
            string token = PrincipalUtil.GetToken(20001337, 50003899, 2, performedBy, AuthzConstants.SCOPE_PORTAL_ENDUSER);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            ConsentContextExternal consentContextExternal = new ConsentContextExternal
            {
                Language = "nb",
                Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv",
                ConsentContextResources = new List<ResourceContextExternal>
               {
                   new()
                   {
                       ResourceId = "urn:altinn:resource:ttd_skattegrunnlag",
                       Language = "nb",
                       Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv"
                   },
                   new()
                   {
                       ResourceId = "urn:altinn:resource:ttd_inntektsopplysninger",
                       Language = "nb",
                       Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv"
                   }
               }
            };

            // Serialize the object to JSON
            string jsonContent = JsonSerializer.Serialize(consentContextExternal);

            // Create HttpContent from the JSON string
            HttpContent httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync($"accessmanagement/api/v1/bff/consentrequests/{requestId.ToString()}/accept/", httpContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            AltinnValidationProblemDetails problemDetails = JsonSerializer.Deserialize<AltinnValidationProblemDetails>(responseContent, _jsonOptions);
            Assert.Equal("AM-00002", problemDetails.ErrorCode.ToString());
        }

        [Fact]
        public async Task RejectRequest_Valid()
        {
            Guid requestId = Guid.Parse("e2071c55-6adf-487b-af05-9198a230ed44");
            IConsentRepository repositgo = Fixture.Services.GetRequiredService<IConsentRepository>();
            await repositgo.CreateRequest(await GetRequest(requestId), ConsentPartyUrn.PartyUuid.Create(Guid.Parse("8ef5e5fa-94e1-4869-8635-df86b6219181")), default);
            HttpClient client = GetTestClient();
            string token = PrincipalUtil.GetToken(20001337, 50003899, 2, Guid.Parse("d5b861c8-8e3b-44cd-9952-5315e5990cf5"), AuthzConstants.SCOPE_PORTAL_ENDUSER);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.PostAsync($"accessmanagement/api/v1/bff/consentrequests/{requestId.ToString()}/reject/", null);
            string responseText = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Test case: End user rejects a consent request that earlier has been accepted
        /// Expected result: The consent request is rejected and the consent request event is created. Total 3 events
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RejectRequest_AlreadyAccepted()
        {
            Guid performedBy = Guid.Parse("d5b861c8-8e3b-44cd-9952-5315e5990cf5");
            Guid requestId = Guid.Parse("e2071c55-6adf-487b-af05-9198a230ed44");
            IConsentRepository repositgo = Fixture.Services.GetRequiredService<IConsentRepository>();
            await repositgo.CreateRequest(await GetRequest(requestId), ConsentPartyUrn.PartyUuid.Create(Guid.Parse("8ef5e5fa-94e1-4869-8635-df86b6219181")), default);
            ConsentContextExternal consentContextExternal = new ConsentContextExternal
            {
                Language = "nb",
                Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv",
                ConsentContextResources = new List<ResourceContextExternal>
               {
                   new()
                   {
                       ResourceId = "urn:altinn:resource:ttd_skattegrunnlag",
                       Language = "nb",
                       Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv"
                   },
                   new()
                   {
                       ResourceId = "urn:altinn:resource:ttd_inntektsopplysninger",
                       Language = "nb",
                       Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv"
                   }
               }
            };
            await repositgo.AcceptConsentRequest(requestId, performedBy, consentContextExternal.ToCore());
            HttpClient client = GetTestClient();
            string token = PrincipalUtil.GetToken(20001337, 50003899, 2, Guid.Parse("d5b861c8-8e3b-44cd-9952-5315e5990cf5"), AuthzConstants.SCOPE_PORTAL_ENDUSER);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.PostAsync($"accessmanagement/api/v1/bff/consentrequests/{requestId.ToString()}/reject/", null);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(responseContent);
            AltinnMultipleProblemDetails problemDetails = JsonSerializer.Deserialize<AltinnMultipleProblemDetails>(responseContent, _jsonOptions);

            Assert.Equal(Problems.ConsentCantBeRejected.ErrorCode, problemDetails.ErrorCode);
            Assert.Empty(problemDetails.Problems);
        }

        /// <summary>
        /// Test case: End user rejects a consent request that earlier has been accepted
        /// Expected result: The consent request is rejected and the consent request event is created. Total 3 events
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetConsent_Valid()
        {
            Guid performedBy = Guid.Parse("d5b861c8-8e3b-44cd-9952-5315e5990cf5");
            Guid requestId = Guid.Parse("e2071c55-6adf-487b-af05-9198a230ed44");
            IConsentRepository repositgo = Fixture.Services.GetRequiredService<IConsentRepository>();
            await repositgo.CreateRequest(await GetRequest(requestId), ConsentPartyUrn.PartyUuid.Create(Guid.Parse("8ef5e5fa-94e1-4869-8635-df86b6219181")), default);
            ConsentContextExternal consentContextExternal = new ConsentContextExternal
            {
                Language = "nb",
                Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv",
                ConsentContextResources = new List<ResourceContextExternal>
               {
                   new()
                   {
                       ResourceId = "urn:altinn:resource:ttd_skattegrunnlag",
                       Language = "nb",
                       Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv"
                   },
                   new()
                   {
                       ResourceId = "urn:altinn:resource:ttd_inntektsopplysninger",
                       Language = "nb",
                       Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv"
                   }
               }
            };
            await repositgo.AcceptConsentRequest(requestId, performedBy, consentContextExternal.ToCore());
            HttpClient client = GetTestClient();
            string token = PrincipalUtil.GetToken(20001337, 50003899, 2, Guid.Parse("d5b861c8-8e3b-44cd-9952-5315e5990cf5"), AuthzConstants.SCOPE_PORTAL_ENDUSER);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.GetAsync($"accessmanagement/api/v1/bff/consents/{requestId.ToString()}");
            string responseContent = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Consent consentRequest = await response.Content.ReadFromJsonAsync<Consent>();
            Assert.Equal(requestId, consentRequest.Id);
            Assert.True(consentRequest.ConsentRights.Count > 0);
            Assert.Equal("01025161013", consentRequest.From.ValueSpan);
            Assert.Equal("810419512", consentRequest.To.ValueSpan);  // TODO FIx
            Assert.Equal("urn:altinn:resource", consentRequest.ConsentRights[0].Resource[0].Type);
            Assert.Equal(consentContextExternal.Context, consentRequest.Context.Context);
        }

        /// <summary>
        /// Test case: End user revokes a consent request that earlier has been accepted
        /// Expected result: The consent request is revoked and the consent request event is created. Total 3 events
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RevokeRequest_Valid()
        {
            Guid performedBy = Guid.Parse("d5b861c8-8e3b-44cd-9952-5315e5990cf5");

            Guid requestId = Guid.Parse("e2071c55-6adf-487b-af05-9198a230ed44");
            IConsentRepository repositgo = Fixture.Services.GetRequiredService<IConsentRepository>();
            await repositgo.CreateRequest(await GetRequest(requestId), ConsentPartyUrn.PartyUuid.Create(Guid.Parse("8ef5e5fa-94e1-4869-8635-df86b6219181")), default);
            ConsentContextExternal consentContextExternal = new ConsentContextExternal
            {
                Language = "nb",
                Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv",
                ConsentContextResources = new List<ResourceContextExternal>
               {
                   new()
                   {
                       ResourceId = "urn:altinn:resource:ttd_skattegrunnlag",
                       Language = "nb",
                       Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv"
                   },
                   new()
                   {
                       ResourceId = "urn:altinn:resource:ttd_inntektsopplysninger",
                       Language = "nb",
                       Context = "Ved å samtykke til denne teksten så gir du samtykke til at vi kan dele dataene dine med oss selv"
                   }
               }
            };
            await repositgo.AcceptConsentRequest(requestId, performedBy, consentContextExternal.ToCore());

            HttpClient client = GetTestClient();
            string token = PrincipalUtil.GetToken(20001337, 50003899, 2, performedBy, AuthzConstants.SCOPE_PORTAL_ENDUSER);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.PostAsync($"accessmanagement/api/v1/bff/consents/{requestId.ToString()}/revoke/", null);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            ConsentRequestDetailsExternal consentInfo = JsonSerializer.Deserialize<ConsentRequestDetailsExternal>(responseContent, _jsonOptions);
            Assert.Equal(3,consentInfo.ConsentRequestEvents.Count);
            Assert.Equal(ConsentRequestEventTypeExternal.Created, consentInfo.ConsentRequestEvents[0].EventType);
            Assert.Equal(ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse("810419512")), consentInfo.ConsentRequestEvents[0].PerformedBy);
            Assert.Equal(ConsentRequestEventTypeExternal.Accepted, consentInfo.ConsentRequestEvents[1].EventType);
            Assert.Equal(ConsentRequestEventTypeExternal.Revoked, consentInfo.ConsentRequestEvents[2].EventType);
        }

        [Fact]
        public async Task RevokeRequest_NotAccepted()
        {
            Guid performedBy = Guid.Parse("d5b861c8-8e3b-44cd-9952-5315e5990cf5");

            Guid requestId = Guid.Parse("e2071c55-6adf-487b-af05-9198a230ed44");
            IConsentRepository repositgo = Fixture.Services.GetRequiredService<IConsentRepository>();
            await repositgo.CreateRequest(await GetRequest(requestId), ConsentPartyUrn.PartyUuid.Create(Guid.Parse("8ef5e5fa-94e1-4869-8635-df86b6219181")), default);

            HttpClient client = GetTestClient();
            string token = PrincipalUtil.GetToken(20001337, 50003899, 2, performedBy, AuthzConstants.SCOPE_PORTAL_ENDUSER);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            HttpResponseMessage response = await client.PostAsync($"accessmanagement/api/v1/bff/consents/{requestId.ToString()}/revoke/", null);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(responseContent);
            AltinnMultipleProblemDetails problemDetails = JsonSerializer.Deserialize<AltinnMultipleProblemDetails>(responseContent, _jsonOptions);

            Assert.Equal(Problems.ConsentCantBeRevoked.ErrorCode, problemDetails.ErrorCode);
        }

        private HttpClient GetTestClient()
        {
            HttpClient client = Fixture.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private Task<ConsentRequest> GetRequest(Guid id)
        {
            Stream dataStream = File.OpenRead($"Data/Consent/consent_request_{id.ToString()}.json");
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            ConsentRequest result = JsonSerializer.Deserialize<ConsentRequest>(dataStream, options);
            return Task.FromResult(result);
        }
    }
}
