using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.DbAccess.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessMgmt.DbAccess.Services;

/// <summary>
/// Service for handling migrations
/// </summary>
public class MigrationService
{
    /// <summary>
    /// Configuration
    /// </summary>
    private readonly DbAccessConfig config;

    /// <summary>
    /// Connection
    /// </summary>
    private readonly NpgsqlDataSource connection;

    /// <summary>
    /// Database converter
    /// </summary>
    private readonly IDbConverter dbConverter;

    /// <summary>
    /// Migration Services
    /// </summary>
    /// <param name="options">DbAccessConfig</param>
    /// <param name="connection">NpgsqlDataSource</param>
    /// <param name="dbConverter">IDbConverter</param>
    public MigrationService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter)
    {
        config = options.Value;
        this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
        this.dbConverter = dbConverter;
        Migrations = new List<MigrationEntry>();
    }

    /*
    
    Start migration either by namespace or by type.
    After all types are added to the queue, run execute/print/export.

    TODO: new out methods

    //private void PrintScripts() { }
    //private void PrintScripts(Type type) { }
    //private void PrintScripts<T>() { }

    //private void ExportScripts<T>() { }
    //private void ExportScripts(Type type) { }
    //private void ExportScripts() { }

    */

    private Dictionary<Type, MigrationScriptCollection> Scripts { get; set; } = [];

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

    /// <summary>
    /// Migrate
    /// </summary>
    /// <returns></returns>
    public async Task Migrate()
    {
        if (Scripts.Count == 0)
        {
            throw new Exception("Nothing to migrate. Remember to generate first.");
        }

        await Init();
        await ExecuteMigration();
    }

    #region Generate

    /// <summary>
    /// Generate migration scripts for all types in a namespace
    /// </summary>
    /// <param name="definitionNamespace">Namespace</param>
    public void Generate(string definitionNamespace) 
    {
        List<Type> types = [.. AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.Namespace == definitionNamespace && type.BaseType == typeof(object))];

        foreach (var type in types)
        {
            Console.WriteLine(type.FullName);
            Generate(type);
        }
    }

    /// <summary>
    /// Generate migration scripts for a type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void Generate<T>() 
    {
        Generate(typeof(T));
    }

    /// <summary>
    /// Generate migration scripts for a type
    /// </summary>
    /// <param name="type">Type</param>
    public void Generate(Type type) 
    {
        var dbDefinition = DefinitionStore.TryGetDefinition(type); // ?? throw new Exception($"Definition for '{type.Name}' not found.");
        if (dbDefinition == null)
        {
            Console.WriteLine($"Definition for '{type.Name}' not found.");
            return;
        }

        SqlQueryBuilder queryBuilder = new SqlQueryBuilder(dbDefinition);
        Scripts.Add(type, queryBuilder.GetMigrationScripts());
    }
    #endregion

    private async Task ExecuteMigration(int maxRetry = 10, CancellationToken cancellationToken = default)
    {
        var status = new Dictionary<Type, bool>();
        var retry = new Dictionary<Type, int>(); // Add reson ... For log
        var failed = new Dictionary<Type, string>();

        foreach (var key in Scripts.Keys)
        {
            status[key] = false;
            retry[key] = 0;
        }

        while (status.Values.Contains(false) && !failed.Any() && !cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("============LOOP==============");
            foreach (var script in Scripts)
            {
                if (status[script.Key])
                {
                    continue;
                }

                if (retry[script.Key] > maxRetry)
                {
                    failed[script.Key] = "Max retry reached";
                    continue;
                }

                if (!script.Value.Dependencies.Any())
                {
                    try
                    {
                        await ExecuteMigration(script.Key, script.Value);
                        status[script.Key] = true;
                        retry[script.Key] = 0;
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Migration '{script.Key.Name}' failed");
                        Console.WriteLine(ex.Message);
                        retry[script.Key]++;
                        continue;
                    }
                }

                bool needMigration = script.Value.Scripts.Keys.Any(a => NeedMigration(script.Key, a));

                if (!needMigration)
                {
                    status[script.Key] = true;
                    retry[script.Key] = 0;
                    continue;
                }

                // Check if all dependencies are migrated
                bool ready = script.Value.Dependencies.All(dep => status.ContainsKey(dep.Key) && status[dep.Key]);

                if (!ready)
                {
                    Console.WriteLine($"Migration '{script.Key.Name}' not ready");

                    foreach (var dep in script.Value.Dependencies)
                    {
                        Console.WriteLine("Dep:" + dep.Key.Name);
                    }

                    retry[script.Key]++;
                    continue;
                }

                try
                {
                    await ExecuteMigration(script.Key, script.Value);
                    status[script.Key] = true;
                    retry[script.Key] = 0;
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Migration '{script.Key.Name}' failed: {ex.Message}");
                    retry[script.Key]++;
                    continue;
                }
            }

            Console.WriteLine("Migrationstatus:");
            foreach (var key in status.Keys)
            {
                if (status[key])
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }

                Console.WriteLine($"Migration '{key.Name}' status: {status[key]}");
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        Console.WriteLine("============FINAL==============");
        Console.WriteLine("Migrationstatus:");
        foreach (var key in status.Keys)
        {
            if (status[key])
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.WriteLine($"Migration '{key.Name}' status: {status[key]}");
        }

        foreach (var f in failed)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Migration '{f.Key.Name}' error: {f.Value}");
        }

        Console.ForegroundColor = ConsoleColor.White;
    }

    private async Task ExecuteMigration(Type type, MigrationScriptCollection collection)
    {
        var dbDefinition = DefinitionStore.TryGetDefinition(type) ?? throw new Exception($"Definition for '{type.Name}' not found.");
        var executor = new DbExecutor(connection, dbConverter);
        foreach (var script in collection.Scripts)
        {
            if (NeedMigration(type, script.Key))
            {
                try
                {
                    await executor.ExecuteCommand(script.Value, new List<NpgsqlParameter>());
                    await LogMigration(type, script.Key, script.Value);
                }
                catch
                {
                    Console.WriteLine($"Migration '{script.Key}' failed");
                    Console.WriteLine(script.Value);
                    throw;
                }
            }
        }
    }

    private List<MigrationEntry> Migrations { get; set; } = [];

    private Dictionary<Type, List<KeyValuePair<string,string>>> RetryQueue { get; set; } = [];

    private bool NeedMigration<T>(string key)
    {
        return NeedMigration(key: key, objectName: typeof(T).Name);
    }

    private bool NeedMigration(Type type, string key)
    {
        return NeedMigration(key: key, objectName: type.Name);
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

    private async Task LogMigration(Type type, string key, string script)
    {
        await LogMigration(key: key, script: script, objectName: type.Name);
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
}
