using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.Integration.Platform.Register;

/// <summary>
/// Client for interacting with endpoint that stream roles (ER-roles).
/// </summary>
public partial class RegisterClient
{
    /// <inheritdoc/>
    public async Task<IAsyncEnumerable<PlatformResponse<PageStream<RoleModel>>>> StreamRoles(IEnumerable<string> fields, string nextPage = null, CancellationToken cancellationToken = default)
    {
        List<Action<HttpRequestMessage>> request = [
            RequestComposer.WithHttpVerb(HttpMethod.Get),
            RequestComposer.WithSetUri(Options.Value.Endpoint, "/register/api/v2/internal/parties/external-roles/assignments/events/stream"),
            RequestComposer.WithSetUri(nextPage),
            RequestComposer.WithAppendQueryParam("fields", fields),
            RequestComposer.WithPlatformAccessToken(AccessTokenGenerator, "access-management")
        ];

        var response = await HttpClient.SendAsync(RequestComposer.New([.. request]), cancellationToken);

        return new PaginatorStream<RoleModel>(HttpClient, response, request);
    }
}

/// <summary>
/// Represents a party with various personal and organizational details.
/// This model is used to deserialize JSON responses containing party information.
/// </summary>
[ExcludeFromCodeCoverage]
public class RoleModel
{
    [JsonPropertyName("versionId")]
    public int VersionId { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("roleSource")]
    public string RoleSource { get; set; }

    [JsonPropertyName("roleIdentifier")]
    public string RoleIdentifier { get; set; }

    [JsonPropertyName("toParty")]
    public string ToParty { get; set; }

    [JsonPropertyName("fromParty")]
    public string FromParty { get; set; }
}
