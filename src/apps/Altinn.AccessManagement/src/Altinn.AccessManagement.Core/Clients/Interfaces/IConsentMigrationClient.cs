using Altinn.AccessManagement.Core.Models.Consent;

namespace Altinn.AccessManagement.Core.Clients.Interfaces;

/// <summary>
/// Client interface for communicating with the old consent application
/// </summary>
/// <remarks>
/// This is a mock interface. Replace the implementation when the actual client is available from the other PR.
/// </remarks>
public interface IConsentMigrationClient
{
  /// <summary>
  /// Gets consent IDs from the old application feed for migration
  /// </summary>
  /// <param name="batchSize">Number of consent IDs to retrieve</param>
  /// <param name="status">Status of consents to retrieve (e.g., "expired", "active")</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>List of consent GUIDs ready for migration</returns>
  Task<List<Guid>> GetConsentIdsForMigration(int batchSize, string status, CancellationToken cancellationToken);

  /// <summary>
  /// Gets consent details from the old application
  /// </summary>
  /// <param name="consentId">The consent ID to retrieve</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Consent request details from old application</returns>
  Task<ConsentRequest> GetConsentDetails(Guid consentId, CancellationToken cancellationToken);

  /// <summary>
  /// Updates the migration status for a consent in the old application
  /// </summary>
  /// <param name="consentId">The consent ID to update</param>
  /// <param name="status">Migration status ("migrated" or "failed")</param>
  /// <param name="cancellationToken">Cancellation token</param>
  Task UpdateMigrationStatus(Guid consentId, string status, CancellationToken cancellationToken);
}
