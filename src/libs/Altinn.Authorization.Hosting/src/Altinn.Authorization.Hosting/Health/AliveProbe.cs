using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Altinn.Authorization.Hosting.Health;

/// <summary>
/// Represents a health check implementation that verifies the application is alive.
/// </summary>
public class AliveProbe : IHealthCheck
{
    /// <summary>
    /// Checks the health of the application and returns a healthy result.
    /// </summary>
    /// <param name="context">The context for the health check, which may contain additional information.</param>
    /// <param name="cancellationToken">A cancellation token for controlling the operation.</param>
    /// <returns>A task representing the health check result.</returns>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("Alive"));
    }
}
