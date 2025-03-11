using Altinn.Authorization.Integration.Platform.Extensions;
using Altinn.Common.AccessTokenClient.Services;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.Integration.Platform;

public class TokenGenerator
{
    public class TokenGeneratorTestTool(IOptions<AltinnIntegrationOptions> options, IHttpClientFactory httpClientFactory) : ITokenGenerator
    {
        private IOptions<AltinnIntegrationOptions> Options { get; } = options;

        private IHttpClientFactory HttpClientFactory { get; } = httpClientFactory;

        /// <inheritdoc/>
        public async Task<string> Create(CancellationToken cancellationToken = default)
        {
            var options = Options.Value;
            return await Create(options.PlatformAccessToken.Issuer, options.PlatformAccessToken.App, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<string> Create(string issuer, string app, CancellationToken cancellationToken = default)
        {
            var options = Options.Value;

            var request = RequestComposer.New(
                RequestComposer.WithSetUri(options.PlatformAccessToken.TestTool.TokenGeneratorUrl),
                RequestComposer.WithHttpVerb(HttpMethod.Get),
                RequestComposer.WithAppendQueryParam("env", options.PlatformAccessToken.TestTool.Environment),
                RequestComposer.WithAppendQueryParam("ttl", 3600),
                RequestComposer.WithAppendQueryParam("app", app),
                RequestComposer.WithBasicAuth(options.PlatformAccessToken.TestTool.Username, options.PlatformAccessToken.TestTool.Password)
            );

            var client = CreateHttpClient(options.HttpClientName);

            var response = await client.SendAsync(request, cancellationToken);

            var result = ResponseComposer.Handle<string>(
                response,
                ResponseComposer.SetBodyAsStringResultIfSuccesful
            );

            if (!result.IsSuccessful)
            {
                throw new UnauthorizedAccessException("Failed to retrieve a successful token from the test tool token generator");
            }

            return result.Content;
        }

        private HttpClient CreateHttpClient(string HttpClientName)
        {
            if (string.IsNullOrEmpty(HttpClientName))
            {
                return HttpClientFactory.CreateClient();
            }

            return HttpClientFactory.CreateClient(HttpClientName);
        }
    }

    public class TokenGeneratorKeyVault(IOptions<AltinnIntegrationOptions> options, IAccessTokenGenerator platformKeyVault) : ITokenGenerator
    {
        private IAccessTokenGenerator KeyVaultGenerator { get; } = platformKeyVault;

        private IOptions<AltinnIntegrationOptions> Options { get; } = options;

        public const string ServiceKey = "AltinnPlatformKeyVault";

        public async Task<string> Create(CancellationToken cancellationToken = default)
        {
            var options = Options.Value;
            return await Create(options.PlatformAccessToken.Issuer, options.PlatformAccessToken.App, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<string> Create(string issuer, string app, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(KeyVaultGenerator.GenerateAccessToken(issuer, app));
        }
    }
}

public interface ITokenGenerator
{
    /// <summary>
    /// Creates a new Platform access token
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns></returns>
    Task<string> Create(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new Platform Access Token
    /// </summary>
    /// <param name="issuer">token issuer</param>
    /// <param name="app">app name</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns></returns>
    Task<string> Create(string issuer, string app, CancellationToken cancellationToken = default);
}
