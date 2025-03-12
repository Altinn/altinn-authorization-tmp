using Altinn.Authorization.Host.Identity;
using Altinn.Authorization.Integration.Platform.Appsettings;
using Altinn.Authorization.Integration.Platform.Register;
using Altinn.Authorization.Integration.Platform.ResourceRegister;
using Altinn.Common.AccessTokenClient.Services;
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
    public static IServiceCollection AddAltinnPlatformIntegrationDefaults(this IServiceCollection services, Func<PlatformSettings> configureOptions)
    {
        var appsettings = configureOptions();
        var descriptor = services.GetAltinnServiceDescriptor();

        services.AddAltinnPlatformIntegration(opts =>
        {
            opts.PlatformAccessToken.App = descriptor.Name;
            opts.PlatformAccessToken.Issuer = "platform";
            if (appsettings.Token.KeyVault != null)
            {
                opts.PlatformAccessToken.TokenSource = AltinnIntegrationOptions.TokenSource.AzureKeyVault;
                opts.PlatformAccessToken.KeyVault.Endpoint = appsettings.Token.KeyVault.Endpoint;
                opts.PlatformAccessToken.KeyVault.CacheTimeout = 30;
            }
            else
            {
                opts.PlatformAccessToken.TokenSource = AltinnIntegrationOptions.TokenSource.TestTool;
                opts.PlatformAccessToken.TestTool.Endpoint = appsettings.Token.TestTool.Endpoint;
                opts.PlatformAccessToken.TestTool.Username = appsettings.Token.TestTool.Username;
                opts.PlatformAccessToken.TestTool.Password = appsettings.Token.TestTool.Password;
            }
        })
        .AddRegister(opts =>
        {
            opts.Endpoint = appsettings.Register.Endpoint;
        })
        .AddResourceRegister(opts =>
        {
            opts.Endpoint = appsettings.ResourceRegister.Endpoint;
        });

        return services;
    }

    public static PlatformBuilder AddAltinnPlatformIntegration(this IServiceCollection services, Action<AltinnIntegrationOptions> configureOptions)
    {
        var options = new AltinnIntegrationOptions();
        configureOptions?.Invoke(options);

        if (services.Contains(Marker.ServiceDescriptor))
        {
            return new(services);
        }

        services.AddOptions<AltinnIntegrationOptions>()
            .Validate(opts => opts.PlatformAccessToken != null)
            .Validate(opts => !string.IsNullOrEmpty(opts.PlatformAccessToken.Issuer), $"Platform access token issuer can't be null or empty string")
            .Validate(opts => !string.IsNullOrEmpty(opts.PlatformAccessToken.App), $"Platform access token app can't be null or empty string")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.AzureKeyVault && opts.PlatformAccessToken.KeyVault.Endpoint != null,
                $"Can't specify key vault for token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.KeyVault.Endpoint)}' specified")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.TestTool && string.IsNullOrEmpty(opts.PlatformAccessToken.TestTool.Username),
                $"Can't use test tool token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.TestTool.Username)}' specified")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.TestTool && opts.PlatformAccessToken.TestTool.Endpoint == null,
                $"Can't use test tool token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.TestTool.Endpoint)}' specified")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.TestTool && string.IsNullOrEmpty(opts.PlatformAccessToken.TestTool.Password),
                $"Can't use test tool token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.TestTool.Password)}' specified")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.TestTool && string.IsNullOrEmpty(opts.PlatformAccessToken.TestTool.Environment),
                $"Can't use test tool token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.TestTool.Password)}' specified")
            .Configure(configureOptions);

        if (options.PlatformAccessToken.TokenSource == AltinnIntegrationOptions.TokenSource.AzureKeyVault)
        {
            services.AddAzureClients(opts =>
            {
                opts.AddSecretClient(options.PlatformAccessToken.KeyVault.Endpoint)
                    .WithName(TokenGenerator.TokenGeneratorKeyVault.ServiceKey)
                    .WithCredential(AzureToken.Default);

                opts.AddCertificateClient(options.PlatformAccessToken.KeyVault.Endpoint)
                    .WithName(TokenGenerator.TokenGeneratorKeyVault.ServiceKey)
                    .WithCredential(AzureToken.Default);
            });

            services.TryAddSingleton<ITokenGenerator, TokenGenerator.TokenGeneratorKeyVault>();
            services.TryAddSingleton<IAccessTokenGenerator, AccessTokenGenerator>();
        }

        if (options.PlatformAccessToken.TokenSource == AltinnIntegrationOptions.TokenSource.TestTool)
        {
            if (string.IsNullOrEmpty(options.HttpClientName))
            {
                services.AddHttpClient();
            }

            services.TryAddSingleton<ITokenGenerator, TokenGenerator.TokenGeneratorTestTool>();
        }

        return new(services);
    }

    private sealed class Marker
    {
        public static readonly ServiceDescriptor ServiceDescriptor = ServiceDescriptor.Singleton<Marker, Marker>();
    }

    public class PlatformBuilder
    {
        internal PlatformBuilder(IServiceCollection services)
        {
            Services = services;
        }

        internal IServiceCollection Services { get; }

        /// <summary>
        /// Adds the Altinn Register services to the <see cref="IServiceCollection"/>.
        /// Configures the <see cref="AltinnRegisterOptions"/> using the provided configuration action.
        /// </summary>
        /// <param name="configureOptions">A delegate to configure the <see cref="AltinnRegisterOptions"/> instance.</param>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with the Altinn Register services added.</returns>
        /// <remarks>
        /// This method registers the Altinn Register options, HTTP client, and the <see cref="IAltinnRegister"/> implementation to the dependency injection container.
        /// </remarks>
        public PlatformBuilder AddRegister(Action<AltinnRegisterOptions> configureOptions)
        {
            if (Services.Contains(Register.ServiceDescriptor))
            {
                return this;
            }

            Services.AddOptions<AltinnRegisterOptions>()
                .Validate(opts => opts.Endpoint != null)
                .Configure(configureOptions);

            Services.AddHttpClient(RegisterClient.HttpClientName, (serviceProvider, httpClient) => { });
            Services.AddSingleton<IAltinnRegister, RegisterClient>();
            Services.Add(Register.ServiceDescriptor);

            return this;
        }

        /// <summary>
        /// Adds the Altinn Resource Register services to the <see cref="IServiceCollection"/>.
        /// Configures the <see cref="AltinnResourceRegisterOptions"/> using the provided configuration action.
        /// </summary>
        /// <param name="configureOptions">A delegate to configure the <see cref="AltinnResourceRegisterOptions"/> instance.</param>
        /// <returns>The updated <see cref="IServiceCollection"/> instance with the Altinn Resource Register services added.</returns>
        /// <remarks>
        /// This method registers the Altinn Resource Register options, HTTP client, and the <see cref="IAltinnRegister"/> implementation to the dependency injection container.
        /// </remarks>
        public PlatformBuilder AddResourceRegister(Action<AltinnResourceRegisterOptions> configureOptions)
        {
            if (Services.Contains(ResourceRegister.ServiceDescriptor))
            {
                return this;
            }

            Services.AddOptions<AltinnResourceRegisterOptions>()
                .Validate(opts => opts.Endpoint != null)
                .Configure(configureOptions);

            Services.AddHttpClient(ResourceRegisterClient.HttpClientName, (serviceProvider, httpClient) => { });
            Services.AddSingleton<IAltinnResourceRegister, ResourceRegisterClient>();
            Services.Add(ResourceRegister.ServiceDescriptor);

            return this;
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
}

public class AltinnIntegrationOptions
{
    /// <summary>
    /// Override this with HTTP client name if having a custom HTTP client
    /// </summary>
    public string HttpClientName { get; set; } = string.Empty;

    public PlatformAccessTokenOptions PlatformAccessToken { get; set; } = new();

    public class PlatformAccessTokenOptions
    {
        public TokenSource TokenSource { get; set; } = TokenSource.TestTool;

        public string Issuer { get; set; } = string.Empty;

        public string App { get; set; } = string.Empty;

        public KeyVaultOptions KeyVault { get; set; } = new();

        public TestToolOptions TestTool { get; set; } = new();

        public class KeyVaultOptions
        {
            public Uri Endpoint { get; set; }

            public int CacheTimeout { get; set; } = 30;
        }

        public class TestToolOptions
        {
            public Uri Endpoint { get; set; }

            public string Username { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty;

            public string Environment { get; set; } = string.Empty;
        }
    }

    public enum TokenSource
    {
        AzureKeyVault,

        TestTool,
    }
}
