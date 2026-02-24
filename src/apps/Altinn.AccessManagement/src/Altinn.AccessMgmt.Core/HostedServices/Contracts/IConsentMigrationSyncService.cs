namespace Altinn.AccessMgmt.Core.HostedServices.Contracts;

/// <summary>
/// Service for synchronizing/migrating consents from old application
/// </summary>
public interface IConsentMigrationSyncService
{
    /// <summary>
    /// Processes a batch of consents for migration
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of consents processed in the batch</returns>
    Task<int> ProcessBatch(CancellationToken cancellationToken);

    /// <summary>
    /// Gets migration statistics
    /// </summary>
    /// <returns>Tuple of processed, migrated, failed counts and last run time</returns>
    (int Processed, int Migrated, int Failed, DateTimeOffset LastRun) GetStatistics();
}
