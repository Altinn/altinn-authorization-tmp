namespace Altinn.Authorization.AccessPackages.CLI;

/// <summary>
/// Configuratgion for CLI
/// </summary>
public class CLIConfig
{
    /// <summary>
    /// EnableMigrations
    /// </summary>
    public bool EnableMigrations { get; set; }

    /// <summary>
    /// EnableJsonIngest
    /// </summary>
    public bool EnableJsonIngest { get; set; }

    /// <summary>
    /// RunTests
    /// </summary>
    public bool RunTests { get; set; }
}
