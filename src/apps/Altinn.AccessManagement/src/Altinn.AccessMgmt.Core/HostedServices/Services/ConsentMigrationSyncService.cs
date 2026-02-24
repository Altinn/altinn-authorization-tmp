using System.Diagnostics.Metrics;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        var startTime = _timeProvider.GetUtcNow();

        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var migrationClient = scope.ServiceProvider.GetRequiredService<IConsentMigrationClient>();
            var migrationService = scope.ServiceProvider.GetRequiredService<IConsentMigrationService>();

            List<Guid> consentIds = await migrationClient.GetConsentIdsForMigration(
                _settings.BatchSize,
                _settings.ConsentStatus,
                cancellationToken);

            if (consentIds.Count == 0)
            {
                return 0;
            }

            _logger.LogInformation("Processing batch of {Count} consents with status '{Status}'", consentIds.Count, _settings.ConsentStatus);

            foreach (Guid consentId in consentIds)
            {
                await ProcessSingleConsent(migrationClient, migrationService, consentId, cancellationToken);
            }

            _lastRunTime = _timeProvider.GetUtcNow();

            return consentIds.Count;
        }
        finally
        {
            var duration = (_timeProvider.GetUtcNow() - startTime).TotalMilliseconds;
            _batchDurationHistogram.Record(duration);
        }
    }

    private async Task ProcessSingleConsent(
        IConsentMigrationClient migrationClient,
        IConsentMigrationService migrationService,
        Guid consentId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Fetch consent details from old application
            var consentRequest = await migrationClient.GetConsentDetails(consentId, cancellationToken);

            if (consentRequest == null)
            {
                _totalProcessed++;
                _processedCounter.Add(1);
                _totalFailed++;
                _failedCounter.Add(1);
                await migrationClient.UpdateMigrationStatus(consentId, "failed", cancellationToken);
                _logger.LogWarning("Consent {ConsentId} not found in old application", consentId);
                return;
            }

            // Migrate the consent request
            var result = await migrationService.MigrateConsentRequest(consentRequest, cancellationToken);

            _totalProcessed++;
            _processedCounter.Add(1);

            if (result.Success)
            {
                await migrationClient.UpdateMigrationStatus(consentId, "migrated", cancellationToken);
                _totalMigrated++;
                _migratedCounter.Add(1);

                if (result.AlreadyExisted)
                {
                    _logger.LogInformation("Consent {ConsentId} already existed (duplicate), marked as migrated", consentId);
                }
                else
                {
                    _logger.LogInformation("Successfully migrated consent {ConsentId}", consentId);
                }
            }
            else
            {
                await migrationClient.UpdateMigrationStatus(consentId, "failed", cancellationToken);
                _totalFailed++;
                _failedCounter.Add(1);
                _logger.LogWarning("Failed to migrate consent {ConsentId}: {Error}", consentId, result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _totalProcessed++;
            _processedCounter.Add(1);
            _totalFailed++;
            _failedCounter.Add(1);

            try
            {
                await migrationClient.UpdateMigrationStatus(consentId, "failed", cancellationToken);
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update migration status for consent {ConsentId}", consentId);
            }

            _logger.LogError(ex, "Exception while processing consent {ConsentId}", consentId);
        }
    }
}
