using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace Altinn.AccessMgmt.Core.HealthChecks;

/// <summary>
/// Health check for consent migration service
/// </summary>
public class ConsentMigrationHealthCheck : IHealthCheck
{
    private readonly IConsentMigrationSyncService _syncService;
    private readonly IFeatureManager _featureManager;
    private readonly ConsentMigrationSettings _settings;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsentMigrationHealthCheck"/> class
    /// </summary>
    public ConsentMigrationHealthCheck(
        IConsentMigrationSyncService syncService,
        IFeatureManager featureManager,
        IOptions<ConsentMigrationSettings> settings,
        TimeProvider timeProvider)
    {
        _syncService = syncService;
        _featureManager = featureManager;
        _settings = settings.Value;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            bool isEnabled = await _featureManager.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration);
            var (processed, migrated, failed, lastRun) = _syncService.GetStatistics();

            var data = new Dictionary<string, object>
            {
                { "FeatureFlagEnabled", isEnabled },
                { "TotalProcessed", processed },
                { "TotalMigrated", migrated },
                { "TotalFailed", failed },
                { "LastRunTime", lastRun },
                { "EndDate", _settings.EndDate }
            };

            if (!isEnabled)
            {
                return HealthCheckResult.Degraded("Consent migration is disabled by feature flag", data: data);
            }

            if (_timeProvider.GetUtcNow() > _settings.EndDate)
            {
                return HealthCheckResult.Degraded("Consent migration end date has been reached", data: data);
            }

            if (failed > 0 && processed > 0 && (double)failed / processed > 0.5)
            {
                return HealthCheckResult.Degraded($"High failure rate: {failed}/{processed}", data: data);
            }

            return HealthCheckResult.Healthy("Consent migration is running", data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to check consent migration health", ex);
        }
    }
}
