using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using Altinn.AccessMgmt.DbAccess.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Npgsql;

namespace Altinn.AccessMgmt.DbAccess.Helpers;

/// <summary>
/// Responsible for building SQL queries based on the DbDefinition, RequestOptions, and filters.
/// </summary>
public class SqlQueryBuilder(DbDefinition definition)
{
    private readonly DbDefinition _definition = definition;

    #region Query

    /*Read*/

    /// <summary>
    /// Builds a SELECT viewQuery for basic types
    /// </summary>
    public string BuildBasicSelectQuery(RequestOptions options, IEnumerable<GenericFilter> filters)
    {
        var sb = new StringBuilder();

        if (options.AsOf.HasValue)
        {
            sb.AppendLine($"set local x.asof = '{options.AsOf.Value.ToUniversalTime()}';");
        }

        sb.AppendLine("SELECT ");
        sb.AppendLine(GenerateColumns(options));
        sb.AppendLine("FROM " + GenerateSource(options));
        sb.AppendLine(GenerateFilterStatement(_definition.BaseType.Name, filters));

        string query = sb.ToString();
        return AddPagingToQuery(query, options);
    }

    /// <summary>
    /// Builds a SELECT viewQuery for extended types
    /// </summary>
    public string BuildExtendedSelectQuery(RequestOptions options, IEnumerable<GenericFilter> filters)
    {
        var sb = new StringBuilder();

        // Apply session settings (e.g., as-of)
        if (options.AsOf.HasValue)
        {
            sb.AppendLine($"set local x.asof = '{options.AsOf.Value.ToUniversalTime()}';");
        }

        sb.AppendLine("SELECT ");
        sb.AppendLine(GenerateColumns(options));

        foreach (var relation in _definition.ForeignKeys)
        {
            sb.Append(',');
            sb.AppendLine(GenerateJoinPostgresColumns(relation, options));
        }

        sb.AppendLine("FROM " + GenerateSource(options));
        foreach (var j in _definition.ForeignKeys.Where(t => !t.IsList))
        {
            var joinStatement = GetJoinPostgresStatement(j, options);
            sb.AppendLine(joinStatement);
        }

        sb.AppendLine(GenerateFilterStatement(_definition.BaseType.Name, filters));

        string query = AddPagingToQuery(sb.ToString(), options);
        Console.WriteLine(query);

        return query;
    }

    /*Write*/

    /// <summary>
    /// Builds a INSERT viewQuery
    /// </summary>
    /// <param name="parameters">Parameters</param>
    /// <param name="forTranslation">Is this for a translation table</param>
    /// <returns></returns>
    public string BuildInsertQuery(List<NpgsqlParameter> parameters, bool forTranslation = false)
    {
        return $"INSERT INTO {GetPostgresDefinition(includeAlias: false, useTranslation: forTranslation)} ({InsertColumns(parameters)}) VALUES({InsertValues(parameters)})";
    }

    /// <summary>
    /// Builds a UPDATE viewQuery
    /// </summary>
    /// <param name="parameters">Parameters</param>
    /// <param name="forTranslation">Is this for a translation table</param>
    /// <returns></returns>
    public string BuildUpdateQuery(List<NpgsqlParameter> parameters, bool forTranslation = false)
    {
        return $"UPDATE {GetPostgresDefinition(includeAlias: false, useTranslation: forTranslation)} SET {UpdateSetStatement(parameters)} WHERE id = @_id{(forTranslation ? " AND language = @_language" : "")}";
    }

    /// <summary>
    /// Builds a UPSERT viewQuery
    /// </summary>
    /// <param name="parameters">Parameters</param>
    /// <returns></returns>
    public string BuildUpsertQuery(List<NpgsqlParameter> parameters)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"INSERT INTO {GetPostgresDefinition(includeAlias: false)} ({InsertColumns(parameters)}) VALUES({InsertValues(parameters)})");
        sb.AppendLine(" ON CONFLICT (id) DO ");
        sb.AppendLine($"UPDATE SET {UpdateSetStatement(parameters)}");

        return sb.ToString();
    }

    /// <summary>
    /// Builds a DELETE viewQuery
    /// </summary>
    /// <returns></returns>
    public string BuildDeleteQuery()
    {
        return $"DELETE FROM {GetPostgresDefinition(includeAlias: false)} WHERE id = @_id";
    }

    /*Helpers*/
    private string GetPostgresDefinition(bool includeAlias = true, bool useHistory = false, bool useTranslation = false, bool useHistoryView = false)
    {
        return GetPostgresDefinition(_definition, includeAlias, useHistory, useTranslation, useHistoryView);
    }

    private string GetPostgresDefinition(DbDefinition dbDefinition, bool includeAlias = true, bool useHistory = false, bool useTranslation = false, bool useHistoryView = false)
    {
        // If Definition.Plantform == "Mssql" => Qualify names [..]
        string res = "";
        if (useHistory)
        {
            string historyTablePrefix = useHistoryView ? "" : "_";
            if (useTranslation)
            {
                res = $"{dbDefinition.TranslationHistorySchema}.{historyTablePrefix}{dbDefinition.BaseType.Name}";
            }
            else
            {
                res = $"{dbDefinition.BaseHistorySchema}.{historyTablePrefix}{dbDefinition.BaseType.Name}";
            }
        }
        else
        {
            if (useTranslation)
            {
                res = $"{dbDefinition.TranslationSchema}.{dbDefinition.BaseType.Name}";
            }
            else
            {
                res = $"{dbDefinition.BaseSchema}.{dbDefinition.BaseType.Name}";
            }
        }

        if (includeAlias)
        {
            res += $" AS {dbDefinition.BaseType.Name}";
        }

        return res;
    }

    /*Basic*/
    private string GenerateColumns(RequestOptions options)
    {
        bool useTranslation = !string.IsNullOrEmpty(options.Language) && _definition.HasTranslation;
        var columns = new List<string>();

        foreach (var p in _definition.Columns.Select(t => t.Property))
        {
            if (useTranslation && p.PropertyType == typeof(string))
            {
                columns.Add($"coalesce(T_{_definition.BaseType.Name}.{p.Name}, {_definition.BaseType.Name}.{p.Name}) AS {p.Name}");
            }
            else
            {
                columns.Add($"{_definition.BaseType.Name}.{p.Name} AS {p.Name}");
            }
        }

        // Add paging row number if needed
        if (options.UsePaging)
        {
            string orderBy = !string.IsNullOrEmpty(options.OrderBy) && _definition.Columns.Exists(t => t.Name.Equals(options.OrderBy, StringComparison.CurrentCultureIgnoreCase)) ? options.OrderBy : "Id";
            columns.Add($"ROW_NUMBER() OVER (ORDER BY {_definition.BaseType.Name}.{orderBy}) AS _rownum");
        }

        return string.Join(',', columns);
    }

    private string GenerateSource(RequestOptions options)
    {
        bool useTranslation = !string.IsNullOrEmpty(options.Language) && _definition.HasTranslation;
        bool useHistory = options.AsOf.HasValue;

        if (useTranslation)
        {
            // Example for translation JOIN
            return $@"""
                {GetPostgresDefinition(includeAlias: true, useHistory: useHistory)}
                LEFT JOIN LATERAL (SELECT * FROM {GetPostgresDefinition(includeAlias: false, useTranslation: true, useHistory: useHistory)} AS T 
                WHERE T.Id = {_definition.BaseType.Name}.Id AND T.Language = @Language ) AS T_{_definition.BaseType.Name} ON 1=1
                """;
        }
        else
        {
            return GetPostgresDefinition(useHistory: useHistory);
        }
    }

    private string GenerateFilterStatement(string tableAlias, IEnumerable<GenericFilter>? filters)
    {
        if (filters == null || !filters.Any())
        {
            return string.Empty;
        }

        var conditions = new List<string>();

        foreach (var filter in filters)
        {
            string condition = filter.Comparer switch
            {
                FilterComparer.Equals => $"{tableAlias}.{filter.PropertyName} = @{filter.PropertyName}",
                FilterComparer.NotEqual => $"{tableAlias}.{filter.PropertyName} <> @{filter.PropertyName}",
                FilterComparer.GreaterThan => $"{tableAlias}.{filter.PropertyName} > @{filter.PropertyName}",
                FilterComparer.GreaterThanOrEqual => $"{tableAlias}.{filter.PropertyName} >= @{filter.PropertyName}",
                FilterComparer.LessThan => $"{tableAlias}.{filter.PropertyName} < @{filter.PropertyName}",
                FilterComparer.LessThanOrEqual => $"{tableAlias}.{filter.PropertyName} <= @{filter.PropertyName}",
                FilterComparer.StartsWith => $"{tableAlias}.{filter.PropertyName} ILIKE @{filter.PropertyName}",
                FilterComparer.EndsWith => $"{tableAlias}.{filter.PropertyName} ILIKE @{filter.PropertyName}",
                FilterComparer.Contains => $"{tableAlias}.{filter.PropertyName} ILIKE @{filter.PropertyName}",
                _ => throw new NotSupportedException($"Comparer '{filter.Comparer}' is not supported.")
            };

            conditions.Add(condition);
        }

        return conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;
    }

    private string AddPagingToQuery(string query, RequestOptions options)
    {
        if (!options.UsePaging)
        {
            return query;
        }

        var sb = new StringBuilder();
        sb.AppendLine("WITH pagedresult AS (");
        sb.AppendLine(query);
        sb.AppendLine(")");
        sb.AppendLine("SELECT *");
        sb.AppendLine("FROM pagedresult, (SELECT MAX(pagedresult._rownum) AS totalitems FROM pagedresult) AS pageinfo");
        sb.AppendLine($"ORDER BY _rownum OFFSET {options.PageSize * (options.PageNumber - 1)} ROWS FETCH NEXT {options.PageSize} ROWS ONLY");

        return sb.ToString();
    }

    /*Extended*/
    private string GetJoinPostgresStatement(ForeignKeyDefinition join, RequestOptions options)
    {
        var joinDef = DefinitionStore.TryGetDefinition(join.Ref) ?? throw new InvalidOperationException();
        bool useHistory = options.AsOf.HasValue;

        var sb = new StringBuilder();

        sb.Append($"{(join.IsOptional ? "LEFT OUTER" : "INNER")} JOIN {GetPostgresDefinition(joinDef, includeAlias: false, useHistory: useHistory)} AS _{join.ExtendedProperty} ON {join.Base.Name}.{join.BaseProperty} = _{join.ExtendedProperty}.{join.RefProperty} {GetJoinPostgresFilterString(join)}");

        bool useTranslation = !string.IsNullOrEmpty(options.Language);
        if (useTranslation && joinDef.HasTranslation)
        {
            sb.AppendLine();
            sb.Append($"LEFT JOIN LATERAL (SELECT * FROM {GetPostgresDefinition(joinDef, useTranslation: true, includeAlias: false, useHistory: useHistory)} AS T ");
            sb.Append($"WHERE T.Id = _{join.ExtendedProperty}.Id AND t.Language = @Language) AS T_{join.ExtendedProperty} ON 1=1");
        }

        return sb.ToString();
    }

    private string GenerateJoinPostgresColumns(ForeignKeyDefinition join, RequestOptions options)
    {
        var joinDef = DefinitionStore.TryGetDefinition(join.Ref);
        if (joinDef == null)
        {
            return string.Empty;
        }

        if (!join.IsList)
        {
            bool useTranslation = !string.IsNullOrEmpty(options.Language);
            var columns = new List<string>();
            foreach (var p in joinDef.Columns.Select(t => t.Property))
            {
                if (joinDef.HasTranslation && useTranslation && p.PropertyType == typeof(string))
                {
                    columns.Add($"coalesce(T_{join.ExtendedProperty}.{p.Name}, _{join.ExtendedProperty}.{p.Name}) AS {join.ExtendedProperty}_{p.Name}");
                }
                else
                {
                    columns.Add($"_{join.ExtendedProperty}.{p.Name} AS {join.ExtendedProperty}_{p.Name}");
                }
            }

            return string.Join(',', columns);
        }
        else
        {
            return $"COALESCE((SELECT JSON_AGG(ROW_TO_JSON({join.ExtendedProperty})) FROM {GetPostgresDefinition(joinDef, includeAlias: false)} AS {join.ExtendedProperty} WHERE {join.ExtendedProperty}.{join.RefProperty} = {join.Base.Name}.{join.BaseProperty}), '[]') AS {join.ExtendedProperty}";
        }
    }

    private string GetJoinPostgresFilterString(ForeignKeyDefinition join)
    {
        if (join.Filters == null || join.Filters.Count == 0)
        {
            return "";
        }

        string result = string.Empty;

        foreach (var filter in join.Filters)
        {
            result += $" AND {join.Base.Name}.{filter.PropertyName} = _{join.ExtendedProperty}.{filter.Value}";
        }

        return result;
    }

    /*Write*/
    private string UpdateSetStatement(IEnumerable<NpgsqlParameter> parameters)
    {
        return UpdateSetStatement(parameters.Select(t => t.ParameterName).ToList());
    }

    private string UpdateSetStatement(List<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"{t} = @{t}").ToList());
    }

    private string InsertColumns(List<NpgsqlParameter> values)
    {
        return InsertColumns(values.Select(t => t.ParameterName));
    }

    private string InsertColumns(IEnumerable<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"{t}").ToList());
    }

    private string InsertValues(List<NpgsqlParameter> values)
    {
        return InsertValues(values.Select(t => t.ParameterName));
    }

    private string InsertValues(IEnumerable<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"@{t}").ToList());
    }

    #endregion

    #region Schema

    /// <summary>
    /// Generates mirgration scripts for the definition
    /// </summary>
    /// <returns></returns>
    public MigrationScriptCollection GetMigrationScripts()
    {
        var scriptCollection = new MigrationScriptCollection(_definition.BaseType);

        scriptCollection.AddScripts(CreateTable());

        foreach (var column in _definition.Columns)
        {
            if (_definition.PrimaryKey.Properties.ContainsKey(column.Name))
            {
                continue;
            }

            scriptCollection.AddScripts(CreateColumn(column));
        }

        foreach (var fk in _definition.ForeignKeys)
        {
            scriptCollection.AddScripts(CreateForeignKeyConstraint(fk));
            scriptCollection.AddDependency(fk.Ref);
        }

        foreach (var uc in _definition.UniqueConstraints)
        {
            scriptCollection.AddScripts(CreateUniqueConstraint(uc));
        }

        if (_definition.HasHistory)
        {
            scriptCollection.AddScripts(CreateSharedHistoryFunction());
            scriptCollection.AddScripts(CreateHistoryTriggersAndView(false));
            
            if (_definition.HasTranslation)
            {
                scriptCollection.AddScripts(CreateHistoryTriggersAndView(true));
            }
        }

        return scriptCollection;
    }

    private OrderedDictionary<string, string> CreateTable()
    {
        var scripts = new OrderedDictionary<string, string>();

        _definition.PrimaryKey.Properties ??= new Dictionary<string, Type>();
        if (_definition.PrimaryKey.Properties.Count == 0)
        {
            _definition.PrimaryKey.Properties.Add("Id", typeof(Guid));
        }

        var properties = _definition.BaseType.GetProperties().ToList();
        foreach (var property in _definition.PrimaryKey.Properties)
        {
            if (!properties.Exists(t => t.Name == property.Key))
            {
                throw new Exception($"PK: {_definition.BaseType.Name} does not contain the property '{property.Key}'");
            }
        }

        string primaryKeyDefinition = string.Join(',', _definition.PrimaryKey.Properties.Select(t => $"{t.Key} {Helpers.GetPostgresType(t.Value)} NOT NULL "));

        string basicKey = $"CREATE TABLE {GetPostgresDefinition(includeAlias: false)}";
        var basicTable = new StringBuilder();
        basicTable.AppendLine($"CREATE TABLE {GetPostgresDefinition(includeAlias: false)} (");
        basicTable.AppendLine(primaryKeyDefinition);
        if (_definition.HasHistory)
        {
            basicTable.AppendLine(", validfrom timestamptz default now()");
        }

        basicTable.AppendLine($", CONSTRAINT PK_{_definition.BaseType.Name} PRIMARY KEY ({string.Join(',', _definition.PrimaryKey.Properties.Select(t => $"{t.Key}"))})");
        basicTable.AppendLine(");");

        scripts.Add(basicKey, basicTable.ToString());

        if (_definition.HasTranslation)
        {
            string translationKey = $"CREATE TABLE {GetPostgresDefinition(includeAlias: false, useTranslation: true)}";
            var translationTable = new StringBuilder();
            translationTable.AppendLine($"CREATE TABLE {GetPostgresDefinition(includeAlias: false, useTranslation: true)} (");
            translationTable.AppendLine(primaryKeyDefinition);
            if (_definition.HasHistory)
            {
                translationTable.AppendLine(", validfrom timestamptz default now()");
            }

            translationTable.AppendLine($", Language text NOT NULL");
            translationTable.AppendLine($", CONSTRAINT PK_{_definition.BaseType.Name} PRIMARY KEY ({string.Join(',', _definition.PrimaryKey.Properties.Select(t => $"{t.Key}"))}, Language)");
            translationTable.AppendLine(");");

            scripts.Add(translationKey, translationTable.ToString());
        }

        if (_definition.HasHistory)
        {
            string historyKey = $"CREATE TABLE {GetPostgresDefinition(includeAlias: false, useHistory: true)}";
            var historyTable = new StringBuilder();
            historyTable.AppendLine($"CREATE TABLE {GetPostgresDefinition(includeAlias: false, useHistory: true)}(");
            historyTable.AppendLine(primaryKeyDefinition);
            historyTable.AppendLine(", validfrom timestamptz default now()");
            historyTable.AppendLine(", validto timestamptz default now()");
            historyTable.AppendLine(");");

            scripts.Add(historyKey, historyTable.ToString());

            if (_definition.HasTranslation)
            {
                string historyTranslationKey = $"CREATE TABLE {GetPostgresDefinition(includeAlias: false, useTranslation: true, useHistory: true)}";
                var historyTranslationTable = new StringBuilder();
                historyTranslationTable.AppendLine($"CREATE TABLE {GetPostgresDefinition(includeAlias: false, useTranslation: true, useHistory: true)}(");
                historyTranslationTable.AppendLine(primaryKeyDefinition);
                historyTranslationTable.AppendLine(", validfrom timestamptz default now()");
                historyTranslationTable.AppendLine(", validto timestamptz default now()");
                historyTranslationTable.AppendLine(", Language text NOT NULL");
                historyTranslationTable.AppendLine(");");

                scripts.Add(historyTranslationKey, historyTranslationTable.ToString());
            }
        }

        return scripts;
    }

    private OrderedDictionary<string, string> CreateColumn(ColumnDefinition column)
    {
        var scripts = new OrderedDictionary<string, string>();

        var basic = CreateColumn(GetPostgresDefinition(includeAlias: false), column.Name, Helpers.GetPostgresType(column.Property).ToString(), column.IsNullable, column.DefaultValue);
        scripts.Add(basic.Key, basic.Query);

        if (_definition.HasTranslation)
        {
            var translation = CreateColumn(GetPostgresDefinition(includeAlias: false, useTranslation: true), column.Name, Helpers.GetPostgresType(column.Property).ToString(), true, column.DefaultValue);
            scripts.Add(translation.Key, translation.Query);
        }

        if (_definition.HasHistory)
        {
            var history = CreateColumn(GetPostgresDefinition(includeAlias: false, useHistory: true), column.Name, Helpers.GetPostgresType(column.Property).ToString(), true, column.DefaultValue);
            scripts.Add(history.Key, history.Query);

            if (_definition.HasTranslation)
            {
                var translationHistory = CreateColumn(GetPostgresDefinition(includeAlias: false, useTranslation: true, useHistory: true), column.Name, Helpers.GetPostgresType(column.Property).ToString(), true, column.DefaultValue);
                scripts.Add(translationHistory.Key, translationHistory.Query);
            }
        }

        return scripts;
    }
    
    private (string Key, string Query) CreateColumn(string tableName, string columnName, string columnDataType, bool isNullable, string? defaultValue = null)
    {
        var key = $"ADD COLUMN {tableName}.{columnName}";
        var query = $"ALTER TABLE {tableName} ADD {columnName} {columnDataType} {(isNullable ? "NULL" : "NOT NULL")}{(string.IsNullOrEmpty(defaultValue) ? "" : $" DEFAULT {defaultValue}")};";

        return (key, query);
    }

    private (string Key, string Query) CreateUniqueConstraint(ConstraintDefinition constraint)
    {
        string name = string.IsNullOrEmpty(constraint.Name) ? _definition.BaseType.Name : constraint.Name;

        foreach (var property in constraint.Columns)
        {
            if (_definition.BaseType.GetProperties().Count(t => t.Name.Equals(property)) == 0)
            {
                throw new Exception($"Property {property} not found on {_definition.BaseType.Name}");
            }
        }

        string key = $"ADD CONSTRAINT {GetPostgresDefinition(includeAlias: false)}.UC_{name}";
        string query = $"ALTER TABLE {GetPostgresDefinition(includeAlias: false)} ADD CONSTRAINT UC_{name} UNIQUE ({string.Join(',', constraint.Columns)});";

        return (key, query);
    }

    private (string Key, string Query) CreateForeignKeyConstraint(ForeignKeyDefinition foreignKey)
    {
        if (!_definition.BaseType.GetProperties().ToList().Exists(t => t.Name.Equals(foreignKey.BaseProperty, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception($"{_definition.BaseType.Name} does not contain the property '{foreignKey.BaseProperty}'");
        }

        if (!foreignKey.Ref.GetProperties().ToList().Exists(t => t.Name.Equals(foreignKey.RefProperty, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception($"{foreignKey.Ref.Name} does not contain the property '{foreignKey.RefProperty}'");
        }

        var targetDef = DefinitionStore.TryGetDefinition(foreignKey.Ref);
        if (targetDef == null)
        {
            throw new Exception("Type not found");
        }

        string name = string.IsNullOrEmpty(foreignKey.Name) ? $"{_definition.BaseType.Name}_{foreignKey.BaseProperty}" : foreignKey.Name;

        var key = $"ADD CONSTRAINT {GetPostgresDefinition(includeAlias: false)}.FK_{name}";
        var query = $"ALTER TABLE {GetPostgresDefinition(includeAlias: false)} ADD CONSTRAINT FK_{name} FOREIGN KEY ({foreignKey.BaseProperty}) REFERENCES {GetPostgresDefinition(targetDef, includeAlias: false)} ({foreignKey.RefProperty}) {(foreignKey.UseCascadeDelete ? "ON DELETE CASCADE" : "ON DELETE SET NULL")};";

        return (key, query);
    }

    private OrderedDictionary<string, string> CreateSharedHistoryFunction()
    {
        var res = new OrderedDictionary<string, string>();
        
        string key = "FUNCTION update_validfrom";
        string query = """
        CREATE OR REPLACE FUNCTION dbo.update_validfrom() RETURNS trigger AS
        $$
        BEGIN
            NEW.validfrom := NOW();
            RETURN NEW;
        END;
        $$ LANGUAGE plpgsql;
        """;

        res.Add(key, query);
        return res;
    }

    private OrderedDictionary<string,string> CreateHistoryTriggersAndView(bool forTranslationTable)
    {
        var scripts = new OrderedDictionary<string, string>();

        /*
        var shared = CreateSharedHistoryFunction();
        scripts.Add(shared.key, shared.query);
        */

        string tableName = GetPostgresDefinition(includeAlias: false, useTranslation: forTranslationTable);

        string columnDefinitions = string.Join(',', _definition.Columns.Select(t => t.Name));
        string columnOldDefinitions = string.Join(',', _definition.Columns.Select(t => $"OLD.{t.Name}"));

        string keyTriggerUpdateValidFrom = $"TRIGGER {_definition.BaseType.Name}_update_validfrom ON {tableName}";
        string queryTriggerUpdateValidFrom = $"""
        CREATE OR REPLACE TRIGGER {_definition.BaseType.Name}_update_validfrom BEFORE UPDATE ON {tableName}
        FOR EACH ROW EXECUTE FUNCTION dbo.update_validfrom();
        """;

        scripts.Add(keyTriggerUpdateValidFrom, queryTriggerUpdateValidFrom);

        string copyToHistoryFuncName = $"{tableName}_copy_to_history()";
        string keyCopyToHistory = $"FUNCTION {copyToHistoryFuncName}";
        string queryCopyToHistory = $"""
        CREATE OR REPLACE FUNCTION {copyToHistoryFuncName} RETURNS TRIGGER AS $$ BEGIN
        INSERT INTO {GetPostgresDefinition(includeAlias: false, useHistory: true, useTranslation: forTranslationTable)} ({columnDefinitions}, {(forTranslationTable ? "language, " : "")}validfrom, validto) VALUES({columnOldDefinitions}, {(forTranslationTable ? "OLD.language, " : "")}OLD.validfrom, now());
        RETURN NEW;
        END; $$ LANGUAGE plpgsql;
        CREATE OR REPLACE TRIGGER {_definition.BaseType.Name}_History AFTER UPDATE ON {tableName}
        FOR EACH ROW EXECUTE FUNCTION {copyToHistoryFuncName};
        """;
        scripts.Add(keyCopyToHistory, queryCopyToHistory);

        string viewKey = $"VIEW {GetPostgresDefinition(includeAlias: false, useHistory: true, useHistoryView: true, useTranslation: forTranslationTable)}";
        string viewQuery = $"""
        CREATE OR REPLACE VIEW {GetPostgresDefinition(includeAlias: false, useHistory: true, useHistoryView: true, useTranslation: forTranslationTable)} AS
        SELECT {columnDefinitions}, {(forTranslationTable ? "language, " : "")} validfrom, validto
        FROM  {GetPostgresDefinition(useHistory: true, useTranslation: forTranslationTable)}
        WHERE validfrom <= coalesce(current_setting('x.asof', true)::timestamptz, now())
        AND validto > coalesce(current_setting('x.asof', true)::timestamptz, now())
        UNION ALL
        SELECT  {columnDefinitions}, {(forTranslationTable ? "language, " : "")}validfrom, now() AS validto
        FROM {tableName}
        where validfrom <= coalesce(current_setting('x.asof', true)::timestamptz, now());
        """;
        scripts.Add(viewKey, viewQuery);

        return scripts;
    }
    #endregion

    #region Data?
    /*JSON Import*/
    #endregion
}
