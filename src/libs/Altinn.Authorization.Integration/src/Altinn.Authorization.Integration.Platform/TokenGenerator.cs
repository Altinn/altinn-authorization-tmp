using System.Security.Cryptography.X509Certificates;
using Altinn.Authorization.Integration.Platform.Extensions;
using Altinn.Common.AccessTokenClient.Services;
using Azure.Core;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.Integration.Platform;

internal class TokenGenerator
{
    internal class TokenGeneratorTestTool(IOptions<AltinnIntegrationOptions> options, IHttpClientFactory httpClientFactory) : ITokenGenerator
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
                RequestComposer.WithSetUri(options.PlatformAccessToken.TestTool.Endpoint, "/api/GetPlatformAccessToken"),
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

    internal class TokenGeneratorKeyVault(
            IOptions<AltinnIntegrationOptions> options,
            IAzureClientFactory<SecretClient> azureSecretClientFactory,
            IAzureClientFactory<CertificateClient> azureCertificateClientFactory,
            IAccessTokenGenerator platformKeyVault) : ITokenGenerator
    {
        public const string ServiceKey = "AltinnPlatformKeyVault";

        private SemaphoreSlim Semaphore { get; } = new(1, 1);

        private IAccessTokenGenerator KeyVaultGenerator { get; } = platformKeyVault;

        private IOptions<AltinnIntegrationOptions> Options { get; } = options;

        private IAzureClientFactory<SecretClient> AzureSecretClientFactory { get; } = azureSecretClientFactory;

        private IAzureClientFactory<CertificateClient> AzureCertificateClientFactory { get; } = azureCertificateClientFactory;

        private string AccessToken { get; set; } = string.Empty;

        private DateTimeOffset AccessTokenExpires { get; set; } = DateTime.MinValue;

        public async Task<string> Create(CancellationToken cancellationToken = default)
        {
            var options = Options.Value;
            return await Create(options.PlatformAccessToken.Issuer, options.PlatformAccessToken.App, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<string> Create(string issuer, string app, CancellationToken cancellationToken = default)
        {
            await Semaphore.WaitAsync(cancellationToken);

            try
            {
                if (string.IsNullOrEmpty(AccessToken) || AccessTokenExpires < DateTime.UtcNow)
                {
                    return await GenerateAndSetJWTToken(issuer, app, cancellationToken);
                }
            }
            finally
            {
                Semaphore.Release();
            }

            return AccessToken;
        }

        private async Task<string> GenerateAndSetJWTToken(string issuer, string app, CancellationToken cancellationToken)
        {
            try
            {
                var options = Options.Value;
                var certClient = AzureCertificateClientFactory.CreateClient(ServiceKey);
                var secretClient = AzureSecretClientFactory.CreateClient(ServiceKey);
                await foreach (var cert in certClient.GetPropertiesOfCertificateVersionsAsync("JWTCertificate", cancellationToken))
                {
                    if ((cert.Enabled == true && cert.ExpiresOn == null) || cert.ExpiresOn >= DateTime.UtcNow)
                    {
                        var secret = await secretClient.GetSecretAsync(cert.Name, cert.Version, cancellationToken);
                        var pkcs12 = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(secret.Value.Value), null, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                        AccessToken = KeyVaultGenerator.GenerateAccessToken(issuer, app, pkcs12);
                        AccessTokenExpires = cert.ExpiresOn ?? secret.Value.Properties.ExpiresOn ?? DateTime.UtcNow.AddSeconds(options.PlatformAccessToken.KeyVault.CacheTimeout);
                        return AccessToken;
                    }
                }
            }
            catch (Azure.RequestFailedException ex)
            {
                throw new UnauthorizedAccessException("Failed to retrieve certificate from key vault", ex);
            }

            throw new UnauthorizedAccessException("Couldn't find any cert 'JWTCertificate' that's enabled or not expired");
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
