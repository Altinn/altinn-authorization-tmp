namespace Altinn.AccessManagement.Core.Configuration;

/// <summary>
/// Configuration settings for consent migration from old application
/// </summary>
public class ConsentMigrationSettings
{
  /// <summary>
  /// Number of consent GUIDs to fetch per batch
  /// </summary>
  public int BatchSize { get; set; } = 100;

  /// <summary>
  /// Status of consents to migrate (e.g., "expired", "active")
  /// </summary>
  public int ConsentStatus { get; set; } = 1;

  /// <summary>
  /// Delay in milliseconds between processing batches (when GUIDs are available)
  /// </summary>
  public int NormalDelayMs { get; set; } = 2000;

  /// <summary>
  /// Delay in milliseconds when feed returns zero GUIDs (longer pause before retrying)
  /// </summary>
  public int EmptyFeedDelayMs { get; set; } = 60000;

  /// <summary>
  /// Date after which migration should stop
  /// </summary>
  public DateTime EndDate { get; set; }

  /// <summary>
  /// Indicates whether only expired consents should be migrated
  /// </summary>
  public bool OnlyExpiredConsents { get; set; } = true;
}
