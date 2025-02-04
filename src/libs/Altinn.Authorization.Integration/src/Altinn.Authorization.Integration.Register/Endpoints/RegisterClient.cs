using Altinn.Authorization.Integration.Register.Extensions;
using Altinn.Authorization.Integration.Register.Options;
using Altinn.Common.AccessTokenClient.Services;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.Integration.Register;

/// <summary>
/// Client for interacting with the Altinn Register service.
/// </summary>
public partial class RegisterClient
{
    /// <summary>
    /// The name of the HTTP client used to communicate with the Altinn Register service.
    /// </summary>
    internal const string HttpClientName = "Altinn Register";

    private HttpClient HttpClient => HttpClientFactory.CreateClient(HttpClientName);

    private IHttpClientFactory HttpClientFactory { get; }

    private IOptions<AltinnRegisterOptions> Options { get; }

    private readonly IAccessTokenGenerator _accessTokenGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterClient"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="accessTokenGenerator">Service for generating access tokens.</param>
    /// <param name="options">Configuration options for the Altinn Register service.</param>
    public RegisterClient(
        IHttpClientFactory httpClientFactory,
        IAccessTokenGenerator accessTokenGenerator,
        IOptions<AltinnRegisterOptions> options)
    {
        HttpClientFactory = httpClientFactory;
        Options = options;
        _accessTokenGenerator = accessTokenGenerator;
    }

    /// <summary>
    /// Adds the platform access token authorization headers to an HTTP request.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    private void AddAuthorization(HttpRequestMessage request)
    {
        //// var token = _accessTokenGenerator.GenerateAccessToken("platform", "access-management");
        var token = "eyJhbGciOiJSUzI1NiIsImtpZCI6IkJCQjA2MkM5ODI4NEZBRTYxOUNCMjlGRkYyQ0FBMDFGNUE3QzU2RjIiLCJ0eXAiOiJKV1QiLCJ4NWMiOiJCQkIwNjJDOTgyODRGQUU2MTlDQjI5RkZGMkNBQTAxRjVBN0M1NkYyIn0.eyJ1cm46YWx0aW5uOmFwcCI6ImFjY2Vzcy1tYW5hZ2VtZW50IiwiZXhwIjoxNzM4NTkxNjA3LCJpYXQiOjE3Mzg1ODgwMDcsImlzcyI6InBsYXRmb3JtIiwiYWN0dWFsX2lzcyI6ImFsdGlubi10ZXN0LXRvb2xzIiwibmJmIjoxNzM4NTg4MDA3fQ.gliNjnheFqnoKzMzdyYyApKhYSwKFLS1fXQXBvx0R3RQ7ONog--E2Pmb1jq_YcCT4vJTcArPPP4jcP2dYyjg8LQX0zbyG9VRUQndOCf0ZnRwrsZdYm5j4nG-eSSHF3xQWjVZBh63FipXJHowzV3JgNcm4A4woxtq_OIc2TFyggmvcXE77kjYo982zoUlTYSmhP0v-LexDFJbI7ItzBDWYPjt3Unaok_rIrC9eFoPIwgcomPGb827WoiN8LAu-bniiJl5dBVvuy8BmxmsLUxg0LYZ1QoC7tZGPM9WKJr8OTOglaZgUieHhPAnE9lrOSS7kPgLHQRZy-AKW6i35BEduw";
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Add("PlatformAccessToken", token);
        }
    }
}
