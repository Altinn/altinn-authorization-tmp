using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using NpgsqlTypes;
using OpenTelemetry.Trace;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Services.Postgres;

/// <summary>
/// Postgres implementation of IDbBasicRepo
/// </summary>
/// <typeparam name="T"></typeparam>
public class PostgresBasicRepo<T> : IDbBasicRepo<T>
    where T : class, new()
{
    /// <summary>
    /// Connection
    /// </summary>
    protected string ConnectionString { get; }

    /// <summary>
    /// DbConverter
    /// </summary>
    protected readonly DbConverter DbConverter;

    /// <summary>
    /// Database object definition for T
    /// </summary>
    protected ObjectDefinition DbObjDef { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresBasicRepo{T}"/> class.
    /// </summary>
    /// <param name="config">Configuration</param>
    /// <param name="dbConverter">DbConverter</param>
    public PostgresBasicRepo(IOptions<DbAccessDataConfig> config, DbConverter dbConverter)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        var configuration = config.Value;
        ConnectionString = configuration.ConnectionString;
        DbObjDef = DbDefinitions.Get<T>() ?? throw new Exception($"Definition for '{typeof(T).Name}' not found");
        DbConverter = dbConverter;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get(List<GenericFilter>? parameters = null, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new RequestOptions();
        parameters ??= [];

        var cmd = GetCommand(options, parameters);
        var param = new Dictionary<string, object>();
        if (parameters.Count > 0)
        {
            foreach (var kv in parameters)
            {
                param.Add(kv.Key, kv.Value);
            }
        }

        if (options.Language != null)
        {
            param.Add("Language", options.Language);
        }

        if (options.AsOf.HasValue)
        {
            param.Add("_AsOf", options.AsOf.Value);
        }

        return await Execute(cmd, param, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IEnumerable<T> Data, PagedResult PageInfo)> Search(string term, RequestOptions options, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
        /*
        // SQL Implementation
        try
        {
            var json = await GetJson([new GenericFilter("Name", term.GetValue(), comparer: "LIKE")], options, cancellationToken);
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
        */
    }

    /// <inheritdoc/>
    public async Task Ingest(List<T> data, CancellationToken cancellationToken = default)
    {
        using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

        var dt = new DataTable();
        var dataAdapter = new NpgsqlDataAdapter($"select * from {DbObjDef.BaseDbObject.GetPostgresDefinition(includeAlias: false)} LIMIT 10", conn);
        dataAdapter.Fill(dt);
        dt.Clear();

        var columns = new Dictionary<string, (NpgsqlDbType Type, PropertyInfo Property)>();
        foreach (DataColumn c in dt.Columns)
        {
            columns.Add(c.ColumnName, (PostgresDataTypeConverter.GetPostgresType(c.DataType), DbObjDef.Properties.Values.First(t => t.Name.Equals(c.ColumnName, StringComparison.CurrentCultureIgnoreCase))));
        }

        using var writer = await conn.BeginBinaryImportAsync($"COPY {DbObjDef.BaseDbObject.GetPostgresDefinition(includeAlias: false)} ({string.Join(',', columns.Keys)}) FROM STDIN (FORMAT BINARY)", cancellationToken: cancellationToken);
        writer.Timeout = new TimeSpan(0, 10, 0);
        int batchCompleted = 0;
        int batchSize = 1000;
        int completed = 0;
        foreach (var d in data)
        {
            writer.StartRow();
            foreach (var c in columns)
            {
                try
                {
                    writer.Write(c.Value.Property.GetValue(d), c.Value.Type);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write data in column '{c.Key}' for '{DbObjDef.BaseDbObject.Name}'. Trying to write null. " + ex.Message);
                    try
                    {
                        writer.WriteNull();
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to write null in column '{c.Key}' for '{DbObjDef.BaseDbObject.Name}'.");
                        throw;
                    }
                }
            }

            completed++;
            if (completed == batchSize)
            {
                batchCompleted++;
                completed = 0;
                Console.WriteLine($"Ingested {(batchCompleted * batchSize) + completed}");
            }
        }

        Console.WriteLine($"Ingested {(batchCompleted * batchSize) + completed}");
        writer.Complete();
    }

    /// <inheritdoc/>
    public async Task<int> Create(T entity, CancellationToken cancellationToken = default)
    {
        var param = GetEntityAsSqlParameter(entity);
        string query = $"INSERT INTO {DbObjDef.BaseDbObject.GetPostgresDefinition(includeAlias: false)} ({InsertColumns([.. param.Keys])}) VALUES({InsertValues([.. param.Keys])})";
        return await ExecuteCommand(query, [.. param.Values], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> CreateTranslation(T entity, string language, CancellationToken cancellationToken = default)
    {
        if (DbObjDef.TranslationDbObject == null)
        {
            return 0;
        }

        var param = GetTranslationEntityAsSqlParameter(entity);
        param.Add("Language", new NpgsqlParameter("Language", language));
        var query = $"INSERT INTO {DbObjDef.TranslationDbObject.GetPostgresDefinition(includeAlias: false)} ({InsertColumns([.. param.Keys])}) VALUES({InsertValues([.. param.Keys])})";
        return await ExecuteCommand(query, [.. param.Values], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> UpdateTranslation(Guid id, T entity, string language, CancellationToken cancellationToken = default)
    {
        if (DbObjDef.TranslationDbObject == null)
        {
            return 0;
        }

        var param = GetTranslationEntityAsSqlParameter(entity);
        string query = $"UPDATE {DbObjDef.TranslationDbObject.GetPostgresDefinition(includeAlias: false)} SET {UpdateSetStatement([.. param.Keys])} WHERE Id = @_id AND Language = @_language";
        param.Add("_id", new NpgsqlParameter("_id", id));
        param.Add("Language", new NpgsqlParameter("_language", language));
        return await ExecuteCommand(query, [.. param.Values], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, T entity, CancellationToken cancellationToken = default)
    {
        var param = GetEntityAsSqlParameter(entity);
        string query = $"UPDATE {DbObjDef.BaseDbObject.GetPostgresDefinition(includeAlias: false)} SET {UpdateSetStatement([.. param.Keys])} WHERE Id = @_id";
        param.Add("_id", new NpgsqlParameter("_id", id));
        return await ExecuteCommand(query, [.. param.Values], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, List<GenericParameter> parameters, CancellationToken cancellationToken = default)
    {
        string query = $"UPDATE {DbObjDef.BaseDbObject.GetPostgresDefinition(includeAlias: false)} SET {UpdateSetStatement(parameters.Select(t => t.Key).ToList())} WHERE id = @_id";
        parameters.Add(new GenericParameter("_id", id));
        return await ExecuteCommand(query, parameters, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        string query = $"DELETE FROM {DbObjDef.BaseDbObject.GetPostgresDefinition(includeAlias: false)} WHERE id = @_id";
        return await ExecuteCommand(query, [new NpgsqlParameter("_id", id)], cancellationToken: cancellationToken);
    }

    #region Internal
    private string UpdateSetStatement(List<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"{t} = @{t}").ToList());
    }

    private string InsertColumns(List<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"{t}").ToList());
    }

    private string InsertValues(List<string> values)
    {
        return string.Join(',', values.OrderBy(t => t).Select(t => $"@{t}").ToList());
    }

    private string GetCommand(RequestOptions? options = null, List<GenericFilter>? filters = null)
    {
        /* check from cache */

        options ??= new RequestOptions();
        var sb = new StringBuilder();
        
        if (options.UsePaging)
        {
            sb.AppendLine("WITH pagedresult AS (");
        }

        sb.AppendLine("SELECT ");
        sb.AppendLine(GenerateColumns(options));

        if (options.UsePaging)
        {
            string orderBy = string.IsNullOrEmpty(options.OrderBy) ? "Id" : options.OrderBy;
            sb.AppendLine($",ROW_NUMBER() OVER (ORDER BY {DbObjDef.BaseDbObject.Alias}.{orderBy}) AS _rownum");
        }

        sb.AppendLine("FROM " + GenerateSource(options));
        if (filters != null && filters.Count > 0)
        {
            sb.AppendLine("WHERE " + string.Join(" AND ", filters.Select(t => $"{DbObjDef.BaseDbObject.Alias}.{t.Key} {t.Comparer} @{t.Key}")));
        }

        if (options.UsePaging)
        {
            sb.AppendLine(")");
            sb.AppendLine("SELECT *");
            sb.AppendLine("FROM pagedresult, (SELECT MAX(pagedresult._rownum) AS totalitems FROM pagedresult) AS pageinfo");
            sb.AppendLine($"ORDER BY _rownum OFFSET {options.PageSize * (options.PageNumber - 1)} ROWS FETCH NEXT {options.PageSize} ROWS ONLY");
        }

        /* save to cache */

        return sb.ToString();
    }

    private async Task<int> ExecuteCommand(string query, List<GenericParameter> parameters, CancellationToken cancellationToken = default)
    {
        var queryParameters = new List<NpgsqlParameter>();
        if (parameters != null && parameters.Count > 0)
        {
            foreach (var p in parameters)
            {
                queryParameters.Add(new NpgsqlParameter(p.Key, p.Value));
            }
        }

        return await ExecuteCommand(query, queryParameters, cancellationToken);
    }

    /// <summary>
    /// Execute command
    /// </summary>
    /// <param name="query">Query</param>
    /// <param name="parameters">Parameters</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>TA <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    private async Task<int> ExecuteCommand(string query, List<NpgsqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            using var cmd = connection.CreateCommand();
            cmd.Parameters.AddRange(parameters.ToArray());
            cmd.CommandText = query;
            connection.Open();
            return await cmd.ExecuteNonQueryAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(query);
            foreach (NpgsqlParameter param in parameters)
            {
                Console.WriteLine($"{param.ParameterName}:{param.Value}");
            }

            throw;
        }
    }

    /// <summary>
    /// Execute query
    /// </summary>
    /// <param name="query">Query</param>
    /// <param name="parameters">Parameters</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    private async Task<IEnumerable<T>> Execute(string query, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var c = new NpgsqlConnection(ConnectionString);
            var cmd = new CommandDefinition(query, parameters, cancellationToken: cancellationToken);
            //// Console.WriteLine(query);
            return DbConverter.ConvertToObjects<T>(await c.ExecuteReaderAsync(cmd));
        }
        catch (Exception ex)
        {
            Console.WriteLine(query);
            Console.WriteLine(ex.Message);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    Console.WriteLine($"{param.Key}:{param.Value}");
                }
            }

            throw;
        }
    }

    /// <summary>
    /// Get translation parameters
    /// Ignores non-string values
    /// </summary>
    /// <param name="entity">Translated object</param>
    /// <returns></returns>
    private Dictionary<string, NpgsqlParameter> GetTranslationEntityAsSqlParameter(object entity)
    {
        var parameters = new Dictionary<string, NpgsqlParameter>();
        foreach (PropertyInfo property in entity.GetType().GetProperties())
        {
            if (property.PropertyType == typeof(string) || property.Name == "Id")
            {
                parameters.Add(property.Name, new NpgsqlParameter(property.Name, property.GetValue(entity) ?? DBNull.Value));
            }
        }

        return parameters;
    }

    /// <summary>
    /// PostgresDataTypeConverter
    /// </summary>
    internal static class PostgresDataTypeConverter
    {
        /// <summary>
        /// GetPostgresType
        /// </summary>
        /// <param name="type">Type</param>
        /// <returns></returns>
        public static NpgsqlDbType GetPostgresType(Type type)
        {
            if (type == typeof(string))
            {
                return NpgsqlDbType.Text;
            }

            if (type == typeof(int))
            {
                return NpgsqlDbType.Integer;
            }

            if (type == typeof(DateTimeOffset))
            {
                return NpgsqlDbType.TimestampTz;
            }

            if (type == typeof(Guid))
            {
                return NpgsqlDbType.Uuid;
            }

            if (type == typeof(bool))
            {
                return NpgsqlDbType.Boolean;
            }

            Console.WriteLine($"Typeconverter not found for '{type.Name}'");

            return NpgsqlDbType.Text;
        }
    }

    /// <summary>
    /// Generate parameters based on object
    /// </summary>
    /// <param name="entity">Object</param>
    /// <returns></returns>
    protected Dictionary<string, NpgsqlParameter> GetEntityAsSqlParameter(object entity)
    {
        var parameters = new Dictionary<string, NpgsqlParameter>();
        foreach (PropertyInfo property in entity.GetType().GetProperties())
        {
            parameters.Add(property.Name, new NpgsqlParameter(property.Name, property.GetValue(entity) ?? DBNull.Value));
        }

        return parameters;
    }

    /// <summary>
    /// Generate source for query
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

        /*
        // Not implemented
        string AsOfCommand = options.AsOf.HasValue ? " FOR SYSTEM_TIME AS OF @_AsOf " : "";
        */

        if (dbObjDef.TranslationDbObject == null)
        {
            useTranslation = false;
        }

        if (useTranslation && dbObjDef.UseTranslation)
        {
            return $"""
            {dbObjDef.BaseDbObject.GetPostgresDefinition(useAsOf: options.AsOf.HasValue)}
            LEFT JOIN LATERAL (select * from {dbObjDef.TranslationDbObject?.GetPostgresDefinition(includeAlias: false, useAsOf: options.AsOf.HasValue)} as t where t.Id = {dbObjDef.BaseDbObject.Alias}.Id AND t.Language = @Language) as {dbObjDef.TranslationDbObject?.Alias} on 1=1
            """;
        }
        else
        {
            return dbObjDef.BaseDbObject.GetPostgresDefinition(useAsOf: options.AsOf.HasValue);
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
        foreach (var p in dbObjDef.Properties.Values)
        {
            if (useTranslation && dbObjDef.UseTranslation && p.PropertyType == typeof(string))
            {
                columns.Add($"coalesce({dbObjDef.TranslationDbObject?.Alias}.{p.Name},{dbObjDef.BaseDbObject.Alias}.{p.Name}) AS {p.Name}");
            }
            else
            {
                columns.Add($"{dbObjDef.BaseDbObject.Alias}.{p.Name} AS {p.Name}");
            }
        }

        return string.Join(',', columns);
    }
    #endregion
}
