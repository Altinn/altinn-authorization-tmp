using System.Text;
using System.Text.Json;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Services.Postgres;

/// <inheritdoc/>
public class PostgresExtendedRepo<T, TExtended> : PostgresBasicRepo<T>, IDbExtendedRepo<T, TExtended>
    where T : class
    where TExtended : class
{
    private List<Join> Joins { get; set; } = new List<Join>();
    private readonly IDbExtendedConverter<T, TExtended> dbExtendedConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresExtendedRepo{T, E}"/> class.
    /// </summary>
    /// <param name="config">IConfiguration</param>
    /// <param name="dbConverter">IDbBasicConverter</param>
    public PostgresExtendedRepo(IConfiguration config, IDbExtendedConverter<T, TExtended> dbConverter) : base(config, dbConverter)
    {
        dbExtendedConverter = dbConverter;
    }

    /// <inheritdoc/>
    public async Task<(IEnumerable<TExtended> Data, PagedResult PageInfo)> SearchExtended(string term, RequestOptions? options = null, bool startsWith = false)
    {
        var data = await GetExtended([new GenericFilter("Name", term, comparer: startsWith ? DbOperators.StartsWith : DbOperators.Contains)], options);
        var paged = new PagedResult()
        {
            PageCount = 1,
            ItemCount = data.Count(),
            CurrentPage = 0,
            PageSize = data.Count()
        };

        return (data, paged);
    }

    /// <inheritdoc/>
    public void Join<TJoin>(string? alias = null, string baseJoinProperty = "", string joinProperty = "Id", bool optional = false)
    {
        alias = string.IsNullOrEmpty(alias) ? typeof(TJoin).Name : alias;
        Joins.Add(new Join(alias, typeof(T), typeof(TJoin), baseJoinProperty, joinProperty, optional));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TExtended>> GetExtended(List<GenericFilter>? filters = null, RequestOptions? options = null)
    {
        options ??= new RequestOptions();
        var cmd = GetCommand(options, filters);
        var param = new Dictionary<string, object>();

        if (filters != null)
        {
            foreach (var filter in filters)
            {
                param.Add(filter.Key, filter.Value);
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

        return await ExecuteExtended(cmd, param);
    }

    #region Internal
    private async Task<IEnumerable<TExtended>> ExecuteExtended(string query, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            CommandDefinition cmd = new CommandDefinition(query, parameters, cancellationToken: cancellationToken);
            return dbExtendedConverter.ConvertExtended(await connection.ExecuteReaderAsync(cmd));
        }
        catch
        {
            Console.WriteLine(query);
            throw;
        }
    }

    private string GetCommand(RequestOptions? options = null, List<GenericFilter>? filters = null)
    {
        options ??= new RequestOptions();
        StringBuilder sb = new StringBuilder();

        if (options.UsePaging)
        {
            sb.AppendLine("WITH PagedResult AS (");
        }

        sb.AppendLine("SELECT ");
        sb.AppendLine(GenerateColumns(options));
        foreach (var j in Joins)
        {
            sb.Append(",");
            sb.AppendLine(GenerateJoinPostgresColumns(j, options));
        }

        if (options.UsePaging)
        {
            string orderBy = string.IsNullOrEmpty(options.OrderBy) ? "Id" : options.OrderBy;
            sb.AppendLine($",ROW_NUMBER() OVER (ORDER BY {DbObjDef.BaseDbObject.Alias}.{orderBy}) AS _RowNum");
        }

        sb.AppendLine("FROM " + GenerateSource(options));
        foreach (var j in Joins)
        {
            var joinStatement = GetJoinPostgresStatement(j, options);
            sb.AppendLine(joinStatement.Query);
        }

        if (filters != null && filters.Count > 0)
        {
            sb.AppendLine("WHERE " + string.Join(" AND ", filters.Select(t => $"{DbObjDef.BaseDbObject.Alias}.{t.Key} {t.Comparer} @{t.Key}")));
        }

        if (options.UsePaging)
        {
            sb.AppendLine(")");
            sb.AppendLine("SELECT *");
            sb.AppendLine("FROM PagedResult, (SELECT MAX(PagedResult._RowNum) AS TotalItems FROM PagedResult) AS PageInfo");
            sb.AppendLine($"ORDER BY _RowNum OFFSET {options.PageSize * (options.PageNumber - 1)} ROWS FETCH NEXT {options.PageSize} ROWS ONLY");
        }

        // Console.WriteLine(sb.ToString());
        return sb.ToString();
    }

    private string GetJoinPostgresFilterString(Join join)
    {
        if (join.Filter == null || join.Filter.Count == 0)
        {
            return "";
        }

        string result = string.Empty;

        foreach (var filter in join.Filter)
        {
            result += $" AND {join.BaseObj.BaseDbObject.Alias}.{filter.Key} = _{join.Alias}.{filter.Value}";
        }

        return result;
    }

    /// <summary>
    /// Generate Postgres join statement
    /// </summary>
    /// <param name="join">Join</param>
    /// <param name="options">RequestOptions</param>
    /// <returns></returns>
    public (string Query, List<GenericFilter> Parameters) GetJoinPostgresStatement(Join join, RequestOptions options)
    {
        // TODO: If table hasHistory...
        if (options.AsOf.HasValue)
        {
            throw new NotImplementedException("AsOf feature not enabled in postgres");
        }

        string asOfCommand = options.AsOf.HasValue ? " FOR SYSTEM_TIME AS OF @_AsOf " : " ";
        var query = $"{(join.Optional ? "LEFT OUTER" : "INNER")} JOIN {join.JoinObj.BaseDbObject.GetPostgresDefinition(includeAlias: false)}{asOfCommand}AS _{join.Alias} ON {join.BaseObj.BaseDbObject.Alias}.{join.BaseJoinProperty} = _{join.Alias}.{join.JoinProperty} {GetJoinPostgresFilterString(join)}";

        bool useTranslation = !string.IsNullOrEmpty(options.Language);
        if (useTranslation && join.JoinObj.UseTranslation && join.JoinObj.TranslationDbObject != null)
        {
            query += $"""
            LEFT JOIN LATERAL (select * from {join.JoinObj.TranslationDbObject.GetPostgresDefinition(includeAlias: false)} {asOfCommand} as t where t.Id = _{join.Alias}.Id AND t.Language = @Language) as {join.Alias}{join.JoinObj.TranslationDbObject.Alias} on 1=1
            """;
        }

        return (Query: query, Parameters: join.Filter);
    }

    /// <summary>
    /// Generate Postgres columns
    /// </summary>
    /// <param name="join">Join</param>
    /// <param name="options">RequestOptions</param>
    /// <returns></returns>
    public string GenerateJoinPostgresColumns(Join join, RequestOptions options)
    {
        bool useTranslation = !string.IsNullOrEmpty(options.Language);
        var columns = new List<string>();
        foreach (var p in join.JoinObj.Properties.Values)
        {
            if (join.JoinObj.TranslationDbObject != null && join.JoinObj.UseTranslation && useTranslation && p.PropertyType == typeof(string))
            {
                columns.Add($"coalesce({join.Alias}{join.JoinObj.TranslationDbObject.Alias}.{p.Name},_{join.Alias}.{p.Name}) AS {join.Alias}_{p.Name}");
            }
            else
            {
                columns.Add($"_{join.Alias}.{p.Name} AS {join.Alias}_{p.Name}");
            }
        }

        return string.Join(',', columns);
    }
    #endregion
}
