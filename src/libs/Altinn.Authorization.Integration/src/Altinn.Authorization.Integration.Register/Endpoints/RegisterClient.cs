using Altinn.Authorization.Integration.Register.Extensions;
using Altinn.Common.AccessTokenClient.Services;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.Integration.Register;

public partial class RegisterClient
{
    internal const string HttpClientName = "Altinn Register";

    private HttpClient HttpClient => HttpClientFactory.CreateClient(HttpClientName);

    private IHttpClientFactory HttpClientFactory { get; }

    public IOptions<AltinnRegisterOptions> Options { get; }

    private readonly IAccessTokenGenerator _accessTokenGenerator;

    /// <summary>
    /// RegisterClient
    /// </summary>
    /// <param name="httpClientFactory">Http client factory</param>
    /// <param name="accessTokenGenerator"></param>
    /// <param name="options"></param>
    public RegisterClient(
        IHttpClientFactory httpClientFactory,
        IAccessTokenGenerator accessTokenGenerator,
        IOptions<AltinnRegisterOptions> options)
    {
        HttpClientFactory = httpClientFactory;
        Options = options;
        _accessTokenGenerator = accessTokenGenerator;
    }

    private void AddAuthorization(HttpRequestMessage request)
    {
        var token = _accessTokenGenerator.GenerateAccessToken("platform", "access-management");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Add("PlatformAccessToken", token);
        }
    }
}
