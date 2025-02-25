namespace Altinn.AccessMgmt.Core.Models;

/// <summary>
/// Hold running configuration for workers
/// </summary>
public class WorkerConfig
{
    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Value
    /// </summary>
    public string Value { get; set; }
}
