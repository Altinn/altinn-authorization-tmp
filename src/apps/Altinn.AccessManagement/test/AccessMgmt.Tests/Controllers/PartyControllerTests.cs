using System.Net;
using System.Net.Http.Json;
using Altinn.AccessManagement.Core.Models.Party;
using Altinn.AccessManagement.Tests;

using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.Authorization.Api.Contracts.Party;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Altinn.AccessManagement.Api.Internal.IntegrationTests.Controllers
{
    [Collection("Internal PartyController Test")]
    public class PartyControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public PartyControllerTests(WebApplicationFactory<Program> factory)
        {
            // TODO: Set up a correct WebApplicationFactory that spins up a default db to run the test in, that can be scrapped afterwards to ensure equal result each time. WebApplicationFactory//CustomWebApplicationFactory
            _factory = factory;
        }

        [Fact]
        public async Task AddParty_ValidParty_ReturnsOkAndTrue()
        {
            // Arrange
            var client = _factory.CreateClient();
            var party = new PartyBaseDto
            {
                PartyUuid = Guid.NewGuid(),
                EntityType = "Systembruker",
                EntityVariantType = "System",
                DisplayName = "Test User"
            };

            // Act
            var response = await client.PostAsJsonAsync("/accessmanagement/api/v1/internal/party", party);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<AddPartyResultDto>();
            Assert.True(result.PartyCreated);
        }

        [Fact]
        public async Task AddParty_ValidParty_ReturnsOkAndFalse()
        {
            // Arrange
            var client = _factory.CreateClient();
            var party = new PartyBaseDto
            {
                PartyUuid = Guid.NewGuid(),
                EntityType = "Systembruker",
                EntityVariantType = "System",
                DisplayName = "Test User"
            };

            // Act
            var response = await client.PostAsJsonAsync("/accessmanagement/api/v1/internal/party", party);
            response = await client.PostAsJsonAsync("/accessmanagement/api/v1/internal/party", party);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<AddPartyResultDto>();
            Assert.False(result.PartyCreated);
        }
    }
}
