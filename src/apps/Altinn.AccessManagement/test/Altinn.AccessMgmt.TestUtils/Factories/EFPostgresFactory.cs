using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.Authorization.Host.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit.Sdk;

namespace Altinn.AccessMgmt.TestUtils.Factories;

/// <summary>
/// Postgres singleton that creates a npg sql server and creates a new database for each test 
/// </summary>
public static class EFPostgresFactory
{
    private static PostgreSqlContainer Server { get; } = new PostgreSqlBuilder()
        .WithCleanUp(true)
        .WithImage("docker.io/postgres:16.1-alpine")
        .Build();

    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    private static bool _isInitialized = false;

    private static int _databaseInstance = 0;

    /// <summary>
    /// Database Password
    /// </summary>
    public static readonly string DbPassword = "Password";

    /// <summary>
    /// Database Username
    /// </summary>
    public static readonly string DbUserName = "platform_authorization";

    /// <summary>
    /// Database Admin
    /// </summary>
    public static readonly string DbAdminName = "platform_authorization_admin";

    /// <summary>
    /// Creates a new database instance cloned from a migrated template database
    /// </summary>
    /// <returns></returns>
    /// <exception cref="XunitException">gets raised if bootstrapping of primary db fails.</exception>
    public static async Task<PostgresDatabase> Create()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!_isInitialized)
            {
                await Server.StartAsync();

                const string templateDb = "test_primary";

                var roleResult = await Server.ExecScriptAsync($@"
                DO $$
                    BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '{DbUserName}') THEN
                        CREATE ROLE {DbUserName} LOGIN PASSWORD '{DbPassword}' SUPERUSER INHERIT;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = '{DbAdminName}') THEN
                        CREATE ROLE {DbAdminName} LOGIN PASSWORD '{DbPassword}' SUPERUSER INHERIT;
                    END IF;
                END $$;
                ");

                if (roleResult.ExitCode != 0 || !string.IsNullOrEmpty(roleResult.Stderr))
                {
                    throw new XunitException($"Role init failed. Exitcode {roleResult.ExitCode}, Error {roleResult.Stderr}");
                }

                var createDbResult = await Server.ExecScriptAsync($@"CREATE DATABASE {templateDb};");
                if (createDbResult.ExitCode != 0 || !string.IsNullOrEmpty(createDbResult.Stderr))
                {
                    throw new XunitException($"Create Db failed. Exitcode {createDbResult.ExitCode}, Error {createDbResult.Stderr}");
                }

                var connString = new PostgresDatabase(templateDb, Server.GetConnectionString());

                using var sp = new ServiceCollection()
                    .AddAccessManagementDatabase(opts =>
                    {
                        opts.MigrationConnectionString = connString.Admin.ToString();
                        opts.Source = SourceType.Migration;
                        opts.EnableEFPooling = false;
                    }).BuildServiceProvider();

                var audit = new AuditValues(SystemEntityConstants.StaticDataIngest);
                using var db = sp.CreateEFScope(audit).ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();
                await TestDataSeeds.Exec(db);
                NpgsqlConnection.ClearAllPools();
                _isInitialized = true;
            }
        }
        finally
        {
            _semaphore.Release();
        }

        var dbInstance = Interlocked.Increment(ref _databaseInstance);
        var dbName = $"test_{dbInstance}";

        var cloneResult = await Server.ExecScriptAsync($"CREATE DATABASE {dbName} WITH TEMPLATE test_primary OWNER {DbUserName};");

        if (cloneResult.ExitCode != 0 || !string.IsNullOrEmpty(cloneResult.Stderr))
        {
            throw new XunitException($"create database with template failed. Exitcode {cloneResult.ExitCode}, Error {cloneResult.Stderr}");
        }

        return new PostgresDatabase(dbName, Server.GetConnectionString());
    }
}

/// <summary>
/// Container for persisting connections string and database name
/// </summary>
public class PostgresDatabase(string dbname, string connectionString) : IOptions<PostgreSQLSettings>
{
    /// <summary>
    /// Admin name
    /// </summary>
    public NpgsqlConnectionStringBuilder Admin { get; } = new NpgsqlConnectionStringBuilder(connectionString)
    {
        Timeout = 3,
        Database = dbname,
        Username = EFPostgresFactory.DbAdminName,
        Password = EFPostgresFactory.DbPassword,
        IncludeErrorDetail = true,
        //// Pooling enabled (remove previous Pooling = false)
        Pooling = true,
        ConnectionIdleLifetime = 30,
        MinPoolSize = 0,
        MaxPoolSize = 50,
        ConnectionPruningInterval = 15,
    };

    /// <summary>
    /// User name
    /// </summary>
    public NpgsqlConnectionStringBuilder User { get; } = new NpgsqlConnectionStringBuilder(connectionString)
    {
        Timeout = 3,
        Database = dbname,
        Username = EFPostgresFactory.DbUserName,
        Password = EFPostgresFactory.DbPassword,
        IncludeErrorDetail = true,
        Pooling = true,
        ConnectionIdleLifetime = 30,
        MinPoolSize = 0,
        MaxPoolSize = 50,
        ConnectionPruningInterval = 15,
    };

    /// <summary>
    /// Implements <see cref="IOptions{PostgreSQLSettings}"/> 
    /// </summary>
    public PostgreSQLSettings Value => new()
    {
        ConnectionString = User.ToString(),
        AuthorizationDbPwd = User.Password
    };
}
