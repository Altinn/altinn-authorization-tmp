using Altinn.Authorization.AccessPackages.Services;
using Altinn.Authorization.Configuration.Extensions;
using Altinn.Authorization.Configuration.OpenTelemetry.Extensions;
using Altinn.Authorization.Hosting.Extensions;

namespace Altinn.Authorization.AccessPackages;

/// <summary>
/// Provides methods for creating and configuring the host for the Altinn Access Packages service.
/// </summary>
public static class AccessPackagesHost
{
    /// <summary>
    /// Creates and configures a new instance of the <see cref="WebApplication"/>.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    /// <param name="service">The name of the service being configured.</param>
    /// <returns>An instance of <see cref="WebApplication"/> configured for the Altinn Access Packages service.</returns>
    public static WebApplication Create(string[] args, string service)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureAppsettings(builder);
        ConfigureServices(builder, service);
        return builder.Build();
    }

    /// <summary>
    /// Configures application settings for the Altinn Access Packages service.
    /// </summary>
    /// <param name="builder">The instance of <see cref="WebApplicationBuilder"/> to configure.</param>
    /// <returns>The updated <see cref="WebApplicationBuilder"/> instance for method chaining.</returns>
    public static WebApplicationBuilder ConfigureAppsettings(WebApplicationBuilder builder)
    {
        builder.AddAltinnAppConfiguration(opts => opts.AddDefaults());
        return builder;
    }

    /// <summary>
    /// Configures services for the Altinn Access Packages service.
    /// </summary>
    /// <param name="builder">The instance of <see cref="WebApplicationBuilder"/> to configure.</param>
    /// <param name="service">The name of the service being configured.</param>
    /// <returns>The updated <see cref="WebApplicationBuilder"/> instance for method chaining.</returns>
    public static WebApplicationBuilder ConfigureServices(WebApplicationBuilder builder, string service)
    {
        builder.AddAltinnHostDefaults();
        builder.AddAltinnDefaultOpenTelemetry(otel => otel.ServiceName = service);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddLogging();
        builder.Services.AddControllers();

        builder.Services.AddSingleton<IAccessPackageService, AccessPackagesService>();
        return builder;
    }
}
