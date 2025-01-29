using System;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Altinn.AccessMgmt.DbAccess.Data.Models;
using Altinn.AccessMgmt.DbAccess.Migrate.Contracts;
using Altinn.AccessMgmt.DbAccess.Migrate.Models;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessMgmt.DbAccess.Migrate.Services;

/// <summary>
/// Postgres Database Migration Factory
/// </summary>
public class PostgresMigrationFactory : IDbMigrationFactory
{
    private readonly NpgsqlConnection _connection;

    private readonly string defaultSchema = "dbo";
    private readonly string translationSchema = "translation";

    /// <inheritdoc/>
    public bool Enable { get; set; }

    /// <summary>
    /// Migrations executed
    /// </summary>
    private List<MigrationEntry> Migrations { get; set; }

    private readonly string _migrationId;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresMigrationFactory"/> class.
    /// </summary>
    /// <param name="options">DbMigrationConfig</param>
    public PostgresMigrationFactory(IOptions<DbMigrationConfig> options)
    {
        var config = options.Value;

        Enable = config.Enable;

        _connection = new NpgsqlConnection(config.ConnectionString);

        defaultSchema = config.DefaultSchema ?? "dbo";
        translationSchema = config.TranslationSchema ?? "translation";

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

    /// <summary>
    /// Holds list of columns to be used for triggers
    /// </summary>
    public Dictionary<Type, Dictionary<string, CommonDataType>> Columns { get; set; } = new Dictionary<Type, Dictionary<string, CommonDataType>>();

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

    #region Migrations
    private async Task CreateMigrationSchema()
    {
        var checkQuery = "CREATE SCHEMA IF NOT EXISTS dbo;";
        _connection.Open();
        await _connection.ExecuteAsync(checkQuery);
        _connection.Close();
    }

    private async Task InitMigration()
    {
        var checkQuery = "SELECT TABLE_NAME AS Name FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = '_migration'";
        _connection.Open();
        var res = await _connection.ExecuteScalarAsync(checkQuery);
        if (res?.ToString() == "_migration")
        {
            return;
        }

        _connection.Close();

        try
        {
            await CreateMigrationSchema();
            var query = "CREATE TABLE dbo._migration (" +
                "ObjectName text NOT NULL," +
                "Key text NOT NULL," +
                "At timestamp NOT NULL," +
                "Status text NOT NULL," +
                "Script text NOT NULL," +
                "CollectionId text NOT NULL" +
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
            Migrations.AddRange(await _connection.QueryAsync<MigrationEntry>("SELECT * FROM dbo._migration WHERE collectionid = @CollectionId", new { CollectionId = _migrationId }));
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
        var migrationEntry = new MigrationEntry();
        migrationEntry.Key = key;
        migrationEntry.At = DateTimeOffset.UtcNow;
        migrationEntry.Status = "Executed";
        migrationEntry.ObjectName = objectName;
        migrationEntry.Script = script;
        migrationEntry.CollectionId = _migrationId;
        LogInfo(key);
        await _connection.ExecuteAsync("INSERT INTO dbo._migration (ObjectName, Key, At, Status, Script, CollectionId) VALUES(@ObjectName, @Key, @At, @Status, @Script, @CollectionId)", migrationEntry);
        Migrations.Add(migrationEntry);
    }
    #endregion

    /// <inheritdoc/>
    public async Task CreateFunction(string name, string query)
    {
        string migrationKey = $"CREATE FUNCTION {defaultSchema}.{name}";
        if (NeedMigration(migrationKey, "_common"))
        {
            await ExecuteQuery(query);
            await LogMigration(migrationKey, query, "_common");
        }
    }

    /// <inheritdoc/>
    public async Task CreateView<T>(string name, string query)
    {
        string migrationKey = $"CREATE VIEW {defaultSchema}.{name}";
        if (NeedMigration<T>(migrationKey))
        {
            // await ExecuteQuery(query);
            await LogMigration<T>(migrationKey, query);
        }
    }

    /// <inheritdoc/>
    public async Task CreateSchema(string name)
    {
        string migrationKey = $"CREATE SCHEMA {name}";
        if (NeedMigration(migrationKey, "Schema"))
        {
            string query = $"CREATE SCHEMA IF NOT EXISTS {name}";
            await ExecuteQuery(query);
            await LogMigration(migrationKey, query, "Schema");
        }
    }

    private bool HasHistoryCheck<T>()
    {
        if (HasHistory.ContainsKey(typeof(T)) && HasHistory[typeof(T)])
        {
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task CreateTable<T>(bool useTranslation = false, Dictionary<string, CommonDataType>? primaryKeyColumns = null)
    {
        string name = typeof(T).Name;
        string migrationKey = $"CREATE TABLE {TableName<T>()}";

        HasTranslation.Add(typeof(T), useTranslation);

        Columns.Add(typeof(T), new Dictionary<string, CommonDataType>());

        primaryKeyColumns ??= new Dictionary<string, CommonDataType>() { { "Id", DataTypes.Guid } };
        foreach (var column in primaryKeyColumns)
        {
            Columns[typeof(T)].Add(column.Key, column.Value);
        }

        var properties = typeof(T).GetProperties().ToList();
        foreach (var property in primaryKeyColumns)
        {
            if (!properties.Exists(t => t.Name == property.Key))
            {
                // TODO: Check datatype
                LogError($"PK: {typeof(T).Name} does not contain the property '{property.Key}'");
                return;
            }
        }

        string primaryKeyDefinition = string.Join(',', primaryKeyColumns.Select(t => $"{t.Key} {t.Value.Postgres} NOT NULL "));

        if (NeedMigration<T>(migrationKey))
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE {TableName<T>()} (");
            sb.AppendLine(primaryKeyDefinition);

            if (HasHistoryCheck<T>())
            {
                sb.AppendLine(", validfrom timestamptz default now()");
            }

            sb.AppendLine($", CONSTRAINT PK_{name} PRIMARY KEY ({string.Join(',', primaryKeyColumns.Select(t => $"{t.Key}"))})");
            sb.AppendLine(")");
            string query = sb.ToString();

            await ExecuteQuery(query);
            await LogMigration<T>(migrationKey, query);
        }

        if (useTranslation)
        {
            string translationKey = $"CREATE TABLE {TranslationTableName<T>()}";
            if (NeedMigration<T>(translationKey))
            {
                var sb = new StringBuilder();
                sb.AppendLine($"CREATE TABLE {TranslationTableName<T>()} (");
                sb.AppendLine(primaryKeyDefinition);
                if (HasHistoryCheck<T>())
                {
                    sb.AppendLine(", validfrom timestamptz default now()");
                }

                sb.AppendLine($", Language text NOT NULL");
                sb.AppendLine($", CONSTRAINT PK_{name} PRIMARY KEY ({string.Join(',', primaryKeyColumns.Select(t => $"{t.Key}"))}, Language)");
                sb.AppendLine(")");
                string query = sb.ToString();

                await ExecuteQuery(query);
                await LogMigration<T>(translationKey, query);
            }
        }

        if (HasHistoryCheck<T>())
        {
            string historyKey = $"CREATE TABLE {HistoryTableName<T>()}";
            if (NeedMigration<T>(historyKey))
            {
                var sb = new StringBuilder();
                sb.AppendLine($"CREATE TABLE {HistoryTableName<T>()}(");
                sb.AppendLine(primaryKeyDefinition);
                sb.AppendLine(", validfrom timestamptz default now()");
                sb.AppendLine(", validto timestamptz default now()");
                sb.AppendLine(")");
                string query = sb.ToString();

                await ExecuteQuery(query);
                await LogMigration<T>(historyKey, query);
            }

            if (useTranslation)
            {
                string historyTranslationKey = $"CREATE TABLE {HistoryTranslationTableName<T>()}";
                if (NeedMigration<T>(historyTranslationKey))
                {                     
                    var sb = new StringBuilder();
                    sb.AppendLine($"CREATE TABLE {HistoryTranslationTableName<T>()}(");
                    sb.AppendLine(primaryKeyDefinition);
                    sb.AppendLine(", validfrom timestamptz default now()");
                    sb.AppendLine(", validto timestamptz default now()");
                    sb.AppendLine(", Language text NOT NULL");
                    sb.AppendLine(")");
                    string query = sb.ToString();

                    await ExecuteQuery(query);
                    await LogMigration<T>(historyTranslationKey, query);
                }
            }
        }
    }

    /// <inheritdoc/>
    public async Task CreateColumn<T>(Expression<Func<T, object?>> TProperty, CommonDataType dbType, bool nullable = false, string? defaultValue = null)
    {
        var name = ExtractPropertyInfo(TProperty as Expression<Func<T, object>>).Name;
        if (nullable && string.IsNullOrEmpty(defaultValue))
        {
            LogWarning($"A nullable column with no default value will fail if table is not empty. ({TableName<T>()}.{name})");
        }

        await CreateColumn<T>(name: name, dbType: dbType, nullable: nullable, defaultValue: string.IsNullOrEmpty(defaultValue) ? null : $"'{defaultValue}'");
    }

    private async Task CreateColumn<T>(string name, CommonDataType dbType, bool nullable = false, string? defaultValue = null)
    {
        Columns[typeof(T)].Add(name, dbType);

        await CreateColumn(typeof(T).Name, TableName<T>(), name, dbType, nullable: nullable, defaultValue: defaultValue);
        
        if (HasTranslation[typeof(T)])
        {
            await CreateColumn(typeof(T).Name, TranslationTableName<T>(), name, dbType, nullable: true, defaultValue: defaultValue);
        }

        if (HasHistoryCheck<T>())
        {
            await CreateColumn(typeof(T).Name, HistoryTableName<T>(), name, dbType, nullable: true, defaultValue: defaultValue);
            if (HasTranslation[typeof(T)])
            {
                await CreateColumn(typeof(T).Name, HistoryTranslationTableName<T>(), name, dbType, nullable: true, defaultValue: defaultValue);
            }
        }
    }
   
    private async Task CreateColumn(string objectName, string tableName, string columnName, CommonDataType dbType, bool nullable = false, string? defaultValue = null)
    {
        string key = $"ADD COLUMN {tableName}.{columnName}";
        string dbTypeString = dbType.Postgres;

        if (NeedMigration(key, objectName))
        {
            var query = $"ALTER TABLE {tableName} ADD {columnName} {dbTypeString} {(nullable ? "NULL" : "NOT NULL")} {(string.IsNullOrEmpty(defaultValue) ? "" : $"DEFAULT {defaultValue}")};";
            Console.WriteLine(query);

            await ExecuteQuery(query);
            await LogMigration(key, query, objectName);
        }
    }

    /// <inheritdoc/>
    public async Task RemoveColumn<T>(string name)
    {
        if (!IsValidColumnName(name))
        {
            throw new ArgumentException("Not a valid column name", nameof(name));
        }

        // Only remove if prev added

        string migrationKey = $"DROP COLUMN {TableName<T>()}.{name}";
        if (NeedMigration<T>(migrationKey))
        {
            string query = $"ALTER TABLE {TableName<T>()} DROP COLUMN IF EXISTS {name};";
            await ExecuteQuery(query);
            await LogMigration<T>(migrationKey, query);
        }

        if (HasTranslation[typeof(T)])
        {
            string translationKey = $"DROP COLUMN {TranslationTableName<T>()}.{name}";
            if (NeedMigration<T>(translationKey))
            {
                string query = $"ALTER TABLE {TranslationTableName<T>()} DROP COLUMN IF EXISTS {name};";
                await ExecuteQuery(query);
                await LogMigration<T>(translationKey, query);
            }
        }

        if (HasHistoryCheck<T>())
        {
            string historyKey = $"DROP COLUMN {HistoryTableName<T>()}.{name}";
            if (NeedMigration<T>(historyKey))
            {
                string query = $"ALTER TABLE {HistoryTableName<T>()} DROP COLUMN IF EXISTS {name};";
                await ExecuteQuery(query);
                await LogMigration<T>(historyKey, query);
            }

            if (HasTranslation[typeof(T)])
            {
                string translationKey = $"DROP COLUMN {HistoryTranslationTableName<T>()}.{name}";
                if (NeedMigration<T>(translationKey))
                {
                    string query = $"ALTER TABLE {HistoryTranslationTableName<T>()} DROP COLUMN IF EXISTS {name};";
                    await ExecuteQuery(query);
                    await LogMigration<T>(translationKey, query);
                }
            }
        }
    }

    /// <inheritdoc/>
    public async Task CreateUniqueConstraint<T>(IEnumerable<Expression<Func<T, object?>>> properties)
    {
        var propertyNames = new List<string>();
        foreach (var property in properties)
        {
            var propertyName = ExtractPropertyInfo(property as Expression<Func<T, object>>).Name;
            propertyNames.Add(propertyName);

            if (!typeof(T).GetProperties().ToList().Exists(t => t.Name == propertyName))
            {
                LogError($"{typeof(T).Name} does not contain the property '{propertyName}'");
                return;
            }
        }

        var migrationKey = $"ADD CONSTRAINT {TableName<T>()}.UC_{typeof(T).Name}";
        if (NeedMigration<T>(migrationKey))
        {
            var query = $"ALTER TABLE {TableName<T>()} ADD CONSTRAINT UC_{typeof(T).Name} UNIQUE ({string.Join(',', propertyNames.Select(t => $"{t}"))})";
            await ExecuteQuery(query);
            await LogMigration<T>(migrationKey, query);
        }
    }

    /// <inheritdoc/>
    public async Task CreateForeignKeyConstraint<TSource, TTarget>(Expression<Func<TSource, object?>> TSourceProperty, Expression<Func<TTarget, object?>>? TTargetProperty = null, bool cascadeDelete = false)
    {
        var sourceProperty = ExtractPropertyInfo(TSourceProperty as Expression<Func<TSource, object>>).Name;
        var targetProperty = TTargetProperty == null ? "Id" : ExtractPropertyInfo(TTargetProperty as Expression<Func<TTarget, object>>).Name ?? "Id";

        if (!typeof(TSource).GetProperties().ToList().Exists(t => t.Name == sourceProperty))
        {
            LogError($"{typeof(TSource).Name} does not contain the property '{sourceProperty}'");
        }

        if (!typeof(TTarget).GetProperties().ToList().Exists(t => t.Name == targetProperty))
        {
            LogError($"{typeof(TSource).Name} does not contain the property '{targetProperty}'");
        }

        var migrationKey = $"ADD CONSTRAINT {TableName<TSource>()}.FK_{typeof(TSource).Name}_{sourceProperty}";
        if (NeedMigration<TSource>(migrationKey))
        {
            var query = $"ALTER TABLE {TableName<TSource>()} ADD CONSTRAINT FK_{typeof(TSource).Name}_{sourceProperty} FOREIGN KEY ({sourceProperty}) REFERENCES {TableName<TTarget>()} ({targetProperty}) {(cascadeDelete ? "ON DELETE CASCADE" : "ON DELETE SET NULL")}";
            await ExecuteQuery(query);
            await LogMigration<TSource>(migrationKey, query);
        }
    }

    private async Task CreateSharedHistoryFunction()
    {
        string key = "FUNCTION update_validfrom";
        if (NeedMigration(key, "dbo"))
        {
            string query = """
            CREATE OR REPLACE FUNCTION dbo.update_validfrom() RETURNS trigger AS
            $$
            BEGIN
              NEW.validfrom := NOW();
              RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;
            """;
            await ExecuteQuery(query);
            await LogMigration(key, query, "dbo");
        }
    }

    /// <inheritdoc/>
    public async Task UseHistory<T>()
    {
        HasHistory.Add(typeof(T), true);
    }

    /// <inheritdoc/>
    public async Task AddHistory<T>()
    {
        if (HasHistoryCheck<T>())
        {
            await CreateSharedHistoryFunction();
            await AddHistory<T>(HistorySchema<T>(), TableName<T>(), HistoryTableName<T>(), HistoryViewName<T>());
            if (HasTranslation[typeof(T)])
            {
                await AddHistory<T>(HistoryTranslationSchema<T>(), TranslationTableName<T>(), HistoryTranslationTableName<T>(), HistoryTranslationViewName<T>(), isTranslation: true);
            }
        }
    }

    private async Task AddHistory<T>(string schema, string tableName, string historyTableName, string historyViewName, bool isTranslation = false)
    {
        string functionName = $"{tableName}_copy_to_history()";

        string columnDefinitions = string.Join(',', Columns[typeof(T)].Keys);
        string columnOldDefinitions = string.Join(',', Columns[typeof(T)].Select(t => $"OLD.{t.Key}"));

        string key = $"TRIGGER {typeof(T).Name}_update_validfrom ON {tableName}";
        if (NeedMigration<T>(key))
        {
            string query = $"""
            CREATE OR REPLACE TRIGGER {typeof(T).Name}_update_validfrom BEFORE UPDATE ON {tableName}
            FOR EACH ROW EXECUTE FUNCTION dbo.update_validfrom();
            """;
            await ExecuteQuery(query);
            await LogMigration<T>(key, query);
        }

        string functionKey = $"FUNCTION {functionName}";
        if (NeedMigration<T>(functionKey))
        {
            string query = $"""
            CREATE OR REPLACE FUNCTION {functionName} RETURNS TRIGGER AS $$ BEGIN
            INSERT INTO {historyTableName} ({columnDefinitions}, {(isTranslation ? "language, " : "")}validfrom, validto) VALUES({columnOldDefinitions}, {(isTranslation ? "OLD.language, " : "")}OLD.validfrom, now());
            RETURN NEW;
            END; $$ LANGUAGE plpgsql;
            CREATE OR REPLACE TRIGGER {typeof(T).Name}_History AFTER UPDATE ON {tableName}
            FOR EACH ROW EXECUTE FUNCTION {functionName};
            """;
            await ExecuteQuery(query);
            await LogMigration<T>(functionKey, query);
        }

        string viewKey = $"VIEW {historyViewName}";
        if (NeedMigration<T>(viewKey))
        {
            string query = $"""
            CREATE OR REPLACE VIEW {historyViewName} AS
            SELECT {columnDefinitions}, {(isTranslation ? "language, " : "")} validfrom, validto
            FROM  {historyTableName}
            WHERE validfrom <= coalesce(current_setting('x.asof', true)::timestamptz, now())
            AND validto > coalesce(current_setting('x.asof', true)::timestamptz, now())
            UNION ALL
            SELECT  {columnDefinitions}, {(isTranslation ? "language, " : "")}validfrom, now() AS validto
            FROM {tableName}
            where validfrom <= coalesce(current_setting('x.asof', true)::timestamptz, now());
            """;
            await ExecuteQuery(query);
            await LogMigration<T>(viewKey, query);
        }
    }

    private string TableName<T>() => $"{defaultSchema}.{typeof(T).Name}";
    private string TranslationTableName<T>() => $"{translationSchema}.{typeof(T).Name}";
    private string TranslationViewName<T>() => $"{translationSchema}.{typeof(T).Name}";

    private string HistorySchema<T>() => $"{defaultSchema}_history";
    private string HistoryTableName<T>() => $"{defaultSchema}_history._{typeof(T).Name}";
    private string HistoryViewName<T>() => $"{defaultSchema}_history.{typeof(T).Name}";

    private string HistoryTranslationSchema<T>() => $"{translationSchema}_history";
    private string HistoryTranslationTableName<T>() => $"{translationSchema}_history._{typeof(T).Name}";
    private string HistoryTranslationViewName<T>() => $"{translationSchema}_history.{typeof(T).Name}";

    private async Task ExecuteQuery(string query)
    {
        try
        {
            await _connection.ExecuteAsync(query);
        }
        catch (Exception ex)
        {
            LogInfo(query);
            LogWarning(query);
            LogError(ex.Message);
            throw;
        }
    }

    private static bool IsValidColumnName(string str)
    {
        return Regex.IsMatch(str, @"^[a-zA-Z0-9]+$");
    }

    private PropertyInfo ExtractPropertyInfo<TLocal>(Expression<Func<TLocal, object>> expression)
    {
        MemberExpression memberExpression;

        if (expression.Body is MemberExpression)
        {
            // Hvis Body er direkte en MemberExpression, bruk den
            memberExpression = (MemberExpression)expression.Body;
        }
        else if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression)
        {
            // Hvis Body er en UnaryExpression (f.eks. ved en typekonvertering), bruk Operand
            memberExpression = (MemberExpression)unaryExpression.Operand;
        }
        else
        {
            throw new ArgumentException("Expression must refer to a property.");
        }

        return memberExpression.Member as PropertyInfo
            ?? throw new ArgumentException("Member is not a property.");
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
