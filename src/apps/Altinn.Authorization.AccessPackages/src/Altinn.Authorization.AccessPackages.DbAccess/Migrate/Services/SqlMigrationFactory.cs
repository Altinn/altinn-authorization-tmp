using System.Data;
using System.Data.SqlClient;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Altinn.Authorization.AccessPackages.DbAccess.Migrate.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Migrate.Models;
using Dapper;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.DbAccess.Migrate.Services;

/// <summary>
/// Sql Database Migration Factory
/// </summary>
public class SqlMigrationFactory : IDbMigrationFactory
{
    private readonly IDbConnection _connection;

    /// <summary>
    /// Default schema
    /// </summary>
    private readonly string defaultSchema = "";

    /// <inheritdoc/>
    public bool UseTranslation { get; set; }
    private readonly string translationPrefix = "";
    private readonly string translationSchema = "";

    /// <inheritdoc/>
    public bool UseHistory { get; set; }
    private readonly string historyPrefix = "";
    private readonly string historySchema = "";

    /// <summary>
    /// List of migrations executed
    /// </summary>
    private List<MigrationEntry> Migrations { get; set; }
    private readonly string _migrationId;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlMigrationFactory"/> class.
    /// </summary>
    /// <param name="options">DbMigrationConfig</param>
    public SqlMigrationFactory(IOptions<DbMigrationConfig> options)
    {
        var config = options.Value;

        if (!string.IsNullOrEmpty(translationSchema))
        {
            UseTranslation = true;
        }

        if (!string.IsNullOrEmpty(historySchema))
        {
            UseHistory = true;
        }

        _connection = new SqlConnection(config.ConnectionString);

        defaultSchema = config.DefaultSchema ?? "dbo";
        translationSchema = config.TranslationSchema ?? defaultSchema;
        historySchema = config.HistorySchema ?? defaultSchema;

        _migrationId = config.CollectionId;
        Migrations = new List<MigrationEntry>();
    }

    /// <summary>
    /// List of types that has history enabled
    /// </summary>
    public Dictionary<Type, bool> HasHistory { get; set; } = new Dictionary<Type, bool>();

    /// <summary>
    /// List of types that has translations enabled
    /// </summary>
    public Dictionary<Type, bool> HasTranslation { get; set; } = new Dictionary<Type, bool>();

    /// <inheritdoc/>
    public async Task Init()
    {
        try
        {
            await InitMigration();
            await GetMigrations();
        }
        catch (Exception ex)
        {
            LogError("Failed to load Migrations. " + ex.Message);
        }
    }

    private async Task InitMigration()
    {
        var checkQuery = "SELECT [TABLE_NAME] AS [Name] FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = '_Migration'";
        _connection.Open();
        var res = await _connection.ExecuteScalarAsync(checkQuery);
        if (res?.ToString() == "_Migration")
        {
            return;
        }

        _connection.Close();

        try
        {
            var query = "CREATE TABLE [dbo].[_Migration] (" +
                "[ObjectName] NVARCHAR(250) NOT NULL," +
                "[Key] NVARCHAR(500) NOT NULL," +
                "[At] DATETIMEOFFSET(7) NOT NULL," +
                "[Status] NVARCHAR(50) NOT NULL," +
                "[Script] NVARCHAR(MAX) NOT NULL," +
                "[CollectionId] NVARCHAR(64) NOT NULL" +
                ")";
            await _connection.ExecuteAsync(query);
            await LogMigration<MigrationEntry>($"CREATE TABLE {TableName<MigrationEntry>()}", query);
        }
        catch (Exception ex)
        {
            LogError(ex.Message);
        }
        finally
        {
            _connection.Close();
        }
    }

    private async Task GetMigrations()
    {
        Migrations = new List<MigrationEntry>();
        try
        {
            Migrations.AddRange(await _connection.QueryAsync<MigrationEntry>("SELECT * FROM [dbo].[_Migration] WHERE [CollectionId] = @CollectionId", new { CollectionId = _migrationId }));
        }
        catch (Exception ex)
        {
            if (Migrations.Count == 0)
            {
                await InitMigration();
            }

            LogWarning(ex.Message);
            return;
        }
        finally
        {
            _connection.Close();
        }
    }

    private bool NeedMigration<T>(string key)
    {
        return NeedMigration(key: key, objectName: typeof(T).Name);
    }

    private bool NeedMigration(string key, string objectName)
    {
        if (Migrations == null || Migrations.Count == 0)
        {
            throw new Exception("No migrations to check");
        }

        return !Migrations.Exists(t => t.ObjectName == objectName && t.Key == key);
    }

    private async Task LogMigration<T>(string key, string script)
    {
        await LogMigration(key: key, script: script, objectName: typeof(T).Name);
    }

    private async Task LogMigration(string key, string script, string objectName)
    {
        var m = new MigrationEntry();
        m.Key = key;
        m.At = DateTimeOffset.UtcNow;
        m.Status = "Executed";
        m.ObjectName = objectName;
        m.Script = script;
        m.CollectionId = _migrationId;
        LogInfo(key);
        await _connection.ExecuteAsync("INSERT [dbo].[_Migration] ([ObjectName], [Key], [At], [Status], [Script], [CollectionId]) VALUES(@ObjectName, @Key, @At, @Status, @Script, @CollectionId)", m);
        Migrations.Add(m);
    }

    /// <inheritdoc/>
    public async Task CreateFunction(string name, string query)
    {
        string migrationKey = $"CREATE FUNCTION [{defaultSchema}].[{name}]";
        if (NeedMigration(migrationKey, "_common"))
        {
            await ExecuteQuery(query);
            await LogMigration(migrationKey, query, "_common");
        }
    }

    /// <inheritdoc/>
    public async Task CreateSchema(string name)
    {
        string migrationKey = $"CREATE SCHEMA [{name}]";
        if (NeedMigration(migrationKey, "Schema"))
        {
            string query = $"CREATE SCHEMA [{name}]";
            await ExecuteQuery(query);
            await LogMigration(migrationKey, query, "Schema");
        }
    }

    /// <inheritdoc/>
    public async Task CreateTable<T>(bool withHistory = false, bool withTranslation = false, Dictionary<string, CommonDataType>? primaryKeyColumns = null)
    {
        string name = typeof(T).Name;
        string migrationKey = $"CREATE TABLE {TableName<T>()}";
        HasHistory.Add(typeof(T), withHistory);
        HasTranslation.Add(typeof(T), withTranslation);

        if (NeedMigration<T>(migrationKey))
        {
            primaryKeyColumns = primaryKeyColumns ?? new Dictionary<string, CommonDataType>() { { "Id", DataTypes.Guid } };
            foreach (var property in primaryKeyColumns)
            {
                if (!typeof(T).GetProperties().ToList().Exists(t => t.Name == property.Key))
                {
                    // TODO: Check datatype
                    LogError($"{typeof(T).Name} does not contain the property '{property.Key}'");
                    return;
                }
            }

            string primaryKeyDefinition = string.Join(',', primaryKeyColumns.Select(t => $"[{t.Key}] [{t.Value.MsSql}] NOT NULL"));
            string primaryKeyConstraint = $"CONSTRAINT [PK_{name}] PRIMARY KEY ({string.Join(',', primaryKeyColumns.Select(t => $"[{t.Key}]"))})";

            if (!withHistory)
            {
                string query = $"CREATE TABLE {TableName<T>()} (" + primaryKeyDefinition + " " + primaryKeyConstraint + ")";
                await ExecuteQuery(query);
                await LogMigration<T>(migrationKey, query);

                if (withTranslation)
                {
                    primaryKeyColumns.Add("Language", DataTypes.String(10));
                    primaryKeyConstraint = $"CONSTRAINT [PK_{name}] PRIMARY KEY ({string.Join(',', primaryKeyColumns.Select(t => $"[{t.Key}]"))})";

                    query = $"CREATE TABLE {TranslationTableName<T>()} (" + primaryKeyDefinition +
                    $"[Language] NVARCHAR(10) NOT NULL," +
                    primaryKeyConstraint + ")";
                    await ExecuteQuery(query);
                    await LogMigration<T>(migrationKey, query);
                }
            }

            if (withHistory)
            {
                string query = $"CREATE TABLE {TableName<T>()}(" + primaryKeyDefinition + "," +
                $"[ValidFrom] DATETIME2(7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL," +
                $"[ValidTo] DATETIME2(7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL," +
                $"PERIOD FOR SYSTEM_TIME([ValidFrom], [ValidTo])," +
                primaryKeyConstraint + ")" +
                $"WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = {HistoryTableName<T>()}, DATA_CONSISTENCY_CHECK = ON))";
                await ExecuteQuery(query);
                await LogMigration<T>(migrationKey, query);

                if (withTranslation)
                {
                    primaryKeyColumns.Add("Language", DataTypes.String(10));
                    primaryKeyConstraint = $"CONSTRAINT [PK_{name}] PRIMARY KEY ({string.Join(',', primaryKeyColumns.Select(t => $"[{t.Key}]"))})";

                    query = $"CREATE TABLE {TranslationTableName<T>()}(" + primaryKeyDefinition + "," +
                    $"[Language] NVARCHAR(10) NOT NULL," +
                    $"[ValidFrom] DATETIME2(7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL," +
                    $"[ValidTo] DATETIME2(7) GENERATED ALWAYS AS ROW END HIDDEN NOT NULL," +
                    $"PERIOD FOR SYSTEM_TIME([ValidFrom], [ValidTo])," +
                    primaryKeyConstraint + ")" +
                    $"WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = {HistoryTableName<T>("T_")}, DATA_CONSISTENCY_CHECK = ON))";
                    await ExecuteQuery(query);
                    await LogMigration<T>(migrationKey, query);
                }
            }
        }
    }

    /// <inheritdoc/>
    public async Task CreateColumn<T>(string name, CommonDataType dbType, bool nullable = false, string? defaultValue = null)
    {
        string dbTypeString = dbType.MsSql;
        string migrationKey = $"ADD COLUMN {TableName<T>()}.[{name}]";
        if (NeedMigration<T>(migrationKey))
        {
            if (nullable && string.IsNullOrEmpty(defaultValue))
            {
                LogWarning("A nullable column with no default value will fail if table is not empty.");
            }

            string query = $"ALTER TABLE {TableName<T>()} ADD [{name}] {dbTypeString} {(nullable ? "NULL" : "NOT NULL")} {(string.IsNullOrEmpty(defaultValue) ? "" : $"DEFAULT('{defaultValue}')")};";
            await ExecuteQuery(query);
            await LogMigration<T>($"ADD COLUMN [{defaultSchema}].[{typeof(T).Name}].[{name}]", query);

            if (HasTranslation[typeof(T)])
            {
                query = $"ALTER TABLE {TranslationTableName<T>()} ADD [{name}] {dbTypeString} NULL {(string.IsNullOrEmpty(defaultValue) ? "" : $"DEFAULT('{defaultValue}')")};";
                await ExecuteQuery(query);
                await LogMigration<T>($"ADD COLUMN {TableName<T>()} [{defaultSchema}].[{typeof(T).Name}].[{name}]", query);
            }
        }
    }

    /// <inheritdoc/>
    public async Task CreateUniqueConstraint<T>(string[] propertyNames)
    {
        foreach (var property in propertyNames)
        {
            if (!typeof(T).GetProperties().ToList().Exists(t => t.Name == property))
            {
                LogError($"{typeof(T).Name} does not contain the property '{property}'");
                return;
            }
        }

        var migrationKey = $"ADD CONSTRAINT {TableName<T>()}.[UC_{typeof(T).Name}]";
        if (NeedMigration<T>(migrationKey))
        {
            var query = $"ALTER TABLE {TableName<T>()} ADD CONSTRAINT [UC_{typeof(T).Name}] UNIQUE ({string.Join(',', propertyNames.Select(t => $"[{t}]"))})";
            await ExecuteQuery(query);
            await LogMigration<T>(migrationKey, query);
        }
    }

    /// <inheritdoc/>
    public async Task CreateForeignKeyConstraint<TSource, TTarget>(string sourceProperty, string targetProperty = "Id")
    {
        if (!typeof(TSource).GetProperties().ToList().Exists(t => t.Name == sourceProperty))
        {
            LogError($"{typeof(TSource).Name} does not contain the property '{sourceProperty}'");
        }

        if (!typeof(TTarget).GetProperties().ToList().Exists(t => t.Name == targetProperty))
        {
            LogError($"{typeof(TSource).Name} does not contain the property '{targetProperty}'");
        }

        var migrationKey = $"ADD CONSTRAINT {TableName<TSource>()}.[FK_{typeof(TSource).Name}_{sourceProperty}]";
        if (NeedMigration<TSource>(migrationKey))
        {
            var query = $"ALTER TABLE {TableName<TSource>()} ADD CONSTRAINT [FK_{typeof(TSource).Name}_{sourceProperty}] FOREIGN KEY ([{sourceProperty}]) REFERENCES {TableName<TTarget>()}([{targetProperty}])";
            await ExecuteQuery(query);
            await LogMigration<TSource>(migrationKey, query);
        }
    }

    private string TableName<T>() => $"[{defaultSchema}].[{typeof(T).Name}]";
    private string TranslationTableName<T>() => $"[{translationSchema}].[{translationPrefix}{typeof(T).Name}]";
    private string HistoryTableName<T>(string prefix = "") => $"[{historySchema}].[{historyPrefix}{prefix}{typeof(T).Name}]";
    private async Task ExecuteQuery(string query)
    {
        try
        {
            await _connection.ExecuteAsync(query);
        }
        catch (Exception ex)
        {
            LogWarning(query);
            LogError(ex.Message);
            throw;
        }
    }

    private void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("ERROR: " + message);
        Console.ForegroundColor = ConsoleColor.White;
    }

    private void LogWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("WARN: " + message);
        Console.ForegroundColor = ConsoleColor.White;
    }

    private void LogInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("INFO: " + message);
        Console.ForegroundColor = ConsoleColor.White;
    }
}
