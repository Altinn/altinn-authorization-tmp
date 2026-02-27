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
    private readonly ConsentMigrationSettings _settings;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ConsentMigrationHostedService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsentMigrationHostedService"/> class
    /// </summary>
    public ConsentMigrationHostedService(
        IConsentMigrationSyncService syncService,
        IFeatureManager featureManager,
        IOptions<ConsentMigrationSettings> settings,
        TimeProvider timeProvider,
        ILogger<ConsentMigrationHostedService> logger)
    {
        _syncService = syncService;
        _featureManager = featureManager;
        _settings = settings.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Gets migration statistics
    /// </summary>
    public (int Processed, int Migrated, int Failed, DateTimeOffset LastRun) GetStatistics()
    {
    return _syncService.GetStatistics();
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.StartConsentMigration(_logger);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                bool isEnabled = await _featureManager.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration);
                if (!isEnabled)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    continue;
                }

                if (_timeProvider.GetUtcNow() > _settings.EndDate)
                {
                    Log.EndDateReached(_logger, _settings.EndDate);
                    break;
                }

                int processedCount = await _syncService.ProcessBatch(stoppingToken);

                if (processedCount == 0)
                {
                    await Task.Delay(_settings.EmptyFeedDelayMs, stoppingToken);
                }
                else
                {
                    await Task.Delay(_settings.NormalDelayMs, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                Log.StoppingConsentMigration(_logger);
                break;
            }
            catch (Exception ex)
            {
                Log.SyncError(_logger, ex);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        var (processed, migrated, failed, _) = _syncService.GetStatistics();
        Log.StoppedConsentMigration(_logger, processed, migrated, failed);
    }

    static partial class Log
  {
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Consent Migration Service starting")]
    internal static partial void StartConsentMigration(ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Consent migration end date {endDate} reached. Stopping service.")]
    internal static partial void EndDateReached(ILogger logger, DateTime endDate);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Consent migration service is stopping")]
    internal static partial void StoppingConsentMigration(ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Consent Migration Service stopped. Total processed: {processed}, Migrated: {migrated}, Failed: {failed}")]
    internal static partial void StoppedConsentMigration(ILogger logger, int processed, int migrated, int failed);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Unexpected error in consent migration loop")]
    internal static partial void SyncError(ILogger logger, Exception ex);
  }
}
