using Npgsql;

namespace Altinn.Authorization.Host.Database;

public class AltinnHostDatabaseOptions
{
    public DataSource AppSource { get; set; }

    public DataSource MigrationSource { get; set; }

    public class DataSource(string connectionString)
    {
        public NpgsqlConnectionStringBuilder ConnectionString { get; } = new NpgsqlConnectionStringBuilder(connectionString);

        public Action<NpgsqlDataSourceBuilder> Builder { get; set; } = null;
    }
}

public enum SourceType
{
    Migration,

    App,
}
