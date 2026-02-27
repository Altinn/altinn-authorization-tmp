using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Altinn.AccessMgmt.Core.HostedServices.Services;

/// <summary>
/// Service for synchronizing/migrating consents from old application
/// </summary>
public class ConsentMigrationSyncService : BaseSyncService, IConsentMigrationSyncService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConsentMigrationSettings _settings;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ConsentMigrationSyncService> _logger;
    private readonly Counter<long> _processedCounter;
    private readonly Counter<long> _migratedCounter;
    private readonly Counter<long> _failedCounter;
    private readonly Histogram<double> _batchDurationHistogram;

    private int _totalProcessed;
    private int _totalMigrated;
    private int _totalFailed;
    private DateTimeOffset _lastRunTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsentMigrationSyncService"/> class
    /// </summary>
    public ConsentMigrationSyncService(
        IServiceProvider serviceProvider,
        IOptions<ConsentMigrationSettings> settings,
        TimeProvider timeProvider,
        IMeterFactory meterFactory,
        ILogger<ConsentMigrationSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _timeProvider = timeProvider;
        _logger = logger;

        var meter = meterFactory.Create("Altinn.AccessManagement.ConsentMigration");
        _processedCounter = meter.CreateCounter<long>("consent_migration_processed_total", description: "Total number of consents processed");
        _migratedCounter = meter.CreateCounter<long>("consent_migration_migrated_total", description: "Total number of consents successfully migrated");
        _failedCounter = meter.CreateCounter<long>("consent_migration_failed_total", description: "Total number of consents that failed migration");
        _batchDurationHistogram = meter.CreateHistogram<double>("consent_migration_batch_duration_ms", unit: "ms", description: "Duration of batch processing");
    }

    /// <inheritdoc/>
    public (int Processed, int Migrated, int Failed, DateTimeOffset LastRun) GetStatistics()
    {
        return (_totalProcessed, _totalMigrated, _totalFailed, _lastRunTime);
    }

    /// <inheritdoc/>
    public async Task<int> ProcessBatch(CancellationToken cancellationToken)
    {
        var batchStopwatch = Stopwatch.StartNew();
        List<Guid> consentIds = null;
        int successCount = 0;
        int failedCount = 0;

        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var migrationClient = scope.ServiceProvider.GetRequiredService<IAltinn2ConsentClient>();
            var migrationService = scope.ServiceProvider.GetRequiredService<IConsentMigrationService>();

            consentIds = await migrationClient.GetAltinn2ConsentListForMigration(
                _settings.BatchSize,
                _settings.ConsentStatus,
                _settings.OnlyExpiredConsents,
                cancellationToken);

            if (consentIds == null || consentIds.Count == 0)
            {
                return 0;
            }

            _logger.LogInformation("Processing batch of {Count} consents with status '{Status}'", consentIds.Count, _settings.ConsentStatus);

            foreach (Guid consentId in consentIds)
            {
                bool success = await ProcessSingleConsent(migrationService, consentId, cancellationToken);
                if (success)
                {
                    successCount++;
                }
                else
                {
                    failedCount++;
                }
            }

            _lastRunTime = _timeProvider.GetUtcNow();
            batchStopwatch.Stop();

            var totalProcessed = successCount + failedCount;
            var successRate = totalProcessed > 0 ? (double)successCount / totalProcessed * 100 : 0;

            _logger.LogInformation(
                    "Batch completed: {Success}/{Total} succeeded ({SuccessRate:F1}%), {Failed} failed, Duration: {Duration}ms",
                    successCount, totalProcessed, successRate, failedCount, batchStopwatch.ElapsedMilliseconds);

            return consentIds.Count;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogWarning(httpEx, "Network error in batch processing. Will retry in next cycle.");
            return 0;
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Batch processing cancelled. Processed: {Success} succeeded, {Failed} failed",
              successCount, failedCount);
            return successCount + failedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch processing. Processed: {Success} succeeded, {Failed} failed",
              successCount, failedCount);
            return successCount + failedCount;
        }
        finally
        {
            _batchDurationHistogram.Record(batchStopwatch.Elapsed.TotalMilliseconds);
        }
    }

    private async Task<bool> ProcessSingleConsent(
        IConsentMigrationService migrationService,
        Guid consentId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Migrate the consent request
            var result = await migrationService.MigrateConsent(consentId, cancellationToken);

            _totalProcessed++;
            _processedCounter.Add(1);

            if (result.Success)
            {
                _totalMigrated++;
                _migratedCounter.Add(1);
                return true;
            }
            else
            {
                _totalFailed++;
                _failedCounter.Add(1);
                _logger.LogWarning("Failed to migrate consent {ConsentId}: {Error}", consentId, result.ErrorMessage);
                return false;
            }
        }
        catch (Exception ex)
        {
            _totalProcessed++;
            _processedCounter.Add(1);
            _totalFailed++;
            _failedCounter.Add(1);

            // Only log unexpected errors
            if (ex is not HttpRequestException and not TaskCanceledException)
            {
                _logger.LogError(ex, "Unexpected error migrating consent {ConsentId}", consentId);
            }

            return false;
        }
    }
}
