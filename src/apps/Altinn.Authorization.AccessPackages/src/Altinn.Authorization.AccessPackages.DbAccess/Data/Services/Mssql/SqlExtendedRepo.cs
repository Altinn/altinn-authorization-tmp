using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Services.Mssql;

/// <inheritdoc/>
public class SqlExtendedRepo<T, TExtended> : SqlBasicRepo<T>, IDbExtendedRepo<T, TExtended>
    where T : class
    where TExtended : class
{
    private List<Join> Joins { get; set; } = [];

    /// <inheritdoc/>
    public void Join<TJoin>(Expression<Func<T, object>> TProperty, Expression<Func<TJoin, object>> TJoinProperty, Expression<Func<TExtended, object>> TExtendedProperty, bool optional = false)
    {
        Joins.Add(new Join()
        {
            Alias = ExtractPropertyInfoDirectly(TExtendedProperty).Name,
            BaseObj = DbDefinitions.Get<T>(),
            BaseJoinProperty = ExtractPropertyInfoDirectly(TProperty).Name,
            JoinObj = DbDefinitions.Get<TJoin>(),
            JoinProperty = ExtractPropertyInfoDirectly(TJoinProperty).Name,
            Optional = optional
        });

        PropertyInfo ExtractPropertyInfoDirectly<TLocal>(Expression<Func<TLocal, object>> expression)
        {
            MemberExpression memberExpression = (MemberExpression)expression.Body ?? ((UnaryExpression)expression.Body)?.Operand as MemberExpression;

            return memberExpression?.Member as PropertyInfo
                ?? throw new ArgumentException("Expression must refer to a property.");
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlExtendedRepo{T, E}"/> class.
    /// </summary>
    /// <param name="config">IConfiguration</param>
    public SqlExtendedRepo(IConfiguration config) : base(config) { }

    /// <inheritdoc/>
    public async Task<(IEnumerable<TExtended> Data, PagedResult PageInfo)> SearchExtended(string term, RequestOptions options, bool startsWith = false)
    {
        try
        {
            var json = await GetExtJson([new GenericFilter("Name", term, comparer: startsWith ? DbOperators.StartsWith : DbOperators.Contains)], options);

            var data = JsonSerializer.Deserialize<IEnumerable<TExtended>>(json) ?? throw new Exception("Unable to deserialize data");
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
    public async Task<IEnumerable<TExtended>> GetExtended(List<GenericFilter>? filters = null, RequestOptions? options = null)
    {
        options ??= new RequestOptions();
        filters ??= new List<GenericFilter>();
        var cmd = GetCommand(options, filters);

        if (options.Language != null)
        {
            filters.Add(new GenericFilter("Language", options.Language));
        }

        if (options.AsOf.HasValue)
        {
            filters.Add(new GenericFilter("_AsOf", options.AsOf.Value));
        }

        string jsonResult = await ExecuteForJson(cmd, parameters: filters, singleResult: false);
        if (string.IsNullOrEmpty(jsonResult))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<TExtended>>(jsonResult) ?? throw new Exception("Unable to deserialize data");
    }

    private async Task<string> GetExtJson(List<GenericFilter> filters, RequestOptions? options = null)
    {
        options ??= new RequestOptions();
        var cmd = GetCommand(options, filters);

        if (options.Language != null)
        {
            filters.Add(new GenericFilter("Language", options.Language));
        }

        if (options.AsOf.HasValue)
        {
            filters.Add(new GenericFilter("_AsOf", options.AsOf.Value));
        }

        try
        {
            return await ExecuteForJson(cmd, parameters: filters, singleResult: false);
        }
        catch
        {
            Console.WriteLine(cmd);
            throw;
        }
    }

    private string GetCommand(RequestOptions? options = null, List<GenericFilter>? filters = null)
    {
        options ??= new RequestOptions();
        StringBuilder sb = new StringBuilder();

        if (options.UsePaging)
        {
            sb.AppendLine("WITH [PagedResult] AS (");
        }

        sb.AppendLine("SELECT ");
        sb.AppendLine(GenerateColumns(options));
        foreach (var j in Joins)
        {
            sb.AppendLine(",");
            sb.Append(GenerateJoinColumns(j, options));
        }

        if (options.UsePaging)
        {
            string orderBy = string.IsNullOrEmpty(options.OrderBy) ? "Id" : options.OrderBy;
            sb.AppendLine($",ROW_NUMBER() OVER (ORDER BY [{DbObjDef.BaseDbObject.Alias}].[{orderBy}]) AS [_RowNum]");
        }

        sb.AppendLine(" FROM " + GenerateSource(options));
        foreach (var j in Joins)
        {
            var joinStatement = GetJoinStatement(j, options);
            sb.AppendLine(joinStatement.Query);
        }

        if (filters != null && filters.Count > 0)
        {
            sb.AppendLine("WHERE " + string.Join(" AND ", filters.Select(t => $"[{DbObjDef.BaseDbObject.Alias}].[{t.Key}] {t.Comparer} @{t.Key}")));
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

    private string GetJoinFilterString(Join join)
    {
        if (join.Filter == null || join.Filter.Count == 0)
        {
            return "";
        }

        string result = string.Empty;

        foreach (var filter in join.Filter)
        {
            result += $" AND [{join.BaseObj.BaseDbObject.Alias}].[{filter.Key}] {filter.Comparer} [{join.Alias}].[{filter.Value}]";
        }

        return result;
    }

    /// <summary>
    /// Generate mssql join statement
    /// </summary>
    /// <param name="join">Join</param>
    /// <param name="options">RequestOptions</param>
    /// <returns></returns>
    /// 
    public (string Query, List<GenericFilter> Parameters) GetJoinStatement(Join join, RequestOptions options)
    {
        // TODO: If table hasHistory...
        string asOfCommand = options.AsOf.HasValue ? " FOR SYSTEM_TIME AS OF @_AsOf " : "";

        var query = $"{(join.Optional ? "LEFT OUTER" : "INNER")} JOIN {join.JoinObj.BaseDbObject.GetSqlDefinition(includeAlias: false)} {asOfCommand} AS [{join.Alias}] ON [{join.BaseObj.BaseDbObject.Alias}].[{join.BaseJoinProperty}] = [{join.Alias}].[{join.JoinProperty}] {GetJoinFilterString(join)}";

        bool useTranslation = !string.IsNullOrEmpty(options.Language);
        if (useTranslation && join.JoinObj.UseTranslation && join.JoinObj.TranslationDbObject != null)
        {
            query += $"""
            OUTER APPLY (SELECT TOP(1) [T].* FROM {join.JoinObj.TranslationDbObject.GetSqlDefinition(includeAlias: false)} {asOfCommand} AS [T] WHERE [T].[Id] = [{join.Alias}].[Id] AND [T].[Language] = @Language) AS [{join.Alias}{join.JoinObj.TranslationDbObject.Alias}]
            """;
        }

        return (Query: query, Parameters: join.Filter);
    }

    /// <summary>
    /// Generate mssql columns
    /// </summary>
    /// <param name="join">Join</param>
    /// <param name="options">RequestOptions</param>
    /// <returns></returns>
    public string GenerateJoinColumns(Join join, RequestOptions options)
    {
        bool useTranslation = !string.IsNullOrEmpty(options.Language);
        var columns = new List<string>();
        foreach (var p in join.JoinObj.BaseDbObject.Type.GetProperties())
        {
            if (join.JoinObj.TranslationDbObject != null && join.JoinObj.UseTranslation && useTranslation && p.PropertyType == typeof(string))
            {
                columns.Add($"ISNULL([{join.Alias}{join.JoinObj.TranslationDbObject.Alias}].[{p.Name}],[{join.Alias}].[{p.Name}]) AS [{join.Alias}.{p.Name}]");
            }
            else
            {
                columns.Add($"[{join.Alias}].[{p.Name}] AS [{join.Alias}.{p.Name}]");
            }
        }

        return string.Join(',', columns);
    }
}
