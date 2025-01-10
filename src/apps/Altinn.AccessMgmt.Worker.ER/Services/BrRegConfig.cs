namespace Altinn.AccessMgmt.Worker.ER.Services;

/// <summary>
/// BrReg Config
/// </summary>
public class BrRegConfig
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

    /// <summary>
    /// ImportUnits
    /// </summary>
    public bool ImportUnits { get; set; }

    /// <summary>
    /// ImportSubUnits
    /// </summary>
    public bool ImportSubUnits { get; set; }

    /// <summary>
    /// ImportRoles
    /// </summary>
    public bool ImportRoles { get; set; }
}
