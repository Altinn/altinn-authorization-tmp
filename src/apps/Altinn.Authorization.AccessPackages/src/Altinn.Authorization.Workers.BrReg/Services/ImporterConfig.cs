namespace Altinn.Authorization.Workers.BrReg.Services;

/// <summary>
/// BrReg Importer Config
/// </summary>
public class ImporterConfig
{
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
