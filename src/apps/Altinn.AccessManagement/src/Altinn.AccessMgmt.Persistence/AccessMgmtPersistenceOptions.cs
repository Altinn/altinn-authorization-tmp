namespace Altinn.AccessMgmt.Persistence;

/// <summary>
/// Represents configuration options for Access Management Persistence.
/// </summary>
public class AccessMgmtPersistenceOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the persistence layer is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the type of database used for persistence.
    /// </summary>
    public MgmtDbType DbType { get; set; } = MgmtDbType.Postgres;
}

/// <summary>
/// Enumerates the available database types for Access Management Persistence.
/// </summary>
public enum MgmtDbType
{
    /// <summary>
    /// PostgreSQL database.
    /// </summary>
    Postgres = default,

    /// <summary>
    /// Microsoft SQL Server database.
    /// </summary>
    MSSQL
}
