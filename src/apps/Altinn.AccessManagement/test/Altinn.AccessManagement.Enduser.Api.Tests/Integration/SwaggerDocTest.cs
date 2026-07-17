using System.Net;
using System.Text.Json;
using Altinn.AccessManagement.TestUtils.Fixtures;

namespace Altinn.AccessManagement.Enduser.Api.Tests.Integration;

/// <summary>
/// Regression tests for the generated swagger documents. The v1 document is
/// extracted by APIM under the name "v1" at <c>/swagger/v1/swagger.json</c>,
/// so both its URL and its content must stay stable. Swagger is only mapped
/// in the Development environment, which is the default environment for the
/// <see cref="ApiFixture"/> test host.
/// </summary>
public class SwaggerDocTest
{
    public const string ClientDelegationV1Route = "/accessmanagement/api/v1/enduser/clientdelegations";

    /// <summary>
    /// Fetches a swagger document and returns the response together with the
    /// path keys of the parsed document.
    /// </summary>
    private static async Task<(HttpResponseMessage Response, List<string> Paths)> GetSwaggerDoc(HttpClient client, string docName)
    {
        var response = await client.GetAsync($"/swagger/{docName}/swagger.json", TestContext.Current.CancellationToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var doc = JsonDocument.Parse(body);
        var paths = doc.RootElement.GetProperty("paths").EnumerateObject().Select(p => p.Name).ToList();
        return (response, paths);
    }

    /// <summary>
    /// Tests for the v1 swagger document.
    /// </summary>
    [IntegrationTest]
    public class V1Doc : IClassFixture<ApiFixture>
    {
        public V1Doc(ApiFixture fixture)
        {
            Client = fixture.BuildConfiguration();
        }

        private HttpClient Client { get; }

        [Fact]
        public async Task GetSwaggerJson_V1Doc_Returns200WithClientDelegationV1Paths()
        {
            var (_, paths) = await GetSwaggerDoc(Client, "v1");

            paths.Should().Contain(p => p.StartsWith($"{ClientDelegationV1Route}/", StringComparison.Ordinal));
        }

        [Fact]
        public async Task GetSwaggerJson_V1Doc_Returns200WithoutV2Paths()
        {
            var (_, paths) = await GetSwaggerDoc(Client, "v1");

            paths.Should().NotContain(p => p.Contains("/v2/", StringComparison.Ordinal));
        }
    }
}
