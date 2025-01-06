namespace Altinn.Authorization.Workers.ResReg.Models;

/// <summary>
/// Service configuration
/// </summary>
public class ResourceRegisterImportConfig
{
    /// <summary>
    /// Is the service enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Seconds to wait between imports
    /// </summary>
    public int Interval { get; set; } = 100000;
}
