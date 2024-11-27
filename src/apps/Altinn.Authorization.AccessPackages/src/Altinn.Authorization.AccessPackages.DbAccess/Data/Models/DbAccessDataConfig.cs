namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

/// <summary>
/// DbAccess Data Config
/// </summary>
public class DbAccessDataConfig
{
    /// <summary>
    /// Constructor
    /// </summary>
    public DbAccessDataConfig() { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configureOptions">DbAccessDataConfig</param>
    public DbAccessDataConfig(Action<DbAccessDataConfig> configureOptions)
    {
        configureOptions?.Invoke(this);
    }

    /// <summary>
    /// ConnectionString
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// UseSqlServer
    /// </summary>
    public bool UseSqlServer { get; set; }
}
