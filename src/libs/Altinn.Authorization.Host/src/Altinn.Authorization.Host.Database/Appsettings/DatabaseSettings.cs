using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.Host.Database.Appsettings;

/// <summary>
/// Appsettings for database settings.
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// Gets or sets the PostgreSQL-specific settings.
    /// </summary>
    public PostgresSettings Postgres { get; set; } = new();

    /// <summary>
    /// Specifies if application should migrate db and terminate.
    /// </summary>
    public bool MigrateDb { get; set; } = false;

    /// <summary>
    /// Migrates DB and terminates if true. Should be used with init containers.
    /// </summary>
    public bool MigrateDbAndTerminate { get; set; } = false;

    /// <summary>
    /// Contains settings related to PostgreSQL database connections.
    /// </summary>
    public class PostgresSettings
    {
        /// <summary>
        /// Gets or sets the application connection string used to connect to the database.
        /// </summary>
        public string AppConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the migration connection string used for database migrations.
        /// </summary>
        public string MigrationConnectionString { get; set; } = string.Empty;
    }
}
