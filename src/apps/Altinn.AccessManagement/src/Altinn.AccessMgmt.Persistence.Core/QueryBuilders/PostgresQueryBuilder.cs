using System.Data;
using System.Text;
using Altinn.AccessMgmt.Persistence.Core.Definitions;
using Altinn.AccessMgmt.Persistence.Core.Helpers;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using Yuniql.PostgreSql;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace Altinn.AccessMgmt.Persistence.Core.QueryBuilders;

/// <inheritdoc/>
public class PostgresQueryBuilder : IDbQueryBuilder
{
    private readonly IOptions<AccessMgmtPersistenceOptions> _config;
    private readonly DbDefinition _definition;
    private readonly DbDefinitionRegistry _definitionRegistry;

    /// <summary>
    /// QueryBuilder for Postgres
    /// </summary>
    /// <param name="options"><see cref="IOptions{TOptions}"/></param>
    /// <param name="type">Type</param>
    /// <param name="definitionRegistry">DbDefinitionRegistry</param>
    public PostgresQueryBuilder(IOptions<AccessMgmtPersistenceOptions> options, Type type, DbDefinitionRegistry definitionRegistry)
    {
        _config = options;
        _definition = definitionRegistry.TryGetDefinition(type) ?? throw new Exception("Missing definition");
        _definitionRegistry = definitionRegistry;
    }

    #region Query

    /// <inheritdoc/>
    public string BuildBasicSelectQuery(RequestOptions options, IEnumerable<GenericFilter> filters, DbCrossRelationDefinition crossDef = null)
    {
        var sb = new StringBuilder();

        if (options.AsOf.HasValue)
        {
            sb.AppendLine($"set local app.asof = '{options.AsOf.Value.ToUniversalTime()}';");
        }

        sb.AppendLine("SELECT ");
        sb.AppendLine(GenerateColumns(options));
        sb.AppendLine("FROM " + GenerateSource(options));

        if (crossDef != null)
        {
            sb.AppendLine(GenerateCrossReferenceJoin(crossDef, options));
        }

        sb.AppendLine(GenerateFilterStatement(_definition.ModelType.Name, filters));

        string query = sb.ToString();
        return AddPagingToQuery(query, options);
    }

    /// <inheritdoc/>
    public string BuildExtendedSelectQuery(RequestOptions options, IEnumerable<GenericFilter> filters, DbCrossRelationDefinition crossDef = null)
    {
        var sb = new StringBuilder();

        // Apply session settings (e.g., as-of)
        if (options.AsOf.HasValue)
        {
            sb.AppendLine($"set local app.asof = '{options.AsOf.Value.ToUniversalTime()}';");
        }

        sb.AppendLine("SELECT ");
        sb.AppendLine(GenerateColumns(options));

        foreach (var relation in _definition.Relations)
        {
            sb.Append(',');
            sb.AppendLine(GenerateJoinPostgresColumns(relation, options));
        }

        sb.AppendLine("FROM " + GenerateSource(options));

        if (crossDef != null)
        {
            sb.AppendLine(GenerateCrossReferenceJoin(crossDef, options));
        }

        foreach (var j in _definition.Relations.Where(t => !t.IsList))
        {
            var joinStatement = GetJoinPostgresStatement(j, options);
            sb.AppendLine(joinStatement);
        }

        sb.AppendLine(GenerateFilterStatement(_definition.ModelType.Name, filters));

        string query = AddPagingToQuery(sb.ToString(), options);
        Console.WriteLine(query);

        return query;
    }

    private static string GetAuditVariables(ChangeRequestOptions options)
    {
        /*
        private static readonly Guid DefaultPerformedBy = Guid.Parse("1201FF5A-172E-40C1-B0A4-1C121D41475F");
        
        if (options.ChangedBySystem == Guid.Empty)
        {
            options.ChangedBySystem = DefaultPerformedBy;
        }
        */

        return string.Format("SET LOCAL app.changed_by = '{0}'; SET LOCAL app.changed_by_system = '{1}'; SET LOCAL app.change_operation_id = '{2}';", options.ChangedBy, options.ChangedBySystem, options.ChangeOperationId);
    }

    /// <inheritdoc/>
    public string BuildInsertQuery(List<GenericParameter> parameters, ChangeRequestOptions options, bool forTranslation = false)
    {
        return $"{GetAuditVariables(options)} INSERT INTO {GetTableName(includeAlias: false, useTranslation: forTranslation)} ({InsertColumns(parameters)}) VALUES({InsertValues(parameters)})";
    }

    /// <inheritdoc/>
    public string BuildUpdateQuery(List<GenericParameter> parameters, ChangeRequestOptions options, bool forTranslation = false)
    {
        return $"{GetAuditVariables(options)} UPDATE {GetTableName(includeAlias: false, useTranslation: forTranslation)} SET {UpdateSetStatement(parameters)} WHERE id = @_id{(forTranslation ? " AND language = @_language" : string.Empty)}";
    }

    /// <inheritdoc/>
    public string BuildSingleNullUpdateQuery(GenericParameter parameter, ChangeRequestOptions options, bool forTranslation = false)
    {
        return $"{GetAuditVariables(options)} UPDATE {GetTableName(includeAlias: false, useTranslation: forTranslation)} SET {parameter.Key} = NULL WHERE id = @_id{(forTranslation ? " AND language = @_language" : string.Empty)}";
    }

    /// <inheritdoc/>
    public string BuildUpsertQuery(List<GenericParameter> parameters, ChangeRequestOptions options, bool forTranslation = false)
    {
        return BuildMergeQuery(parameters, [new GenericFilter("id", "id")], options, forTranslation);

        /*
        var sb = new StringBuilder();
        sb.AppendLine($"INSERT INTO {GetTableName(includeAlias: false, useTranslation: forTranslation)} ({InsertColumns(parameters)}) VALUES({InsertValues(parameters)})");
        sb.AppendLine(" ON CONFLICT (id) DO ");
        sb.AppendLine($"UPDATE SET {UpdateSetStatement(parameters)}");
        return sb.ToString();
        */
    }

    /// <inheritdoc/>
    public string BuildUpsertQuery(List<GenericParameter> parameters, List<GenericFilter> mergeFilter, ChangeRequestOptions options, bool forTranslation = false)
    {
        if (mergeFilter == null || !mergeFilter.Any())
        {
            mergeFilter.Add(new GenericFilter("id", "id"));
        }

        return BuildMergeQuery(parameters, mergeFilter, options, forTranslation);
    }

    private string BuildMergeQuery(List<GenericParameter> parameters, List<GenericFilter> mergeFilter, ChangeRequestOptions options, bool forTranslation = false)
    {
        if (mergeFilter == null || !mergeFilter.Any())
        {
            throw new ArgumentException("Missing mergefilter");
        }

        var mergeUpdateUnMatchStatement = string.Join(" OR ", parameters.Where(t => mergeFilter.Count(y => y.PropertyName.Equals(t.Key, StringComparison.OrdinalIgnoreCase)) == 0).Select(t => $"T.{t.Key} <> @{t.Key}"));
        var mergeUpdateStatement = string.Join(" , ", parameters.Where(t => mergeFilter.Count(y => y.PropertyName.Equals(t.Key, StringComparison.OrdinalIgnoreCase)) == 0).Select(t => $"{t.Key} = @{t.Key}"));


        var sb = new StringBuilder();
        sb.AppendLine($"{GetAuditVariables(options)}");
        sb.AppendLine("WITH N AS ( SELECT ");
        sb.AppendLine("@id as id");
        if (forTranslation)
        {
            sb.AppendLine(", @language as language");
        }

        sb.AppendLine(")");

        var mergeStatementFilter = string.Join(" AND ", mergeFilter.Select(t => $"T.{t.PropertyName} = N.{t.PropertyName}"));

        sb.AppendLine($"MERGE INTO {GetTableName(includeAlias: false, useTranslation: forTranslation)} AS T USING N ON {mergeStatementFilter}");
        if (forTranslation)
        {
            sb.AppendLine(" AND T.language = N.language");
        }

        sb.AppendLine($"WHEN MATCHED AND ({mergeUpdateUnMatchStatement}) THEN");
        sb.AppendLine($"UPDATE SET {mergeUpdateStatement}");
        sb.AppendLine("WHEN NOT MATCHED THEN");
        sb.AppendLine($"INSERT ({InsertColumns(parameters)}) VALUES({InsertValues(parameters)});");

        return sb.ToString();
    }

    /// <inheritdoc/>
    public string BuildDeleteQuery(IEnumerable<GenericFilter> filters, ChangeRequestOptions options)
    {
        var filterStatement = GenerateFilterStatement(_definition.ModelType.Name, filters);
        return $"{GetAuditVariables(options)} DELETE FROM {GetTableName(includeAlias: false)} {filterStatement}";
    }

    /// <inheritdoc />
    public string GetTableName(bool includeAlias = true, bool useHistory = false, bool useTranslation = false, bool useHistoryView = false, bool includeSchema = true)
    {
        return GetTableName(_definition, includeAlias, useHistory, useTranslation, useHistoryView, includeSchema);
    }

    private string GetTableName(DbDefinition dbDefinition, bool includeAlias = true, bool useHistory = false, bool useTranslation = false, bool useHistoryView = false, bool includeSchema = true)
    {
        var config = _config.Value;

        var schema = includeSchema
            ? (useHistory
                ? (useTranslation ? config.TranslationHistorySchema : config.BaseHistorySchema)
                : (useTranslation ? config.TranslationSchema : config.BaseSchema))
            : string.Empty;

        var prefix = (useHistory && !useHistoryView) ? "_" : string.Empty;
        var tableName = string.IsNullOrEmpty(schema)
            ? $"{prefix}{dbDefinition.ModelType.Name}"
            : $"{schema}.{prefix}{dbDefinition.ModelType.Name}";

        return includeAlias
            ? $"{tableName} AS {dbDefinition.ModelType.Name}"
            : tableName;
    }

    private string GetSchemaName(bool useHistory = false, bool useTranslation = false)
    {
        var config = _config.Value;
        return useHistory ? (useTranslation ? config.TranslationHistorySchema : config.BaseHistorySchema) : (useTranslation ? config.TranslationSchema : config.BaseSchema);
    }

    private string GetTableAlias()
    {
        return GetTableAlias(_definition);
    }

    private string GetTableAlias(DbDefinition dbDefinition)
    {
        return dbDefinition.ModelType.Name;
    }

    /*Basic*/
    private string GenerateColumns(RequestOptions options)
    {
        bool useTranslation = !string.IsNullOrEmpty(options.Language) && _definition.EnableTranslation;
        var columns = new List<string>();

        foreach (var p in _definition.Properties.Select(t => t.Property))
        {
            if (useTranslation && p.PropertyType == typeof(string))
            {
                columns.Add($"coalesce(T_{_definition.ModelType.Name}.{p.Name}, {_definition.ModelType.Name}.{p.Name}) AS {p.Name}");
            }
            else
            {
                columns.Add($"{_definition.ModelType.Name}.{p.Name} AS {p.Name}");
            }
        }

        // Add paging row number if needed
        if (options.UsePaging)
        {
            string orderBy = !string.IsNullOrEmpty(options.OrderBy) && _definition.Properties.Exists(t => t.Name.Equals(options.OrderBy, StringComparison.CurrentCultureIgnoreCase)) ? options.OrderBy : "Id";
            columns.Add($"ROW_NUMBER() OVER (ORDER BY {_definition.ModelType.Name}.{orderBy}) AS _rownum");
        }

        return string.Join(',', columns);
    }

    private string GenerateSource(RequestOptions options)
    {
        bool useTranslation = !string.IsNullOrEmpty(options.Language) && _definition.EnableTranslation;
        bool useHistory = options.AsOf.HasValue;

        if (useTranslation)
        {
            // Example for translation JOIN
            return $"""
                {GetTableName(includeAlias: true, useHistory: useHistory)}
                LEFT JOIN LATERAL (SELECT * FROM {GetTableName(includeAlias: false, useTranslation: true, useHistory: useHistory)} AS T 
                WHERE T.Id = {_definition.ModelType.Name}.Id AND T.Language = @Language ) AS T_{_definition.ModelType.Name} ON 1=1
                """;
        }
        else
        {
            return GetTableName(includeAlias: true, useHistory: useHistory);
        }
    }

    /// <summary>
    /// Generates an INNER JOIN SQL filterStatement for a cross-reference relationship.
    /// </summary>
    /// <param name="crossRef">The cross-reference definition.</param>
    /// <param name="options">Request options that may affect SQL generation.</param>
    /// <returns>A formatted SQL INNER JOIN filterStatement.</returns>
    private string GenerateCrossReferenceJoin(DbCrossRelationDefinition crossRef, RequestOptions options)
    {
        bool useHistory = options.AsOf.HasValue;

        var crossRefDef = _definitionRegistry.TryGetDefinition(crossRef.CrossType) ?? throw new InvalidOperationException();

        string mainTable = GetTableAlias();
        string crossTable = GetTableName(crossRefDef, includeAlias: false, useHistory: useHistory);
        string mainIdentityProperty = crossRef.GetReferenceProperty(_definition.ModelType);
        string joinColumn = crossRef.GetIdentityProperty(_definition.ModelType);
        string filterColumn = crossRef.GetReverseIdentityProperty(_definition.ModelType);

        return $@"
        INNER JOIN {crossTable} AS X 
        ON {mainTable}.{mainIdentityProperty} = X.{joinColumn}
        AND X.{filterColumn} = @X_Id";
    }

    private string GenerateFilterStatement(string tableAlias, IEnumerable<GenericFilter> filters)
    {
        if (filters == null || !filters.Any())
        {
            return string.Empty;
        }

        var conditions = new List<string>();

        var multiple = filters.CountBy(t => t.PropertyName).Where(t => t.Value > 1).Select(t => t.Key);

        foreach (var filter in filters.Where(t => !multiple.Contains(t.PropertyName)))
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

        foreach (var m in multiple)
        {
            var inList = new List<string>();
            var notInList = new List<string>();

            int a = 1;
            foreach (var filter in filters.Where(t => t.PropertyName == m))
            {
                if (filter.Comparer == FilterComparer.Equals)
                {
                    inList.Add($"@{m}_{a}");
                }
                else if (filter.Comparer == FilterComparer.NotEqual)
                {
                    notInList.Add($"@{m}_{a}");
                }
                else
                {
                    throw new Exception("Filter not supported");
                }

                a++;
            }

            if (inList.Any())
            {
                conditions.Add($"{tableAlias}.{m} IN ({string.Join(",", notInList)})");
            }

            if (notInList.Any())
            {
                conditions.Add($"{tableAlias}.{m} NOT IN ({string.Join(",", notInList)})");
            }
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
    private string GetJoinPostgresStatement(DbRelationDefinition join, RequestOptions options)
    {
        var joinDef = _definitionRegistry.TryGetDefinition(join.Ref) ?? throw new InvalidOperationException();
        bool useHistory = options.AsOf.HasValue;

        var sb = new StringBuilder();

        sb.Append($"{(join.IsOptional ? "LEFT OUTER" : "INNER")} JOIN {GetTableName(joinDef, includeAlias: false, useHistory: useHistory)} AS _{join.ExtendedProperty} ON {join.Base.Name}.{join.BaseProperty} = _{join.ExtendedProperty}.{join.RefProperty} {GetJoinPostgresFilterString(join)}");

        bool useTranslation = !string.IsNullOrEmpty(options.Language);
        if (useTranslation && joinDef.EnableTranslation)
        {
            sb.AppendLine();
            sb.Append($"LEFT JOIN LATERAL (SELECT * FROM {GetTableName(joinDef, useTranslation: true, includeAlias: false, useHistory: useHistory)} AS T ");
            sb.Append($"WHERE T.Id = _{join.ExtendedProperty}.Id AND t.Language = @Language) AS T_{join.ExtendedProperty} ON 1=1");
        }

        return sb.ToString();
    }

    private string GenerateJoinPostgresColumns(DbRelationDefinition join, RequestOptions options)
    {
        var joinDef = _definitionRegistry.TryGetDefinition(join.Ref);
        if (joinDef == null)
        {
            return string.Empty;
        }

        if (!join.IsList)
        {
            bool useTranslation = !string.IsNullOrEmpty(options.Language);
            var columns = new List<string>();
            foreach (var p in joinDef.Properties.Select(t => t.Property))
            {
                if (joinDef.EnableTranslation && useTranslation && p.PropertyType == typeof(string))
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
            return $"COALESCE((SELECT JSON_AGG(ROW_TO_JSON({join.ExtendedProperty})) FROM {GetTableName(joinDef, includeAlias: false)} AS {join.ExtendedProperty} WHERE {join.ExtendedProperty}.{join.RefProperty} = {join.Base.Name}.{join.BaseProperty}), '[]') AS {join.ExtendedProperty}";
        }
    }

    private string GetJoinPostgresFilterString(DbRelationDefinition join)
    {
        if (join.Filters == null || join.Filters.Count == 0)
        {
            return string.Empty;
        }

        string result = string.Empty;

        foreach (var filter in join.Filters)
        {
            result += $" AND {join.Base.Name}.{filter.PropertyName} = _{join.ExtendedProperty}.{filter.Value}";
        }

        return result;
    }

    /*Write*/
    private string UpdateSetStatement(IEnumerable<GenericParameter> parameters)
    {
        return UpdateSetStatement(parameters.Select(t => t.Key).ToList());
    }

    private string UpdateSetStatement(List<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"{t} = @{t}").ToList());
    }

    private string InsertColumns(List<GenericParameter> values)
    {
        return InsertColumns(values.Select(t => t.Key));
    }

    private string InsertColumns(IEnumerable<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"{t}").ToList());
    }

    private string InsertValues(List<GenericParameter> values)
    {
        return InsertValues(values.Select(t => t.Key));
    }

    private string InsertValues(IEnumerable<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"@{t}").ToList());
    }

    private string MergeUpdateMatchStatement(List<GenericParameter> values)
    {
        return MergeUpdateMatchStatement(values.Select(t => t.Key));
    }

    private string MergeUpdateMatchStatement(IEnumerable<string> values)
    {
        return string.Join(" OR ", values.OrderBy(t => t).Select(t => $"T.{t} <> @{t}").ToList());
    }
    #endregion

    #region Schema

    /// <inheritdoc/>
    public DbMigrationScriptCollection GetMigrationScripts()
    {
        var scriptCollection = new DbMigrationScriptCollection(_definition.ModelType);
        scriptCollection.Version = _definition.Version;

        //// Create view
        if (_definition.IsView)
        {
            scriptCollection.AddScripts(CreateView());
            foreach (var dep in _definition.ViewDependencies)
            {
                scriptCollection.AddDependency(dep);
            }

            return scriptCollection;
        }

        //// Create tables
        scriptCollection.AddScripts(CreateTables());

        //// Create columns
        var pk = _definition.Constraints.FirstOrDefault(t => t.IsPrimaryKey);
        foreach (var column in _definition.Properties)
        {
            if (pk != null && pk.Properties.ContainsKey(column.Name))
            {
                continue;
            }

            scriptCollection.AddScripts(CreateColumn(column));
        }

        //// Create Foreign keys
        foreach (var fk in _definition.Relations)
        {
            scriptCollection.AddScripts(CreateForeignKeyConstraint(fk));
            scriptCollection.AddDependency(fk.Ref);
        }

        //// Create unique constraints
        foreach (var uc in _definition.Constraints)
        {
            if (uc.IsPrimaryKey)
            {
                continue;
            }

            scriptCollection.AddScripts(CreateUniqueConstraint(uc));
        }

        //// Create history views
        if (_definition.EnableAudit)
        {
            scriptCollection.AddScripts($"VIEW {GetTableName(includeAlias: false, useHistory: true, useHistoryView: true, useTranslation: false)}", CreateHistoryView(isTranslation: false));

            if (_definition.EnableTranslation)
            {
                scriptCollection.AddScripts($"VIEW {GetTableName(includeAlias: false, useHistory: true, useHistoryView: true, useTranslation: true)}", CreateHistoryView(isTranslation: true));
            }
        }

        //// Common meta function
        scriptCollection.AddScripts(CreateAuditMetadataFunction()); // Move to Pre?

        //// Create functions and triggers
        scriptCollection.AddScripts(CreateMetaDataTrigger(isTranslation: false));
        scriptCollection.AddScripts(CreateAuditFunction(isTranslation: false));
        scriptCollection.AddScripts(CreateAuditTrigger(isTranslation: false));

        if (_definition.EnableTranslation)
        {
            scriptCollection.AddScripts(CreateMetaDataTrigger(isTranslation: true));
            scriptCollection.AddScripts(CreateAuditFunction(isTranslation: true));
            scriptCollection.AddScripts(CreateAuditTrigger(isTranslation: true));
        }

        return scriptCollection;
    }

    private OrderedDictionary<string, string> CreateView()
    {
        var scripts = new OrderedDictionary<string, string>();

        var query = $"""
        CREATE OR REPLACE VIEW {GetTableName(includeAlias: false)} AS
        {_definition.ViewQuery}
        """;

        scripts.Add($"CREATE VIEW {GetTableName(includeAlias: false)}", query);

        return scripts;
    }

    private OrderedDictionary<string, string> CreateTables()
    {
        var scripts = new OrderedDictionary<string, string>();

        var pk = _definition.Constraints.FirstOrDefault(t => t.IsPrimaryKey);
        if (pk == null)
        {
            throw new Exception($"PK: {_definition.ModelType.Name} does not contain a primary key");
        }

        var properties = _definition.ModelType.GetProperties().ToList();
        foreach (var property in pk.Properties)
        {
            if (!properties.Exists(t => t.Name == property.Key))
            {
                throw new Exception($"PK: {_definition.ModelType.Name} does not contain the property '{property.Key}'");
            }
        }

        string primaryKeyDefinition = string.Join(',', pk.Properties.Select(t => $"{t.Key} {DbHelperMethods.GetPostgresType(t.Value)} NOT NULL "));

        var dboName = GetTableName(includeAlias: false, useTranslation: false, useHistory: false);
        var translationName = GetTableName(includeAlias: false, useTranslation: true, useHistory: false);
        var dboHistoryName = GetTableName(includeAlias: false, useTranslation: false, useHistory: true);
        var translationHistoryName = GetTableName(includeAlias: false, useTranslation: true, useHistory: true);

        var dboScript = CreateTableScript(dboName, primaryKeyDefinition, _definition.EnableAudit, isHistory: false, isTranslation: false);
        var translationScript = CreateTableScript(translationName, primaryKeyDefinition, _definition.EnableAudit, isHistory: false, isTranslation: true);
        var basicHistoryScript = CreateTableScript(dboHistoryName, primaryKeyDefinition, _definition.EnableAudit, isHistory: true, isTranslation: false);
        var translationHistoryScript = CreateTableScript(translationHistoryName, primaryKeyDefinition, _definition.EnableAudit, isHistory: true, isTranslation: true);

        scripts.Add($"CREATE TABLE {dboName}", dboScript);
        
        if (_definition.EnableTranslation)
        {
            scripts.Add($"CREATE TABLE {translationName}", translationScript);
        }

        if (_definition.EnableAudit)
        {
            scripts.Add($"CREATE TABLE {dboHistoryName}", basicHistoryScript);
            if (_definition.EnableTranslation)
            {
                scripts.Add($"CREATE TABLE {translationHistoryName}", translationHistoryScript);
            }
        }

        return scripts;
    }

    private string CreateTableScript(string name, string primaryKeyDefinition, bool useAudit, bool isHistory, bool isTranslation)
    {
        var script = new StringBuilder();
        script.AppendLine($"CREATE TABLE IF NOT EXISTS {name}(");
        script.AppendLine(primaryKeyDefinition);

        if (isTranslation)
        {
            script.AppendLine(", language text not null");
        }

        if (useAudit)
        {
            if (!isHistory)
            {
                script.AppendLine(", audit_validfrom timestamptz not null default now()");
            }
            else
            {
                script.AppendLine(", audit_validfrom timestamptz not null");
                script.AppendLine(", audit_validto timestamptz not null");
            }

            script.AppendLine(", audit_changedby uuid not null");
            script.AppendLine(", audit_changedbysystem uuid not null");
            script.AppendLine(", audit_changeoperation uuid not null");

            if (isHistory)
            {
                script.AppendLine(", audit_deletedby uuid not null"); // History only
                script.AppendLine(", audit_deletedbysystem uuid not null"); // History only
                script.AppendLine(", audit_deletedoperation uuid not null"); // History only
            }
        }

        if (!isHistory)
        {
            if (isTranslation)
            {
                script.AppendLine($", CONSTRAINT PK_{_definition.ModelType.Name} PRIMARY KEY ({string.Join(',', _definition.Constraints.First(t => t.IsPrimaryKey).Properties.Select(t => $"{t.Key}"))}, language)");
            }
            else
            {
                script.AppendLine($", CONSTRAINT PK_{_definition.ModelType.Name} PRIMARY KEY ({string.Join(',', _definition.Constraints.First(t => t.IsPrimaryKey).Properties.Select(t => $"{t.Key}"))})");
            }
        }


        script.AppendLine(");");

        return script.ToString();
    }

    private OrderedDictionary<string, string> CreateColumn(DbPropertyDefinition column)
    {
        var scripts = new OrderedDictionary<string, string>();

        var basic = CreateColumn(GetTableName(includeAlias: false), column.Name, DbHelperMethods.GetPostgresType(column.Property).ToString(), column.IsNullable, column.DefaultValue);
        scripts.Add(basic.Key, basic.Query);

        if (_definition.EnableTranslation)
        {
            var translation = CreateColumn(GetTableName(includeAlias: false, useTranslation: true), column.Name, DbHelperMethods.GetPostgresType(column.Property).ToString(), true, column.DefaultValue);
            scripts.Add(translation.Key, translation.Query);
        }

        if (_definition.EnableAudit)
        {
            var history = CreateColumn(GetTableName(includeAlias: false, useHistory: true), column.Name, DbHelperMethods.GetPostgresType(column.Property).ToString(), true, column.DefaultValue);
            scripts.Add(history.Key, history.Query);

            if (_definition.EnableTranslation)
            {
                var translationHistory = CreateColumn(GetTableName(includeAlias: false, useTranslation: true, useHistory: true), column.Name, DbHelperMethods.GetPostgresType(column.Property).ToString(), true, column.DefaultValue);
                scripts.Add(translationHistory.Key, translationHistory.Query);
            }
        }

        return scripts;
    }

    private (string Key, string Query) CreateColumn(string tableName, string columnName, string columnDataType, bool isNullable, string defaultValue = null)
    {
        var key = $"ADD COLUMN {tableName}.{columnName}";
        var query = $"ALTER TABLE {tableName} ADD IF NOT EXISTS {columnName} {columnDataType} {(isNullable ? "NULL" : "NOT NULL")}{(string.IsNullOrEmpty(defaultValue) ? string.Empty : $" DEFAULT {defaultValue}")};";

        return (key, query);
    }

    private OrderedDictionary<string, string> CreateUniqueConstraint(DbConstraintDefinition constraint)
    {
        var res = new OrderedDictionary<string, string>();

        string name = string.IsNullOrEmpty(constraint.Name) ? _definition.ModelType.Name : constraint.Name;

        var props = _definition.ModelType.GetProperties();

        foreach (var property in constraint.Properties)
        {
            if (_definition.ModelType.GetProperties().Count(t => t.Name.Equals(property.Key)) == 0)
            {
                throw new Exception($"Property {property.Key} not found on {_definition.ModelType.Name}");
            }
        }

        string key = $"ADD CONSTRAINT {GetTableName(includeAlias: false)}.{name}";
        string query = $"ALTER TABLE {GetTableName(includeAlias: false)} DROP CONSTRAINT IF EXISTS {name}; ALTER TABLE {GetTableName(includeAlias: false)} ADD CONSTRAINT {name} UNIQUE ({string.Join(',', constraint.Properties.Keys)});";

        res.Add(key, query);

        var idxName = $"uc_{_definition.ModelType.Name}_{string.Join('_', constraint.Properties.Keys)}_idx".ToLower();
        var idxKey = $"CREATE INDEX {idxName}";
        var idxQuery = $"CREATE UNIQUE INDEX IF NOT EXISTS {idxName} ON {GetTableName(includeAlias: false)} ({string.Join(',', constraint.Properties.Keys)}) {(constraint.IncludedProperties.Any() ? $"INCLUDE ({string.Join(',', constraint.IncludedProperties.Keys)})" : string.Empty)};";

        res.Add(idxKey, idxQuery);

        return res;
    }

    private OrderedDictionary<string, string> CreateForeignKeyConstraint(DbRelationDefinition foreignKey)
    {
        var res = new OrderedDictionary<string, string>();

        if (!_definition.ModelType.GetProperties().ToList().Exists(t => t.Name.Equals(foreignKey.BaseProperty, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception($"{_definition.ModelType.Name} does not contain the property '{foreignKey.BaseProperty}'");
        }

        if (!foreignKey.Ref.GetProperties().ToList().Exists(t => t.Name.Equals(foreignKey.RefProperty, StringComparison.OrdinalIgnoreCase)))
        {
            throw new Exception($"{foreignKey.Ref.Name} does not contain the property '{foreignKey.RefProperty}'");
        }

        var targetDef = _definitionRegistry.TryGetDefinition(foreignKey.Ref);
        if (targetDef == null)
        {
            throw new Exception("Type not found");
        }

        string name = string.IsNullOrEmpty(foreignKey.Name) ? $"{_definition.ModelType.Name}_{foreignKey.BaseProperty}" : foreignKey.Name;

        var key = $"ADD CONSTRAINT {GetTableName(includeAlias: false)}.{name}";
        var query = $"ALTER TABLE {GetTableName(includeAlias: false)} ADD CONSTRAINT {name} FOREIGN KEY ({foreignKey.BaseProperty}) REFERENCES {GetTableName(targetDef, includeAlias: false)} ({foreignKey.RefProperty}) {(foreignKey.UseCascadeDelete ? "ON DELETE CASCADE" : "ON DELETE SET NULL")};";

        res.Add(key, query);

        var idxName = $"fk_{_definition.ModelType.Name}_{foreignKey.BaseProperty}_{targetDef.ModelType.Name}_idx".ToLower();
        var idxKey = $"CREATE INDEX {idxName}";
        var idxQuery = $"CREATE INDEX IF NOT EXISTS {idxName} ON {GetTableName(includeAlias: false)} ({foreignKey.BaseProperty});";

        res.Add(idxKey, idxQuery);

        return res;
    }

    private string CreateHistoryView(bool isTranslation)
    {

        string tableName = GetTableName(includeAlias: true, useHistory: false, useTranslation: isTranslation);
        string columnDefinitions = string.Join(',', _definition.Properties.Select(t => t.Name));

        if (isTranslation)
        {
            columnDefinitions += ", language";
        }

        string historyColumns = $"{columnDefinitions}, audit_validfrom, audit_validto, audit_changedby, audit_changedbysystem, audit_changeoperation, audit_deletedby, audit_deletedbysystem, audit_deletedoperation";
        string baseColumns = $"{columnDefinitions}, audit_validfrom, now() as audit_validto, audit_changedby, audit_changedbysystem, audit_changeoperation, null::uuid AS audit_deletedby, null::uuid AS audit_deletedbysystem, null::uuid AS audit_deletedoperation";

        string viewQuery = $"""
        CREATE OR REPLACE VIEW {GetTableName(includeAlias: false, useHistory: true, useHistoryView: true, useTranslation: isTranslation)} AS
        SELECT {historyColumns}
        FROM  {GetTableName(useHistory: true, useTranslation: isTranslation)}
        WHERE audit_validfrom <= coalesce(current_setting('app.asof', true)::timestamptz, now())
        AND audit_validto > coalesce(current_setting('app.asof', true)::timestamptz, now())
        UNION ALL
        SELECT {baseColumns}
        FROM {tableName}
        WHERE audit_validfrom <= coalesce(current_setting('app.asof', true)::timestamptz, now());
        """;

        return viewQuery;
    }

    private OrderedDictionary<string, string> CreateAuditMetadataFunction()
    {
        var scripts = new OrderedDictionary<string, string>();

        var sb = new StringBuilder();
        sb.AppendLine("CREATE OR REPLACE FUNCTION dbo.set_audit_metadata_fn()");
        sb.AppendLine("RETURNS TRIGGER AS $$");
        sb.AppendLine("DECLARE");
        sb.AppendLine("current_user UUID;");
        sb.AppendLine("current_system UUID;");
        sb.AppendLine("current_operation UUID;");
        sb.AppendLine("BEGIN");
        sb.AppendLine("SELECT current_setting('app.current_user', false) INTO current_user;");
        sb.AppendLine("SELECT current_setting('app.current_system', false) INTO current_system;");
        sb.AppendLine("SELECT current_setting('app.current_operation', false) INTO current_operation;");
        sb.AppendLine("NEW.audit_changedby := current_user;");
        sb.AppendLine("NEW.audit_changedbysystem := current_system;");
        sb.AppendLine("NEW.audit_changeoperation := current_operation;");
        sb.AppendLine("NEW.audit_validfrom := now();");
        sb.AppendLine("RETURN NEW;");
        sb.AppendLine("END;");
        sb.AppendLine("$$ LANGUAGE plpgsql;");

        scripts.Add($"CREATE FUNCTION dbo.set_audit_metadata_fn", sb.ToString());

        return scripts;
    }

    private OrderedDictionary<string, string> CreateMetaDataTrigger(bool isTranslation)
    {
        var scripts = new OrderedDictionary<string, string>();

        string modelName = _definition.ModelType.Name;
        string schema = GetSchemaName(useHistory: false, useTranslation: isTranslation);
        string functionName = $"set_audit_metadata_fn";
        string tableName = GetTableName(includeAlias: false, useTranslation: isTranslation);

        var sb = new StringBuilder();
        sb.AppendLine($"CREATE OR REPLACE TRIGGER {modelName}_Meta BEFORE INSERT OR UPDATE ON {tableName}");
        sb.AppendLine($"FOR EACH ROW EXECUTE FUNCTION dbo.{functionName}();");

        scripts.Add($"CREATE TRIGGER {schema}.{modelName}_Meta", sb.ToString());

        return scripts;
    }

    private OrderedDictionary<string, string> CreateAuditFunction(bool isTranslation)
    {
        var scripts = new OrderedDictionary<string, string>();

        string tableName = GetTableName(includeAlias: false, useTranslation: isTranslation);
        string columnDefinitions = string.Join(',', _definition.Properties.Select(t => t.Name));
        string columnOldDefinitions = string.Join(',', _definition.Properties.Select(t => $"OLD.{t.Name}"));

        string modelName = _definition.ModelType.Name;
        string historyTableName = GetTableName(includeAlias: false, useHistory: true, useTranslation: isTranslation);
        string schema = GetSchemaName(useHistory: false, useTranslation: isTranslation);
        
        if (isTranslation)
        {
            columnDefinitions += ", language";
            columnOldDefinitions += ", OLD.language";
        }

        var sb = new StringBuilder();
        sb.AppendLine($"CREATE OR REPLACE FUNCTION {schema}.audit_{modelName}_fn()");
        sb.AppendLine("RETURNS TRIGGER AS $$");
        sb.AppendLine("DECLARE");
        sb.AppendLine("current_user UUID;");
        sb.AppendLine("current_system UUID;");
        sb.AppendLine("current_operation UUID;");
        sb.AppendLine("BEGIN");
        sb.AppendLine("SELECT current_setting('app.current_user', false) INTO current_user;");
        sb.AppendLine("SELECT current_setting('app.current_system', false) INTO current_system;");
        sb.AppendLine("SELECT current_setting('app.current_operation', false) INTO current_operation;");

        sb.AppendLine("IF TG_OP = 'UPDATE' THEN");
        sb.AppendLine($"INSERT INTO {historyTableName} (");
        sb.AppendLine($"{columnDefinitions},");
        sb.AppendLine("audit_validfrom, audit_validto,");
        sb.AppendLine("audit_changedby, audit_changedbysystem, audit_changeoperation");
        sb.AppendLine(") VALUES (");
        sb.AppendLine($"{columnOldDefinitions},");
        sb.AppendLine("OLD.audit_validfrom, now(),");
        sb.AppendLine("OLD.audit_changedby, OLD.audit_changedbysystem, OLD.audit_changeoperation");
        sb.AppendLine(");");
        sb.AppendLine("RETURN NEW;");
        
        sb.AppendLine("ELSIF TG_OP = 'DELETE' THEN");
        sb.AppendLine($"INSERT INTO {historyTableName} (");
        sb.AppendLine($"{columnDefinitions},");
        sb.AppendLine("audit_validfrom, audit_validto,");
        sb.AppendLine("audit_changedby, audit_changedbysystem, audit_changeoperation,");
        sb.AppendLine("audit_deletedby, audit_deletedbysystem, audit_deleteoperation");
        sb.AppendLine(") VALUES (");
        sb.AppendLine($"{columnOldDefinitions},");
        sb.AppendLine("OLD.audit_validfrom, now(),");
        sb.AppendLine("OLD.audit_changedby, OLD.audit_changedbysystem, OLD.audit_changeoperation,");
        sb.AppendLine("current_user, current_system, current_operation");
        sb.AppendLine(");");
        sb.AppendLine("RETURN OLD;");
        sb.AppendLine("END IF;");
        
        sb.AppendLine("RETURN NULL;");
        sb.AppendLine("END;");
        sb.AppendLine("$$ LANGUAGE plpgsql;");

        scripts.Add($"CREATE FUNCTION {schema}.audit_{modelName}_fn", sb.ToString());

        return scripts;
    }

    private OrderedDictionary<string, string> CreateAuditTrigger(bool isTranslation)
    {
        var scripts = new OrderedDictionary<string, string>();

        string modelName = _definition.ModelType.Name;
        string schema = GetSchemaName(useHistory: false, useTranslation: isTranslation);
        string functionName = $"audit_{modelName}_fn";
        string tableName = GetTableName(includeAlias: false, useTranslation: isTranslation);

        var sb = new StringBuilder();
        sb.AppendLine($"CREATE OR REPLACE TRIGGER {modelName}_Audit AFTER UPDATE OR DELETE ON {tableName}");
        sb.AppendLine($"FOR EACH ROW EXECUTE FUNCTION {schema}.{functionName}();");

        scripts.Add($"CREATE TRIGGER {schema}.{modelName}_Audit", sb.ToString());

        return scripts;
    }
    #endregion
}
