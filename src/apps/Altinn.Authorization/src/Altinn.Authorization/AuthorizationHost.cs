using Altinn.Authorization.ServiceDefaults;

namespace Altinn.Authorization;

/// <summary>
/// Configures the host.
/// </summary>
internal static class AuthorizationHost
{
    /// <summary>
    /// Configures the host.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public static WebApplication Create(string[] args)
    {
        var builder = AltinnHost.CreateWebApplicationBuilder("access-management", args);

        return builder.Build();
    }
}
