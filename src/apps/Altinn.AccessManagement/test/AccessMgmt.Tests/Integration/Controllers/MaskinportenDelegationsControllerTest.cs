using System.Net;
using System.Net.Http.Headers;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Tests.Fixtures;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Util;
using Altinn.AccessManagement.TestUtils.Mocks;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Altinn.AccessManagement.Tests.Integration.Controllers
{
    /// <summary>
    /// Integration tests for the Maskinporten delegations lookup endpoint served from the ServiceOwner API.
    /// </summary>
    [IntegrationTest]
    public class MaskinportenDelegationsControllerTest : IClassFixture<AccessMgmtApiFixture>
    {
        private readonly AccessMgmtApiFixture _fixture;
        private readonly HttpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaskinportenDelegationsControllerTest"/> class.
        /// </summary>
        public MaskinportenDelegationsControllerTest(AccessMgmtApiFixture fixture)
        {
            _fixture = fixture;
            fixture.WithAppsettings(builder => builder.AddJsonFile("appsettings.test.json", optional: false));
            fixture.ConfigureServices(services =>
            {
                services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
                services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepositoryMock>();
                services.AddSingleton<IPolicyFactory, PolicyFactoryMock>();
                services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
                services.RemoveAll<IPublicSigningKeyProvider>();
                services.AddSingleton<IPublicSigningKeyProvider, PublicSigningKeyProviderMock>();
                services.RemoveAll<IPDP>();
                services.AddSingleton<IPDP, PepWithPDPAuthorizationMock>();
            });

            _client = _fixture.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [Fact]
        public async Task GetMaskinportenDelegations_NoToken_ReturnsUnauthorized()
        {
            HttpResponseMessage response = await _client.GetAsync("accessmanagement/api/v1/maskinporten/delegations/?scope=some:scope", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetMaskinportenDelegations_AdminToken_NoParameters_ReturnsBadRequest()
        {
            string token = PrincipalUtil.GetOrgToken("DIGDIR", "991825827", "altinn:maskinporten/delegations.admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await _client.GetAsync("accessmanagement/api/v1/maskinporten/delegations/", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetMaskinportenDelegations_AdminToken_InvalidSupplierOrg_ReturnsBadRequest()
        {
            string token = PrincipalUtil.GetOrgToken("DIGDIR", "991825827", "altinn:maskinporten/delegations.admin");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await _client.GetAsync("accessmanagement/api/v1/maskinporten/delegations/?supplierorg=123&scope=some:scope", TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
