namespace Altinn.AccessMgmt.Persistence.Core.Models;

/// <summary>
/// Configuration settings for database access and migrations.
/// </summary>
public class DbAccessConfig
{
    /// <summary>
    /// Specifies the type of database to be used (e.g., "Postgres", "MSSQL").
    /// Default is "Postgres".
    /// </summary>
    public string DatabaseType { get; set; } = "Postgres";

    /// <summary>
    /// The connection string used to access the database.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The connection string used for administrative database operations
    /// (e.g., for schema migrations).
    /// </summary>
    public string ConnectionStringAdmin { get; set; }

    /// <summary>
    /// Indicates whether automatic database migrations are enabled.
    /// If true, the application will attempt to migrate the database schema at startup.
    /// </summary>
    public bool MigrationEnabled { get; set; }

    /// <summary>
    /// The key used to perform database migrations.
    /// </summary>
    public string MigrationKey { get; set; }

    /// <summary>
    /// A list of language codes (e.g., "en", "nb", "de") that should be considered
    /// when ingesting JSON-based data into the database.
    /// </summary>
    public List<string> JsonIngestLanguages { get; set; }

    /// <summary>
    /// The base path where JSON files used for ingestion are stored.
    /// Default location: "Ingest/JsonData/" within the application base directory.
    /// </summary>
    public string JsonBasePath { get; set; } = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Ingest/JsonData/");

    /// <summary>
    /// A dictionary that determines whether JSON ingestion is enabled for specific data sets.
    /// The key represents a data category, and the value determines if ingestion is enabled.
    /// </summary>
    public Dictionary<string, bool> JsonIngestEnabled { get; set; } = new Dictionary<string, bool>();

    /// <summary>
    /// Determines if the application should run in mock mode.
    /// If true, database operations might be replaced with in-memory mocks for testing purposes.
    /// </summary>
    public bool MockEnabled { get; set; }

    /// <summary>
    /// A dictionary defining which mock services should be run for specific data types.
    /// The key represents a data type, and the value determines if mocking is enabled for that type.
    /// </summary>
    public Dictionary<string, bool> MockRun { get; set; } = new Dictionary<string, bool>();
}
