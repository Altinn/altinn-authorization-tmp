using Microsoft.AspNetCore.Builder;

namespace Altinn.Authorization.Hosting.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="WebApplication"/> class,
/// facilitating the configuration of default settings for the Altinn host.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures default settings for the Altinn host by mapping a health check endpoint.
    /// </summary>
    /// <param name="app">The instance of the <see cref="WebApplication"/> to configure.</param>
    /// <returns>The updated <see cref="WebApplication"/> instance for method chaining.</returns>
    public static WebApplication UseAltinnHostDefaults(this WebApplication app)
    {
        app.MapHealthChecks("/healthz");
        return app;
    }
}
