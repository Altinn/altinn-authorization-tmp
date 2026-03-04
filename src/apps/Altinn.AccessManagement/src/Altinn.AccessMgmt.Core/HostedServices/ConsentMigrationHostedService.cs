using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace Altinn.AccessMgmt.Core.HostedServices;

/// <summary>
/// Hosted service for migrating consents from old application to new system
/// </summary>
public partial class ConsentMigrationHostedService : BackgroundService
{
    private readonly IConsentMigrationSyncService _syncService;
    private readonly IFeatureManager _featureManager;
    private readonly IOptionsMonitor<ConsentMigrationSettings> _settings;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ConsentMigrationHostedService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsentMigrationHostedService"/> class
    /// </summary>
    public ConsentMigrationHostedService(
        IConsentMigrationSyncService syncService,
        IFeatureManager featureManager,
        IOptionsMonitor<ConsentMigrationSettings> settings,
        TimeProvider timeProvider,
        ILogger<ConsentMigrationHostedService> logger)
    {
        _syncService = syncService;
        _featureManager = featureManager;
        _settings = settings;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.StartConsentMigration(_logger);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var migrationSettings = _settings.CurrentValue;
                bool isEnabled = await _featureManager.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration);
                if (!isEnabled)
                {
                    Log.FeatureDisabled(_logger);
                    await Task.Delay(migrationSettings.FeatureDisabledDelayMs, stoppingToken);
                    continue;
                }

                int processedCount = await _syncService.ProcessBatch(stoppingToken);

                if (processedCount == 0)
                {
                    await Task.Delay(migrationSettings.EmptyFeedDelayMs, stoppingToken);
                }
                else
                {
                    await Task.Delay(migrationSettings.NormalDelayMs, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.SyncError(_logger, ex);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    static partial class Log
    {
        [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Consent migration feature flag is disabled. Waiting before rechecking.")]
        internal static partial void FeatureDisabled(ILogger logger);

        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Consent Migration Service starting")]
        internal static partial void StartConsentMigration(ILogger logger);

        [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Unexpected error in consent migration loop")]
        internal static partial void SyncError(ILogger logger, Exception ex);
    }
}
