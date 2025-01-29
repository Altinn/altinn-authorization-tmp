using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Models;
using FastMember;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Altinn.AccessMgmt.DbAccess.Data.Services.Mssql;

/// <summary>
/// Data Service
/// </summary>
/// <typeparam name="T">For type</typeparam>
public class SqlBasicRepo<T> : IDbBasicRepo<T>
    where T : class
{
    private readonly SqlConnection connection;
    private readonly string connectionString;

    /// <summary>
    /// Database definition of type
    /// </summary>
    public ObjectDefinition DbObjDef { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlBasicRepo{T}"/> class.
    /// </summary>
    /// <param name="config">Configuration</param>
    public SqlBasicRepo(IConfiguration config)
    {
        var configSection = config.GetRequiredSection("SqlBasicRepo");
        connectionString = configSection["ConnectionString"] ?? throw new Exception("Missing connectionstring");
        connection = new SqlConnection(connectionString);

        DbObjDef = DbDefinitions.Get<T>() ?? throw new Exception($"Definition for '{typeof(T).Name}' not found");
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get(List<GenericFilter>? parameters = null, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var jsonResult = await GetJson(parameters, options, cancellationToken);
        if (string.IsNullOrEmpty(jsonResult))
        {
            return [];
        }

        return JsonSerializer.Deserialize<IEnumerable<T>>(jsonResult) ?? throw new Exception("Unable to deserialize data");
    }

    /// <inheritdoc/>
    public async Task<(IEnumerable<T> Data, PagedResult PageInfo)> Search(string term, RequestOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await GetJson([new GenericFilter("Name", term, comparer: FilterComparer.Contains)], options, cancellationToken);
            var data = JsonSerializer.Deserialize<IEnumerable<T>>(json) ?? throw new Exception("Unable to deserialize data");
            var pageInfo = JsonSerializer.Deserialize<List<DbPageResult>>(json) ?? throw new Exception("Unable to deserialize page data");

            var info = pageInfo.First();
            var paged = new PagedResult()
            {
                PageCount = (info.TotalItems + options.PageSize - 1) / options.PageSize,
                ItemCount = pageInfo.First().TotalItems
            };
            paged.CurrentPage = options.PageNumber;
            paged.PageSize = options.PageSize;

            return (data, paged);
        }
        catch
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public async Task Ingest(List<T> data, CancellationToken cancellationToken = default)
    {
        using var conn = new SqlConnection(connection.ConnectionString);
        using (var bcp = new SqlBulkCopy(connection))
        using (var reader = ObjectReader.Create(data, DbObjDef.Properties.Select(t => t.Key).ToArray()))
        {
            // bcp.NotifyAfter = 1000;
            bcp.BatchSize = 1000;
            bcp.BulkCopyTimeout = 1000;
            bcp.DestinationTableName = DbObjDef.BaseDbObject.GetSqlDefinition(includeAlias: false);
            await bcp.WriteToServerAsync(reader, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task<int> CreateTranslation(T entity, string language, CancellationToken cancellationToken = default)
    {
        if (DbObjDef.TranslationDbObject == null)
        {
            return 0;
        }

        var param = GetTranslationEntityAsSqlParameter(entity);
        param.Add("Language", new SqlParameter("Language", language));
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"INSERT {DbObjDef.TranslationDbObject.GetSqlDefinition(includeAlias: false)} ({InsertColumns([.. param.Keys])}) VALUES({InsertValues([.. param.Keys])})";
        cmd.Parameters.AddRange([.. param.Values]);
        return await ExecuteCommand(cmd, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Create(T entity, CancellationToken cancellationToken = default)
    {
        var param = GetEntityAsSqlParameter(entity);
        using var cmd = connection.CreateCommand();
        cmd.CommandText = $"INSERT {DbObjDef.BaseDbObject.GetSqlDefinition(includeAlias: false)} ({InsertColumns([.. param.Keys])}) VALUES({InsertValues([.. param.Keys])})";
        cmd.Parameters.AddRange([.. param.Values]);
        return await ExecuteCommand(cmd, cancellationToken);
    }

    private string GetCommand(RequestOptions? options = null, List<GenericFilter>? filters = null, CancellationToken cancellationToken = default)
    {
        options ??= new RequestOptions();
        StringBuilder sb = new StringBuilder();

        if (options.UsePaging)
        {
            sb.AppendLine("WITH [PagedResult] AS (");
        }

        sb.AppendLine("SELECT ");
        sb.AppendLine(GenerateColumns(options));
        if (options.UsePaging)
        {
            string orderBy = string.IsNullOrEmpty(options.OrderBy) ? "Id" : options.OrderBy;
            sb.AppendLine($",ROW_NUMBER() OVER (ORDER BY [{DbObjDef.BaseDbObject.Alias}].[{orderBy}]) AS [_RowNum]");
        }

        sb.AppendLine(" FROM " + GenerateSource(options));

        if (filters != null && filters.Count > 0)
        {
            sb.AppendLine("WHERE " + string.Join(" AND ", filters.Select(t => $"[{DbObjDef.BaseDbObject.Alias}].[{t.PropertyName}] {t.Comparer} @{t.PropertyName}")));
        }

        if (options.UsePaging)
        {
            sb.AppendLine(")");
            sb.AppendLine("SELECT *");
            sb.AppendLine("FROM [PagedResult], (SELECT MAX([PagedResult].[_RowNum]) AS [TotalItems] FROM [PagedResult]) AS PageInfo");
            sb.AppendLine($"ORDER BY [_RowNum] OFFSET {options.PageSize * (options.PageNumber - 1)} ROWS FETCH NEXT {options.PageSize} ROWS ONLY");
        }

        return sb.ToString();
    }

    private async Task<string> GetJson(List<GenericFilter>? parameters = null, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new RequestOptions();
        parameters ??= [];

        var cmd = GetCommand(options, parameters);

        if (options.Language != null)
        {
            parameters.Add(new GenericFilter("Language", options.Language));
        }

        if (options.AsOf.HasValue)
        {
            parameters.Add(new GenericFilter("_AsOf", options.AsOf.Value));
        }

        return await ExecuteForJson(cmd, parameters: parameters, singleResult: false, cancellationToken);
    }

    private string InsertColumns(List<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"[{t}]").ToList());
    }

    private string InsertValues(List<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"@{t}").ToList());
    }

    /// <inheritdoc/>
    public async Task<int> UpdateTranslation(Guid id, T entity, string language, CancellationToken cancellationToken = default)
    {
        if (DbObjDef.TranslationDbObject == null)
        {
            return 0;
        }

        var param = GetTranslationEntityAsSqlParameter(entity);
        using var cmd = connection.CreateCommand();
        cmd.Parameters.AddRange([.. param.Values]);
        cmd.Parameters.Add(new SqlParameter("_id", id));
        cmd.Parameters.Add(new SqlParameter("_language", language));
        cmd.CommandText = $"UPDATE {DbObjDef.TranslationDbObject.GetSqlDefinition(includeAlias: false)} SET {UpdateSetStatement([.. param.Keys])} WHERE [Id] = @_id AND [Language] = @_language";
        return await ExecuteCommand(cmd, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, T entity, CancellationToken cancellationToken = default)
    {
        var param = GetEntityAsSqlParameter(entity);
        using var cmd = connection.CreateCommand();
        cmd.Parameters.AddRange([.. param.Values]);
        cmd.Parameters.Add(new SqlParameter("_id", id));
        cmd.CommandText = $"UPDATE {DbObjDef.BaseDbObject.GetSqlDefinition(includeAlias: false)} SET {UpdateSetStatement([.. param.Keys])} WHERE [Id] = @_id";
        return await ExecuteCommand(cmd, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Upsert(Guid id, T entity, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        var param = GetEntityAsSqlParameter(entity);
        sb.AppendLine($"UPDATE {DbObjDef.BaseDbObject.GetSqlDefinition(includeAlias: false)} SET {UpdateSetStatement([.. param.Keys])} WHERE Id = @_id");
        sb.AppendLine("IF (@@ROWCOUNT = 0) BEGIN");
        sb.AppendLine($"INSERT INTO {DbObjDef.BaseDbObject.GetSqlDefinition(includeAlias: false)} ({InsertColumns([.. param.Keys])}) VALUES({InsertValues([.. param.Keys])})");
        sb.AppendLine("END");
        param.Add("_id", new SqlParameter("_id", id));

        using var cmd = connection.CreateCommand();
        cmd.Parameters.AddRange([.. param.Values]);
        cmd.Parameters.Add(new SqlParameter("_id", id));
        cmd.CommandText =sb.ToString();
        return await ExecuteCommand(cmd, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, List<GenericParameter> parameters, CancellationToken cancellationToken = default)
    {
        var param = new Dictionary<string, SqlParameter>();
        foreach (var parameter in parameters)
        {
            param.Add(parameter.Key, new SqlParameter(parameter.Key, parameter.Value));
        }

        using var cmd = connection.CreateCommand();
        cmd.Parameters.AddRange([.. param.Values]);
        cmd.Parameters.Add(new SqlParameter("_id", id));
        cmd.CommandText = $"UPDATE {DbObjDef.BaseDbObject.GetSqlDefinition(includeAlias: false)} SET {UpdateSetStatement([.. param.Keys])} WHERE [Id] = @_id";
        return await ExecuteCommand(cmd, cancellationToken);
    }

    private string UpdateSetStatement(List<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"[{t}] = @{t}").ToList());
    }

    /// <inheritdoc/>
    public async Task<int> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        using var cmd = connection.CreateCommand();
        cmd.Parameters.Add(new SqlParameter("_id", id));
        cmd.CommandText = $"DELETE {DbObjDef.BaseDbObject.GetSqlDefinition(includeAlias: false)} WHERE [Id] = @_id";
        return await ExecuteCommand(cmd, cancellationToken);
    }

    private async Task<bool> CheckIfTranslationExists(Guid id, string language, CancellationToken cancellationToken = default)
    {
        if (DbObjDef.TranslationDbObject == null)
        {
            return false;
        }

        using var cmd = connection.CreateCommand();
        cmd.Parameters.Add(new SqlParameter("Id", id));
        cmd.Parameters.Add(new SqlParameter("Language", language));
        cmd.CommandText = $"SELECT COUNT(*) AS [Cnt] FROM {DbObjDef.TranslationDbObject.GetSqlDefinition(includeAlias: false)} WHERE [Id] = @Id AND [Language] = @Language";
        var res = await ExecuteScalarCommand(cmd, cancellationToken);
        _ = int.TryParse(res?.ToString(), out int rowCount);

        if (rowCount > 0)
        {
            return true;
        }

        return false;
    }

    private async Task<object?> ExecuteScalarCommand(SqlCommand cmd, CancellationToken cancellationToken = default)
    {
        try
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            return await cmd.ExecuteScalarAsync(cancellationToken);
        }
        catch
        {
            Console.WriteLine(cmd.CommandText);
            foreach (SqlParameter param in cmd.Parameters)
            {
                Console.WriteLine($"{param.ParameterName}:{param.Value}");
            }

            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    private async Task<int> ExecuteCommand(SqlCommand cmd, CancellationToken cancellationToken = default)
    {
        try
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            return await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        catch
        {
            Console.WriteLine(cmd.CommandText);
            foreach (SqlParameter param in cmd.Parameters)
            {
                Console.WriteLine($"{param.ParameterName}:{param.Value}");
            }

            throw;
        }
        finally
        {
            connection.Close();
        }
    }

    private List<SqlParameter> ConvertToSqlParamter(List<GenericFilter> filters)
    {
        var result = new List<SqlParameter>();
        foreach (var filter in filters)
        {
            result.Add(ConvertToSqlParamter(filter));
        }

        return result;
    }

    private SqlParameter ConvertToSqlParamter(GenericFilter filter)
    {
        return new SqlParameter(filter.PropertyName, filter.Value);
    }

    /// <summary>
    /// Executes query and returns json
    /// </summary>
    /// <param name="query">Query</param>
    /// <param name="parameters">Parameters</param>
    /// <param name="singleResult">Expect pnly one result</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    protected async Task<string> ExecuteForJson(string query, List<GenericFilter>? parameters, bool singleResult = false, CancellationToken cancellationToken = default)
    {
        using var conn = new SqlConnection(connectionString);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = query;
        cmd.CommandText += " FOR JSON PATH";
        if (singleResult)
        {
            cmd.CommandText += ", WITHOUT_ARRAY_WRAPPER";
        }

        if (parameters != null && parameters.Any())
        {
            cmd.Parameters.AddRange(ConvertToSqlParamter(parameters).ToArray());
        }

        try
        {
            Console.WriteLine(cmd.CommandText);
            conn.Open();
            var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            StringBuilder sb = new StringBuilder();
            while (await reader.ReadAsync(cancellationToken))
            {
                sb.Append(reader.GetString(0));
            }

            return sb.ToString();
        }
        catch
        {
            Console.WriteLine(cmd.CommandText);
            throw;
        }
        finally
        {
            conn.Close();
        }
    }

    private Dictionary<string, SqlParameter> GetEntityAsSqlParameter(object entity)
    {
        var parameters = new Dictionary<string, SqlParameter>();
        foreach (PropertyInfo property in entity.GetType().GetProperties())
        {
            parameters.Add(property.Name, new SqlParameter(property.Name, property.GetValue(entity) ?? DBNull.Value));
        }

        return parameters;
    }

    private Dictionary<string, SqlParameter> GetTranslationEntityAsSqlParameter(object entity)
    {
        var parameters = new Dictionary<string, SqlParameter>();
        foreach (PropertyInfo property in entity.GetType().GetProperties())
        {
            if (property.PropertyType == typeof(string) || property.Name == "Id")
            {
                parameters.Add(property.Name, new SqlParameter(property.Name, property.GetValue(entity) ?? DBNull.Value));
            }
        }

        return parameters;
    }

    /// <summary>
    /// Generate sources for query 
    /// </summary>
    /// <param name="options">RequestOptions</param>
    /// <returns></returns>
    protected string GenerateSource(RequestOptions options)
    {
        return GenerateSource(DbObjDef, options);
    }

    /// <summary>
    /// Generate sources for query
    /// </summary>
    /// <param name="dbObjDef">Object definition</param>
    /// <param name="options">RequestOptions</param>
    /// <returns></returns>
    protected string GenerateSource(ObjectDefinition dbObjDef, RequestOptions options)
    {
        bool useTranslation = !string.IsNullOrEmpty(options.Language);
        string asOfCommand = options.AsOf.HasValue ? " FOR SYSTEM_TIME AS OF @_AsOf " : "";

        if (dbObjDef.TranslationDbObject == null)
        {
            useTranslation = false;
        }

        // TODO: If table hasHistory
        if (useTranslation)
        {
            return $"""
            {dbObjDef.BaseDbObject.GetSqlDefinition(useAsOf: options.AsOf.HasValue)}
            OUTER APPLY (SELECT TOP(1) [T].* FROM {dbObjDef.TranslationDbObject?.GetSqlDefinition(includeAlias: false, useAsOf: options.AsOf.HasValue)} AS [T] WHERE [T].[Id] = [{dbObjDef.BaseDbObject.Alias}].[Id] AND [T].[Language] = @Language) AS [{dbObjDef.TranslationDbObject?.Alias}]
            """;
        }
        else
        {
            return dbObjDef.BaseDbObject.GetSqlDefinition(useAsOf: options.AsOf.HasValue);
        }
    }

    /// <summary>
    /// Generate columns for query
    /// </summary>
    /// <param name="options">RequestOptions</param>
    /// <returns></returns>
    protected string GenerateColumns(RequestOptions options)
    {
        return GenerateColumns(DbObjDef, options);
    }

    /// <summary>
    /// Generate columns for query
    /// </summary>
    /// <param name="dbObjDef">Object definition</param>
    /// <param name="options">RequestOptions</param>
    /// <returns></returns>
    protected string GenerateColumns(ObjectDefinition dbObjDef, RequestOptions options)
    {
        bool useTranslation = !string.IsNullOrEmpty(options.Language);
        if (dbObjDef.TranslationDbObject == null)
        {
            useTranslation = false;
        }

        var columns = new List<string>();
        foreach (var p in dbObjDef.Properties)
        {
            if (useTranslation && p.Value.PropertyType == typeof(string))
            {
                columns.Add($"ISNULL([{dbObjDef.TranslationDbObject?.Alias}].[{p.Key}],[{dbObjDef.BaseDbObject.Alias}].[{p.Key}]) AS [{p.Key}]");
            }
            else
            {
                columns.Add($"[{dbObjDef.BaseDbObject.Alias}].[{p.Key}] AS [{p.Key}]");
            }
        }

        return string.Join(',', columns);
    }
}
