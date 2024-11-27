namespace Altinn.Authorization.Workers.BrReg.Services;

/// <summary>
/// BrReg Ingestor Config
/// </summary>
public class IngestorConfig
{
    /// <summary>
    /// IngestUnits
    /// </summary>
    public bool IngestUnits { get; set; }

    /// <summary>
    /// IngestSubUnits
    /// </summary>
    public bool IngestSubUnits { get; set; }

    /// <summary>
    /// IngestRoles
    /// </summary>
    public bool IngestRoles { get; set; }
}
