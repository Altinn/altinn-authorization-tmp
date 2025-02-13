namespace Altinn.AccessMgmt.DbAccess.Models;

/// <summary>
/// MigrationEntry
/// </summary>
public class MigrationEntry
{
    /// <summary>
    /// ObjectName
    /// </summary>
    public string ObjectName { get; set; }

    /// <summary>
    /// Key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Script
    /// </summary>
    public string Script { get; set; }

    /// <summary>
    /// Status
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// At
    /// </summary>
    public DateTimeOffset At { get; set; }

    /// <summary>
    /// Migration Collection Identity
    /// </summary>
    public string CollectionId { get; set; }
}
