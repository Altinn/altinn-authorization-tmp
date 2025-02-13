using System.Collections;
using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.DbAccess.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessMgmt.DbAccess.Services;

public class MigrationService
{
    protected readonly DbAccessConfig config;
    protected readonly NpgsqlDataSource connection;
    protected readonly IDbConverter dbConverter;

    private List<MigrationEntry> Migrations { get; set; } = new List<MigrationEntry>();
    public Dictionary<Type, List<DictionaryEntry>> RetryQueue { get; set; } = new();

    public MigrationService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter)
    {
        config = options.Value;
        this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
        this.dbConverter = dbConverter;
        Migrations = new List<MigrationEntry>();
    }

    private bool HasInitialized { get; set; } = false;
    private async Task Init(CancellationToken cancellationToken = default)
    {
        if (HasInitialized)
        {
            return;
        }

        var executor = new DbExecutor(connection, dbConverter);

        var defaultDefinition = new DbDefinition(typeof(string));

        await executor.ExecuteCommand($"CREATE SCHEMA IF NOT EXISTS {defaultDefinition.BaseSchema};", new List<NpgsqlParameter>(), cancellationToken);
        await executor.ExecuteCommand($"CREATE SCHEMA IF NOT EXISTS {defaultDefinition.TranslationSchema};", new List<NpgsqlParameter>(), cancellationToken);
        await executor.ExecuteCommand($"CREATE SCHEMA IF NOT EXISTS {defaultDefinition.BaseHistorySchema};", new List<NpgsqlParameter>(), cancellationToken);
        await executor.ExecuteCommand($"CREATE SCHEMA IF NOT EXISTS {defaultDefinition.TranslationHistorySchema};", new List<NpgsqlParameter>(), cancellationToken);

        var migrationTable = """
        CREATE TABLE IF NOT EXISTS dbo._migration (
        ObjectName text NOT NULL,
        Key text NOT NULL,
        At timestamptz NOT NULL,
        Status text NOT NULL,
        Script text NOT NULL,
        CollectionId text NOT NULL
        );
        """;

        await executor.ExecuteCommand(migrationTable, new List<NpgsqlParameter>(), cancellationToken);

        Migrations = [.. await executor.ExecuteQuery<MigrationEntry>("SELECT * FROM dbo._migration", new Dictionary<string, object>())];

        HasInitialized = true;
    }

    private bool NeedMigration<T>(string key)
    {
        return NeedMigration(key: key, objectName: typeof(T).Name);
    }

    private bool NeedMigration(string key, string objectName)
    {
        if (Migrations == null)
        {
            throw new Exception("Migrations not initialize");
        }

        return !Migrations.Exists(t => t.ObjectName == objectName && t.Key == key);
    }

    private async Task LogMigration<T>(string key, string script)
    {
        await LogMigration(key: key, script: script, objectName: typeof(T).Name);
    }

    private async Task LogMigration(string key, string script, string objectName, CancellationToken cancellationToken = default)
    {
        var migrationEntry = new MigrationEntry
        {
            Key = key,
            At = DateTimeOffset.UtcNow,
            Status = "Executed",
            ObjectName = objectName,
            Script = script,
            CollectionId = "v1"
        };

        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("Key", key),
            new NpgsqlParameter("At", DateTimeOffset.UtcNow),
            new NpgsqlParameter("Status", "Executed"),
            new NpgsqlParameter("ObjectName", objectName),
            new NpgsqlParameter("Script", script),
            new NpgsqlParameter("CollectionId", "v1")
        };

        var dbExec = new DbExecutor(connection, dbConverter);
        await dbExec.ExecuteCommand("INSERT INTO dbo._migration (ObjectName, Key, At, Status, Script, CollectionId) VALUES(@ObjectName, @Key, @At, @Status, @Script, @CollectionId)", parameters, cancellationToken);
        Migrations.Add(migrationEntry);
        Console.WriteLine(key);
    }

    public async Task RetryMigrate()
    {

    }

    /// <summary>
    /// Migrate Type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task Migrate<T>()
    {
        await Init();

        var dbDefinition = DefinitionStore.TryGetDefinition<T>() ?? throw new Exception($"Definition for '{nameof(T)}' not found.");
        SqlQueryBuilder queryBuilder = new SqlQueryBuilder(dbDefinition);
        var scripts = queryBuilder.GetMigrationScripts();

        var executor = new DbExecutor(connection, dbConverter);

        foreach (DictionaryEntry script in scripts)
        {
            string key = script.Key.ToString() ?? throw new Exception("Missing migration key");
            string value = script.Value?.ToString() ?? throw new Exception($"Script missing for {key}");

            if (NeedMigration<T>(key))
            {
                try
                {
                    await executor.ExecuteCommand(value, new List<NpgsqlParameter>());
                    await LogMigration<T>(key, value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Migration '{key}' failed: {ex.Message}");
                    if (!RetryQueue.ContainsKey(dbDefinition.BaseType))
                    {
                        RetryQueue.Add(dbDefinition.BaseType, new List<DictionaryEntry>());
                    }
                    RetryQueue[dbDefinition.BaseType].Add(script);
                }
            }
        }
    }
}
