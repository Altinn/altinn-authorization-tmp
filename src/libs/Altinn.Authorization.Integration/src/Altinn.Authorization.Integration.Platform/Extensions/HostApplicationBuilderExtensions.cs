using Altinn.Authorization.Integration.Platform.Register;
using Altinn.Authorization.Integration.Platform.ResourceRegister;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altinn.Authorization.Integration.Platform.Extensions;

/// <summary>
/// Extension methods for <see cref="IHostApplicationBuilder"/> to register Altinn Register services.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddAltinnIntegrations(this IHostApplicationBuilder builder, Action<AltinnIntegrationOptions> configureOptions)
    {
        if (builder.Services.Contains(Register.ServiceDescriptor))
        {
            return builder;
        }

        builder.Services.AddOptions<AltinnIntegrationOptions>()
            .Validate(opts => opts.PlatformAccessToken != null)
            .Validate(opts => opts.PlatformAccessToken.UseTestTokenGenerator && opts.PlatformAccessToken.TokenGeneratorUrl != null, $"Can't use test token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.Username)}' specified")
            .Validate(opts => opts.PlatformAccessToken.UseTestTokenGenerator && opts.PlatformAccessToken.Username != null, $"Can't use test token generator and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.Username)}' specified")
            .Validate(opts => opts.PlatformAccessToken.UseTestTokenGenerator && opts.PlatformAccessToken.Password != null, $"Can't use password and not having '{nameof(AltinnIntegrationOptions.PlatformAccessToken.Password)}' specified")
            .Configure(configureOptions);
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
    public PlatformAccessTokenOptions PlatformAccessToken { get; set; } = new();

    public class PlatformAccessTokenOptions
    {
        public bool UseTestTokenGenerator { get; set; }

        public Uri TokenGeneratorUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
