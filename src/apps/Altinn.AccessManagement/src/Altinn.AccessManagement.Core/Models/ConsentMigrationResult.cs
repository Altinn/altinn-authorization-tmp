namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Result of a single consent migration operation
/// </summary>
public record ConsentMigrationResult
{
  /// <summary>
  /// Indicates if the migration was successful
  /// </summary>
  public bool Success { get; init; }

  /// <summary>
  /// Indicates if the consent already existed (duplicate)
  /// </summary>
  public bool AlreadyExisted { get; init; }

  /// <summary>
  /// Error message if migration failed
  /// </summary>
  public string? ErrorMessage { get; init; }

  /// <summary>
  /// Creates a successful result
  /// </summary>
  public static ConsentMigrationResult Succeeded() => new() { Success = true };

  /// <summary>
  /// Creates a result indicating the consent already existed
  /// </summary>
  public static ConsentMigrationResult Duplicate() => new() { Success = true, AlreadyExisted = true };

  /// <summary>
  /// Creates a failed result with error message
  /// </summary>
  public static ConsentMigrationResult Failed(string errorMessage) => new() { Success = false, ErrorMessage = errorMessage };
}
