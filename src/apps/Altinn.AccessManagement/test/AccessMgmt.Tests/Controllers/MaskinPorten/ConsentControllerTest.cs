using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Altinn.AccessManagement.Core.Clients.Interfaces;
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AccessMgmt.Tests.Controllers.MaskinPorten
{
    /// <summary>
    /// Tests for maskinporten controller for consent
    /// </summary>
    public class ConsentControllerTest(WebApplicationFixture fixture) : IClassFixture<WebApplicationFixture>
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

        [Fact]
        public async Task GetConsent_CreatedExpired_BadRequest()
        {
            Guid requestId = Guid.Parse("e2071c55-6adf-487b-af05-9198a230ed44");
            IConsentRepository repositgo = Fixture.Services.GetRequiredService<IConsentRepository>();
            await repositgo.CreateRequest(await GetRequest(requestId), ConsentPartyUrn.PartyUuid.Create(Guid.Parse("8ef5e5fa-94e1-4869-8635-df86b6219181")), default);

            HttpClient client = GetTestClient();
            string url = $"/accessmanagement/api/v1/maskinporten/consent/lookup/";

            string token = PrincipalUtil.GetOrgToken(null, "810419512", "altinn:maskinporten/consent.read");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            ConsentLookup consentLookup = new ConsentLookup()
            {
                Id = requestId,
                From = ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse("01025161013")),
                To = ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse("810419512"))
            };

            HttpResponseMessage response = await client.PostAsJsonAsync(url, consentLookup);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.NotNull(responseContent);
            AltinnMultipleProblemDetails problemDetails = JsonSerializer.Deserialize<AltinnMultipleProblemDetails>(responseContent, _jsonOptions);
            Assert.Equal(StdProblemDescriptors.ErrorCodes.MultipleProblems, problemDetails.ErrorCode);
            Assert.Equal(2, problemDetails.Problems.Count());
            Assert.Equal(Problems.ConsentExpired.ErrorCode, problemDetails.Problems.ToList()[0].ErrorCode);
            Assert.Equal(Problems.ConsentNotAccepted.ErrorCode, problemDetails.Problems.ToList()[1].ErrorCode);
        }

        [Fact]
        public async Task GetConsent_Valid()
        {
            Guid requestId = Guid.Parse("e2071c55-6adf-487b-af05-9198a230ed44");
            IConsentRepository repositgo = Fixture.Services.GetRequiredService<IConsentRepository>();
            ConsentRequest request = await GetRequest(requestId);
            request.ValidTo = DateTime.UtcNow.AddDays(10);
            await repositgo.CreateRequest(request, ConsentPartyUrn.PartyUuid.Create(Guid.Parse("8ef5e5fa-94e1-4869-8635-df86b6219181")), default);
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
            await repositgo.AcceptConsentRequest(requestId, Guid.NewGuid(), consentContextExternal.ToCore());

            HttpClient client = GetTestClient();
            string url = $"/accessmanagement/api/v1/maskinporten/consent/lookup/";

            string token = PrincipalUtil.GetOrgToken(null, "810419512", "altinn:maskinporten/consent.read");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            ConsentLookup consentLookup = new ConsentLookup()
            {
                Id = requestId,
                From = ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse("01025161013")),
                To = ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse("810419512"))
            };

            HttpResponseMessage response = await client.PostAsJsonAsync(url, consentLookup);
            string responseContent = await response.Content.ReadAsStringAsync();
            ConsentInfoMaskinporten consentInfo = JsonSerializer.Deserialize<ConsentInfoMaskinporten>(responseContent, _jsonOptions);
            Assert.Equal(2, consentInfo.ConsentRights.Count());
        }

        [Fact]
        public async Task GetConsent_Created_BadRequest()
        {
            Guid requestId = Guid.Parse("e2071c55-6adf-487b-af05-9198a230ed44");
            IConsentRepository repositgo = Fixture.Services.GetRequiredService<IConsentRepository>();
            ConsentRequest request = await GetRequest(requestId);
            request.ValidTo = DateTime.UtcNow.AddDays(10);
            await repositgo.CreateRequest(request, ConsentPartyUrn.PartyUuid.Create(Guid.Parse("8ef5e5fa-94e1-4869-8635-df86b6219181")), default);

            HttpClient client = GetTestClient();
            string url = $"/accessmanagement/api/v1/maskinporten/consent/lookup/";

            string token = PrincipalUtil.GetOrgToken(null, "810419512", "altinn:maskinporten/consent.read");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            ConsentLookup consentLookup = new ConsentLookup()
            {
                Id = requestId,
                From = ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse("01025161013")),
                To = ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse("810419512"))
            };

            HttpResponseMessage response = await client.PostAsJsonAsync(url, consentLookup);
            string responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(responseContent);
            AltinnMultipleProblemDetails problemDetails = JsonSerializer.Deserialize<AltinnMultipleProblemDetails>(responseContent, _jsonOptions);
            Assert.Equal(Problems.ConsentNotAccepted.ErrorCode, problemDetails.ErrorCode);
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
