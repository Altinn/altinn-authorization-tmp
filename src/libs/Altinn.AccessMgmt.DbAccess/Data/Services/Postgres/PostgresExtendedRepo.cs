using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Models;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessMgmt.DbAccess.Data.Services.Postgres;

/// <inheritdoc/>
public class PostgresExtendedRepo<T, TExtended> : PostgresBasicRepo<T>, IDbExtendedRepo<T, TExtended>
    where T : class, new()
    where TExtended : class, new()
{
    private List<Join> Joins { get; set; } = new List<Join>();

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresExtendedRepo{T, E}"/> class.
    /// </summary>
    /// <param name="config">IConfiguration</param>
    /// <param name="dataMapper">DbConverter</param>
    public PostgresExtendedRepo(IOptions<DbAccessDataConfig> config, DbConverter dataMapper) : base(config, dataMapper) { }

    /// <inheritdoc/>
    public async Task<(IEnumerable<TExtended> Data, PagedResult PageInfo)> SearchExtended(string term, RequestOptions? options = null, bool startsWith = false, CancellationToken cancellationToken = default)
    {
        var data = await GetExtended([new GenericFilter("Name", term, comparer: startsWith ? FilterComparer.StartsWith : FilterComparer.Contains)], options);
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
    public void Join<TJoin>(Expression<Func<T, object?>> TProperty, Expression<Func<TJoin, object>> TJoinProperty, Expression<Func<TExtended, object?>> TExtendedProperty, bool optional = false, bool isList = false)
    {
        Joins.Add(new Join()
        {
            Alias = ExtractPropertyInfo(TExtendedProperty as Expression<Func<TExtended, object>>).Name,
            BaseObj = DbDefinitions.Get<T>() ?? throw new Exception($"Definition for '{typeof(T).Name}' not found"),
            BaseJoinProperty = ExtractPropertyInfo(TProperty as Expression<Func<T, object>>).Name,
            JoinObj = DbDefinitions.Get<TJoin>() ?? DbDefinitions.GetExtended<TJoin>() ?? throw new Exception($"Definition for '{typeof(TJoin).Name}' not found"),
            JoinProperty = ExtractPropertyInfo(TJoinProperty).Name,
            Optional = optional,
            IsList = isList
        });

        PropertyInfo ExtractPropertyInfo<TLocal>(Expression<Func<TLocal, object>> expression)
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
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TExtended>> GetExtended(List<GenericFilter>? filters = null, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var a = DbAccessTelemetry.StartActivity<T>("GetExtended");
        options ??= new RequestOptions();
        filters ??= [];
        var cmd = GetCommand(options, filters);
        var param = PrepareParameters(filters, options);

        return await ExecuteExtended(cmd, param);
    }

    #region Internal
    private async Task<IEnumerable<TExtended>> ExecuteExtended(string query, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        using var a = DbAccessTelemetry.StartActivity<T>("ExecuteExtended");
        try
        {
            a?.AddEvent(new System.Diagnostics.ActivityEvent("Start"));
            using var connection = new NpgsqlConnection(ConnectionString);
            CommandDefinition cmd = new CommandDefinition(query, parameters, cancellationToken: cancellationToken);
            Console.WriteLine(query);
            return DbConverter.ConvertToObjects<TExtended>(await connection.ExecuteReaderAsync(cmd));
        }
        catch (Exception ex)
        {
            a?.SetStatus(System.Diagnostics.ActivityStatusCode.Error);
            Console.WriteLine(ex.Message);
            Console.WriteLine(query);
            throw;
        }
        finally
        {
            a?.SetStatus(System.Diagnostics.ActivityStatusCode.Ok);
            a?.Stop();
        }
    }

    private string GetCommand(RequestOptions? options = null, List<GenericFilter>? filters = null)
    {
        options ??= new RequestOptions();
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("SELECT ");
        sb.AppendLine(GenerateColumns(options));

        foreach (var j in Joins)
        {
            sb.Append(",");
            sb.AppendLine(GenerateJoinPostgresColumns(j, options));
        }

        sb.AppendLine("FROM " + GenerateSource(options));
        foreach (var j in Joins.Where(t => !t.IsList))
        {
            var joinStatement = GetJoinPostgresStatement(j, options);
            sb.AppendLine(joinStatement.Query);
        }

        sb.AppendLine(GenerateStatementFromFilters(DbObjDef.BaseDbObject.Alias, filters));

        var query = sb.ToString();
        query = AddPagingToQuery(query, options);

        return query;
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
            result += $" AND {join.BaseObj.BaseDbObject.Alias}.{filter.PropertyName} = _{join.Alias}.{filter.Value}";
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
        if (!join.IsList)
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
        else
        {
            return $"COALESCE((SELECT JSON_AGG(ROW_TO_JSON({join.JoinObj.BaseDbObject.Alias})) FROM {join.JoinObj.BaseDbObject.GetPostgresDefinition()} WHERE {join.JoinObj.BaseDbObject.Alias}.{join.JoinProperty} = {join.BaseObj.BaseDbObject.Alias}.{join.BaseJoinProperty}), '[]') AS {join.Alias}";
        }
    }
    #endregion
}
