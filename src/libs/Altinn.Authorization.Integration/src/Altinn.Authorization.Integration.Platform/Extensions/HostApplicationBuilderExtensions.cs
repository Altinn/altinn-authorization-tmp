using Altinn.Authorization.Host.Identity;
using Altinn.Authorization.Integration.Platform.Register;
using Altinn.Authorization.Integration.Platform.ResourceRegister;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Altinn.Authorization.Integration.Platform.Extensions;

/// <summary>
/// Extension methods for <see cref="IHostApplicationBuilder"/> to register Altinn Register services.
/// </summary>
public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddAltinnIntegrations(this IHostApplicationBuilder builder, Action<AltinnIntegrationOptions> configureOptions)
    {
        if (builder.Services.Contains(Register.ServiceDescriptor))
        {
            return builder;
        }

        builder.Services.AddOptions<AltinnIntegrationOptions>()
            .Validate(opts => opts.PlatformAccessToken != null)
            .Validate(opts => !string.IsNullOrEmpty(opts.PlatformAccessToken.Issuer), $"Platform access token issuer can't be null or empty string")
            .Validate(opts => !string.IsNullOrEmpty(opts.PlatformAccessToken.App), $"Platform access token app can't be null or empty string")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.AzureKeyVault && opts.PlatformAccessToken.KeyVault.Uri == null,
                $"Can't specify key vault for token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.KeyVault.Uri)}' specified")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.TestTokenGenerator && string.IsNullOrEmpty(opts.PlatformAccessToken.TestTool.Username),
                $"Can't use test tool token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.TestTool.Username)}' specified")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.TestTokenGenerator && opts.PlatformAccessToken.TestTool.TokenGeneratorUrl == null,
                $"Can't use test tool token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.TestTool.TokenGeneratorUrl)}' specified")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.TestTokenGenerator && string.IsNullOrEmpty(opts.PlatformAccessToken.TestTool.Password),
                $"Can't use test tool token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.TestTool.Password)}' specified")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.TestTokenGenerator && string.IsNullOrEmpty(opts.PlatformAccessToken.TestTool.Environment),
                $"Can't use test tool token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.TestTool.Password)}' specified")
            .Configure(configureOptions);

        var options = new AltinnIntegrationOptions();
        configureOptions?.Invoke(options);

        if (options.PlatformAccessToken.TokenSource == AltinnIntegrationOptions.TokenSource.AzureKeyVault)
        {
            builder.Services.AddAzureClients(builder =>
            {
                builder.UseCredential(AzureToken.Default);
                builder.AddSecretClient(options.PlatformAccessToken.KeyVault.Uri)
                    .WithName(TokenGenerator.TokenGeneratorKeyVault.ServiceKey);
            });

            builder.Services.TryAddSingleton<ITokenGenerator, TokenGenerator.TokenGeneratorKeyVault>();
        }

        if (options.PlatformAccessToken.TokenSource == AltinnIntegrationOptions.TokenSource.TestTokenGenerator)
        {
            if (string.IsNullOrEmpty(options.HttpClientName))
            {
                builder.Services.AddHttpClient();
            }

            builder.Services.TryAddSingleton<ITokenGenerator, TokenGenerator.TokenGeneratorTestTool>();
        }

        return builder;
    }

    private sealed class Marker
    {
        public static readonly ServiceDescriptor ServiceDescriptor = ServiceDescriptor.Singleton<Register, Register>();
    }

    /// <summary>
    /// Adds the Altinn Register services to the <see cref="IHostApplicationBuilder"/>.
    /// Configures the <see cref="AltinnRegisterOptions"/> using the provided configuration action.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to add the services to.</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="AltinnRegisterOptions"/> instance.</param>
    /// <returns>The updated <see cref="IHostApplicationBuilder"/> instance with the Altinn Register services added.</returns>
    /// <remarks>
    /// This method registers the Altinn Register options, HTTP client, and the <see cref="IAltinnRegister"/> implementation to the dependency injection container.
    /// </remarks>
    public static IHostApplicationBuilder AddAltinnRegisterIntegration(this IHostApplicationBuilder builder, Action<AltinnRegisterOptions> configureOptions)
    {
        if (builder.Services.Contains(Register.ServiceDescriptor))
        {
            return builder;
        }

        builder.Services.AddOptions<AltinnRegisterOptions>()
            .Validate(opts => opts.Endpoint != null)
            .Configure(configureOptions);

        builder.Services.AddHttpClient(RegisterClient.HttpClientName, (serviceProvider, httpClient) => { });
        builder.Services.AddSingleton<IAltinnRegister, RegisterClient>();
        builder.Services.Add(Register.ServiceDescriptor);

        return builder;
    }

    /// <summary>
    /// Adds the Altinn Resource Register services to the <see cref="IHostApplicationBuilder"/>.
    /// Configures the <see cref="AltinnResourceRegisterOptions"/> using the provided configuration action.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to add the services to.</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="AltinnResourceRegisterOptions"/> instance.</param>
    /// <returns>The updated <see cref="IHostApplicationBuilder"/> instance with the Altinn Resource Register services added.</returns>
    /// <remarks>
    /// This method registers the Altinn Resource Register options, HTTP client, and the <see cref="IAltinnRegister"/> implementation to the dependency injection container.
    /// </remarks>
    public static IHostApplicationBuilder AddAltinnResourceRegisterIntegration(this IHostApplicationBuilder builder, Action<AltinnResourceRegisterOptions> configureOptions)
    {
        if (builder.Services.Contains(ResourceRegister.ServiceDescriptor))
        {
            return builder;
        }

        builder.Services.AddOptions<AltinnResourceRegisterOptions>()
            .Validate(opts => opts.Endpoint != null)
            .Configure(configureOptions);

        builder.Services.AddHttpClient(ResourceRegisterClient.HttpClientName, (serviceProvider, httpClient) => { });
        builder.Services.AddSingleton<IAltinnResourceRegister, ResourceRegisterClient>();
        builder.Services.Add(ResourceRegister.ServiceDescriptor);

        return builder;
    }

    private sealed class Register
    {
        public static readonly ServiceDescriptor ServiceDescriptor = ServiceDescriptor.Singleton<Register, Register>();
    }

    private sealed class ResourceRegister
    {
        public static readonly ServiceDescriptor ServiceDescriptor = ServiceDescriptor.Singleton<ResourceRegister, ResourceRegister>();
    }
}

public class AltinnIntegrationOptions
{
    public string HttpClientName { get; set; } = string.Empty;

    public PlatformAccessTokenOptions PlatformAccessToken { get; set; } = new();

    public class PlatformAccessTokenOptions
    {
        public TokenSource TokenSource { get; set; } = TokenSource.LocalCert;

        public string Issuer { get; set; } = string.Empty;

        public string App { get; set; } = string.Empty;

        public KeyVaultOptions KeyVault { get; set; } = new();

        public TestToolGeneratorOptions TestTool { get; set; } = new();

        public class KeyVaultOptions
        {
            public Uri Uri { get; set; }

            public Func<string, IServiceProvider, Task<string>> CacheHandler { get; set; } = (_, _) =>
            {
                return Task.FromResult(string.Empty);
            };
        }

        public class TestToolGeneratorOptions
        {
            public Uri TokenGeneratorUrl { get; set; }

            public string Username { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty;

            public string Environment { get; set; } = string.Empty;
        }
    }

    public enum TokenSource
    {
        AzureKeyVault,

        TestTokenGenerator,

        LocalCert,
    }
}
