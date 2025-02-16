using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.DbAccess.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessMgmt.DbAccess.Services;

public class MigrationScriptCollection
{
    public Type Type { get; set; }

    public OrderedDictionary<string, string> Scripts { get; set; }

    public Dictionary<Type, int> Dependencies { get; set; }

    public MigrationScriptCollection(Type type)
    {
        Type = type;
        Scripts = new OrderedDictionary<string, string>();
        Dependencies = new Dictionary<Type, int>();
    }


    public void AddScripts(OrderedDictionary<string, string> scripts)
    {
        foreach (var script in scripts)
        {
            Scripts.Add(script.Key, script.Value);
        }
    }
    public void AddScripts((string key, string query) keyValueSet)
    {
        Scripts.Add(keyValueSet.key, keyValueSet.query);
    }
    public void AddScripts(string key, string query)
    {
        Scripts.Add(key, query);
    }

    public void AddDependency(Type type)
    {
        if (Dependencies.ContainsKey(type))
        {
            Dependencies[type]++;
        }

        Dependencies.Add(type, 1);
    }
}

public class MigrationService
{
    protected readonly DbAccessConfig config;
    protected readonly NpgsqlDataSource connection;
    protected readonly IDbConverter dbConverter;

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

    //private void PrintScripts() { }
    //private void PrintScripts(Type type) { }
    //private void PrintScripts<T>() { }

    //private void ExportScripts<T>() { }
    //private void ExportScripts(Type type) { }
    //private void ExportScripts() { }

    */

    private Dictionary<Type, bool> TypesMigrated { get; set; } = [];

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
    public void Generate(string definitionNamespace) 
    {
        List<Type> types = [.. AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.Namespace == definitionNamespace)];

        foreach (var type in types)
        {
            Generate(type);
        }
    }
    
    public void Generate<T>() 
    {
        Generate(typeof(T));
    }
    
    public void Generate(Type type) 
    {
        if (!HasInitialized)
        {
            throw new Exception("MigrationService not initialized");
        }

        var dbDefinition = DefinitionStore.TryGetDefinition(type) ?? throw new Exception($"Definition for '{type.Name}' not found.");
        SqlQueryBuilder queryBuilder = new SqlQueryBuilder(dbDefinition);
        Scripts.Add(type, queryBuilder.GetMigrationScripts());
    }
    #endregion

    private async Task ExecuteMigration(int maxRetry = 3)
    {
        bool needRetry = false;
        int retryAttempts = 0;
        
        while (needRetry && retryAttempts < maxRetry)
        {
            foreach (var script in Scripts)
            {
                if(script.Value.Dependencies.Count == 0)
                {
                    await ExecuteMigration(script.Key, script.Value);
                }
                else
                {
                    bool ready = true;
                    foreach(var dep in script.Value.Dependencies)
                    {
                        if (!TypesMigrated.ContainsKey(dep.Key))
                        {
                            ready = false;
                            break;
                        }
                    }
                    if (ready)
                    {
                        try
                        {
                            await ExecuteMigration(script.Key, script.Value);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Migration '{script.Key.Name}' failed (adding to retry): {ex.Message}");
                            needRetry = true;
                        }
                    }
                    else
                    {
                        needRetry = true;
                    }
                }
            }
            retryAttempts++;
        }
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
                catch (Exception ex)
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
