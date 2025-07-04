using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.Host.Identity;
using Altinn.Authorization.Host.Startup;
using Altinn.Authorization.Integration.Platform.AccessManagement;
using Altinn.Authorization.Integration.Platform.Appsettings;
using Altinn.Authorization.Integration.Platform.Register;
using Altinn.Authorization.Integration.Platform.ResourceRegistry;
using Altinn.Authorization.Integration.Platform.SblBridge;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.Integration.Platform.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IHostApplicationBuilder"/> to register Altinn Register services.
/// </summary>
public static partial class ServiceCollectionExtensions
{
    private static ILogger Logger { get; } = StartupLoggerFactory.Create(nameof(ServiceCollectionExtensions));

    /// <summary>
    /// Adds Altinn platform integration defaults to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configureOptions">configure options for <see cref="PlatformSettings"/></param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddAltinnPlatformIntegrationDefaults(this IServiceCollection services, Func<PlatformSettings> configureOptions)
    {
        var appsettings = configureOptions();
        var descriptor = services.GetAltinnServiceDescriptor();
        services.AddAltinnPlatformIntegration(opts =>
        {
            opts.PlatformAccessToken.App = descriptor.Name;
            opts.PlatformAccessToken.Issuer = "platform";
            if (appsettings.Token.KeyVault.Endpoint != null)
            {
                Log.AddKeyVaultTokenSource(Logger);
                opts.PlatformAccessToken.TokenSource = AltinnIntegrationOptions.TokenSource.AzureKeyVault;
                opts.PlatformAccessToken.KeyVault.Endpoint = appsettings.Token.KeyVault.Endpoint;
                opts.PlatformAccessToken.KeyVault.CacheTimeout = 3;
            }
            else
            {
                Log.AddTestToolTokenSource(Logger);
                opts.PlatformAccessToken.TokenSource = AltinnIntegrationOptions.TokenSource.TestTool;
                opts.PlatformAccessToken.TestTool.Endpoint = appsettings.Token.TestTool.Endpoint;
                opts.PlatformAccessToken.TestTool.Username = appsettings.Token.TestTool.Username;
                opts.PlatformAccessToken.TestTool.Password = appsettings.Token.TestTool.Password;
                opts.PlatformAccessToken.TestTool.Environment = appsettings.Token.TestTool.Environment;
            }
        })
        .AddRegister(opts =>
        {
            opts.Endpoint = appsettings.Register.Endpoint;
        })
        .AddResourceRegistry(opts =>
        {
            opts.Endpoint = appsettings.ResourceRegistry.Endpoint;
        })
        .AddSblBridge(opts =>
        {
            opts.Endpoint = appsettings.SblBridge.Endpoint;
        });

        return services;
    }

    /// <summary>
    /// Adds Altinn platform integration services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configureOptions">A delegate to configure the <see cref="AltinnIntegrationOptions"/>.</param>
    /// <returns>A <see cref="PlatformBuilder"/> instance for further configuration.</returns>
    public static PlatformBuilder AddAltinnPlatformIntegration(this IServiceCollection services, Action<AltinnIntegrationOptions> configureOptions)
    {
        if (services.Contains(Marker.ServiceDescriptor))
        {
            return new(services);
        }

        services.AddOptions<AltinnIntegrationOptions>()
            .Validate(opts => opts.PlatformAccessToken != null)
            .Validate(opts => !string.IsNullOrEmpty(opts.PlatformAccessToken.Issuer), $"Platform access token issuer can't be null or empty string")
            .Validate(opts => !string.IsNullOrEmpty(opts.PlatformAccessToken.App), $"Platform access token app can't be null or empty string")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.AzureKeyVault || opts.PlatformAccessToken.KeyVault.Endpoint != null,
                $"Can't specify key vault for token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.KeyVault.Endpoint)}' specified")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.TestTool || !string.IsNullOrEmpty(opts.PlatformAccessToken.TestTool.Username),
                $"Can't use test tool token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.TestTool.Username)}' specified")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.TestTool || opts.PlatformAccessToken.TestTool.Endpoint != null,
                $"Can't use test tool token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.TestTool.Endpoint)}' specified")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.TestTool || !string.IsNullOrEmpty(opts.PlatformAccessToken.TestTool.Password),
                $"Can't use test tool token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.TestTool.Password)}' specified")
            .Validate(
                opts => opts.PlatformAccessToken.TokenSource != AltinnIntegrationOptions.TokenSource.TestTool || !string.IsNullOrEmpty(opts.PlatformAccessToken.TestTool.Environment),
                $"Can't use test tool token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.TestTool.Password)}' specified")
            .Configure(configureOptions);

        var options = new AltinnIntegrationOptions();
        configureOptions?.Invoke(options);

        if (options.PlatformAccessToken.TokenSource == AltinnIntegrationOptions.TokenSource.AzureKeyVault)
        {
            services.AddAzureClients(builder =>
            {
                builder.UseCredential(AzureToken.Default);
                builder.AddCertificateClient(options.PlatformAccessToken.KeyVault.Endpoint)
                    .WithName(TokenGenerator.KeyVault.ServiceKey);

                builder.AddSecretClient(options.PlatformAccessToken.KeyVault.Endpoint)
                    .WithName(TokenGenerator.KeyVault.ServiceKey);
            });

            services.TryAddSingleton<ITokenGenerator, TokenGenerator.KeyVault>();
        }
        else if (options.PlatformAccessToken.TokenSource == AltinnIntegrationOptions.TokenSource.TestTool)
        {
            if (string.IsNullOrEmpty(options.HttpClientName))
            {
                services.AddHttpClient();
            }

            services.TryAddSingleton<ITokenGenerator, TokenGenerator.TestTool>();
        }

        services.TryAddSingleton(Marker.ServiceDescriptor);

        return new(services);
    }

    private sealed class Marker
    {
        public static readonly ServiceDescriptor ServiceDescriptor = ServiceDescriptor.Singleton<Marker, Marker>();
    }

    /// <summary>
    /// Provides methods for configuring Altinn Register and Resource Register services.
    /// </summary>
    public class PlatformBuilder
    {
        internal PlatformBuilder(IServiceCollection services)
        {
            Services = services;
        }

        internal IServiceCollection Services { get; }

        /// <summary>
        /// Adds Altinn Register services to the service collection.
        /// </summary>
        /// <param name="configureOptions">A delegate to configure <see cref="AltinnRegisterOptions"/>.</param>
        /// <returns>The updated <see cref="PlatformBuilder"/>.</returns>
        public PlatformBuilder AddRegister(Action<AltinnRegisterOptions> configureOptions)
        {
            if (Services.Contains(Markers.Register))
            {
                return this;
            }

            Services.AddOptions<AltinnRegisterOptions>()
                .Validate(opts => opts.Endpoint != null, $"Can't add Register as '{nameof(AltinnRegisterOptions.Endpoint)}' is not specified")
                .Configure(configureOptions);

            Services.TryAddSingleton<IAltinnRegister, AltinnRegisterClient>();
            Services.TryAddSingleton(Markers.Register);

            return this;
        }

        /// <summary>
        /// Adds Altinn Resource Register services to the service collection.
        /// </summary>
        /// <param name="configureOptions">A delegate to configure <see cref="AltinnResourceRegistryOptions"/>.</param>
        /// <returns>The updated <see cref="PlatformBuilder"/>.</returns>
        public PlatformBuilder AddResourceRegistry(Action<AltinnResourceRegistryOptions> configureOptions)
        {
            if (Services.Contains(Markers.ResourceRegistry))
            {
                return this;
            }

            Services.AddOptions<AltinnResourceRegistryOptions>()
                .Validate(opts => opts.Endpoint != null, $"Can't add Resource Register as '{nameof(AltinnRegisterOptions.Endpoint)}' is not specified")
                .Configure(configureOptions);

            Services.AddSingleton<IAltinnResourceRegistry, AltinnResourceRegistryClient>();
            Services.Add(Markers.ResourceRegistry);

            return this;
        }

        /// <summary>
        /// Adds Altinn SBL bridge services to the service collection.
        /// </summary>
        /// <param name="configureOptions">A delegate to configure <see cref="AltinnResourceRegistryOptions"/>.</param>
        /// <returns>The updated <see cref="PlatformBuilder"/>.</returns>
        public PlatformBuilder AddSblBridge(Action<AltinnSblBridgeOptions> configureOptions)
        {
            if (Services.Contains(Markers.SblBridge))
            {
                return this;
            }

            Services.AddOptions<AltinnSblBridgeOptions>()
                .Validate(opts => opts.Endpoint is { }, $"Can't add SBL Bridge as '{nameof(AltinnSblBridgeOptions.Endpoint)}' is not specified")
                .Configure(configureOptions);

            Services.AddSingleton<IAltinnSblBridge, AltinnSblBridgeClient>();
            Services.Add(Markers.SblBridge);

            return this;
        }
        
        /// <summary>
        /// Adds Altinn SBL bridge services to the service collection.
        /// </summary>
        /// <param name="configureOptions">A delegate to configure <see cref="AltinnResourceRegistryOptions"/>.</param>
        /// <returns>The updated <see cref="PlatformBuilder"/>.</returns>
        public PlatformBuilder AddAccessManagement(Action<AltinnAccessManagementOptions> configureOptions)
        {
            if (Services.Contains(Markers.AccessManagement))
            {
                return this;
            }

            Services.AddOptions<AltinnAccessManagementOptions>()
                .Validate(opts => opts.Endpoint is { }, $"Can't add SBL Bridge as '{nameof(AltinnAccessManagementOptions.Endpoint)}' is not specified")
                .Configure(configureOptions);

            Services.AddSingleton<IAltinnAccessManagement, AltinnAccessManagementClient>();
            Services.Add(Markers.AccessManagement);

            return this;
        }

        private sealed class Markers
        {
            public static ServiceDescriptor Register { get; } = ServiceDescriptor.Singleton<RegisterMarker, RegisterMarker>();

            public static ServiceDescriptor ResourceRegistry { get; } = ServiceDescriptor.Singleton<ResourceRegistryMarker, ResourceRegistryMarker>();

            public static ServiceDescriptor SblBridge { get; } = ServiceDescriptor.Singleton<SblBridgeMarker, SblBridgeMarker>();

            public static ServiceDescriptor AccessManagement { get; } = ServiceDescriptor.Singleton<AccessManagementMarker, AccessManagementMarker>();

            [SuppressMessage("CodeSmell", "S2094:Classes should not be empty", Justification = "Used as a DI marker")]
            private sealed class RegisterMarker { }

            [SuppressMessage("CodeSmell", "S2094:Classes should not be empty", Justification = "Used as a DI marker")]
            private sealed class ResourceRegistryMarker { }

            [SuppressMessage("CodeSmell", "S2094:Classes should not be empty", Justification = "Used as a DI marker")]
            private sealed class SblBridgeMarker { }

            [SuppressMessage("CodeSmell", "S2094:Classes should not be empty", Justification = "Used as a DI marker")]
            private sealed class AccessManagementMarker { }
        }
    }

    static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Adding Key Vault source as platform access token generator")]
        internal static partial void AddKeyVaultTokenSource(ILogger logger);

        [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Adding test token source as platform access token generator")]
        internal static partial void AddTestToolTokenSource(ILogger logger);
    }
}
