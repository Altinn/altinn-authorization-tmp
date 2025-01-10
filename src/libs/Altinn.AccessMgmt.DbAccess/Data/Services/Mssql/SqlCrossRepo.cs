using System.Text;
using System.Text.Json;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Models;
using Microsoft.Extensions.Configuration;

namespace Altinn.AccessMgmt.DbAccess.Data.Services.Mssql;

/// <inheritdoc/>
public class SqlCrossRepo<TA, T, TB> : SqlBasicRepo<T>, IDbCrossRepo<TA, T, TB>
    where T : class
    where TA : class
    where TB : class
{
    private string XAColumn { get; set; } = typeof(TA).Name + "Id";
    private string XBColumn { get; set; } = typeof(TB).Name + "Id";

    private ObjectDefinition ObjDefA { get; set; }
    private ObjectDefinition ObjDefB { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlCrossRepo{A, T, B}"/> class.
    /// </summary>
    /// <param name="config">IConfiguration</param>
    public SqlCrossRepo(IConfiguration config) : base(config)
    {
        ObjDefA = DbDefinitions.Get<TA>() ?? throw new Exception($"Definition for '{typeof(TA).Name}' not found");
        ObjDefB = DbDefinitions.Get<TB>() ?? throw new Exception($"Definition for '{typeof(TB).Name}' not found");
    }

    /// <inheritdoc/>
    public void SetCrossColumns(string xAColumn, string xBColumn)
    {
        XAColumn = xAColumn;
        XBColumn = xBColumn;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TA>> ExecuteForA(Guid BId, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new RequestOptions();
        var query = GenerateQuery<TA>(BId, XAColumn, XBColumn, options);
        try
        {
            string jsonResult = await ExecuteForJson(query.Query, query.Parameters, singleResult: false, cancellationToken: cancellationToken);
            if (string.IsNullOrEmpty(jsonResult))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<IEnumerable<TA>>(jsonResult) ?? throw new Exception("Unable to deserialize data");
            }
            catch (Exception ex)
            {
                Console.WriteLine(query);
                Console.WriteLine(ex.Message);
                Console.WriteLine(jsonResult);
                throw;
            }
        }
        catch
        {
            Console.WriteLine(query);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TB>> ExecuteForB(Guid AId, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= new RequestOptions();
        var query = GenerateQuery<TB>(AId, XBColumn, XAColumn, options);
        try
        {
            string jsonResult = await ExecuteForJson(query.Query, query.Parameters, singleResult: false, cancellationToken: cancellationToken);
            if (string.IsNullOrEmpty(jsonResult))
            {
                return [];
            }

            try
            {
                return JsonSerializer.Deserialize<IEnumerable<TB>>(jsonResult) ?? throw new Exception("Unable to deserialize data");
            }
            catch (Exception ex)
            {
                Console.WriteLine(query);
                Console.WriteLine(ex.Message);
                Console.WriteLine(jsonResult);
                throw;
            }
        }
        catch
        {
            Console.WriteLine(query);
            throw;
        }
    }

    private (string Query, List<GenericFilter> Parameters) GenerateQuery<TResult>(Guid id, string sourceColumn, string filterColumn, RequestOptions? options = null)
    {
        options ??= new RequestOptions();
        bool useTranslation = !string.IsNullOrEmpty(options.Language);
        var param = new List<GenericFilter>
        {
            new GenericFilter("Id", id)
        };

        var baseObjDef = DbDefinitions.Get(typeof(T)) ?? throw new Exception($"Definition for '{typeof(T).Name}' not found");
        baseObjDef.BaseDbObject.Alias = "X";

        var objDef = DbDefinitions.Get(typeof(TResult)) ?? throw new Exception($"Definition for '{typeof(TResult).Name}' not found");
        objDef.BaseDbObject.Alias = "TResult";

        StringBuilder sb = new StringBuilder();
        sb.Append("SELECT ");
        sb.AppendLine(GenerateColumns(objDef, options));
        sb.AppendLine($"FROM {DbObjDef.BaseDbObject.GetSqlDefinition(includeAlias: false)} AS [TResult]");
        sb.AppendLine($"INNER JOIN {objDef.BaseDbObject.GetSqlDefinition(includeAlias: false)} AS [{objDef.BaseDbObject.Alias}] ON [TResult].[{sourceColumn}] = [{objDef.BaseDbObject.Alias}].[Id]");

        if (useTranslation)
        {
            sb.AppendLine($"OUTER APPLY (SELECT [Trans].* FROM {objDef.TranslationDbObject?.GetSqlDefinition(includeAlias: false)} AS [Trans] WHERE [Trans].[Id] = [{objDef.BaseDbObject.Alias}].[Id] AND [Trans].[Language] = @Language) AS [{objDef.TranslationDbObject?.Alias}]");
        }

        sb.AppendLine($"WHERE [TResult].[{filterColumn}] = @Id");
        string query = sb.ToString();
        if (useTranslation)
        {
            param.Add(new GenericFilter("Language", options.Language));
        }

        if (options.AsOf.HasValue)
        {
            param.Add(new GenericFilter("_AsOf", options.AsOf.Value));
        }

        return (query, param);
    }

    private async Task<IEnumerable<TResult>> Get<TResult>(Guid id, ObjectDefinition joinDef, string sourceColumn, string filterColumn, RequestOptions? options = null)
    {
        options ??= new RequestOptions();
        bool useTranslation = !string.IsNullOrEmpty(options.Language);
        var param = new List<GenericFilter>
        {
            new GenericFilter("Id", id)
        };

        StringBuilder sb = new StringBuilder();
        sb.Append("SELECT ");
        sb.AppendLine(GenerateColumns(joinDef, options));
        sb.AppendLine($"FROM {DbObjDef.BaseDbObject.GetSqlDefinition(includeAlias: false)} AS [TResult]");
        sb.AppendLine($"INNER JOIN {joinDef.BaseDbObject.GetSqlDefinition(includeAlias: false)} AS [{joinDef.BaseDbObject.Alias}] ON [TResult].[{sourceColumn}] = [{joinDef.BaseDbObject.Alias}].[Id]");

        if (useTranslation)
        {
            sb.AppendLine($"OUTER APPLY (SELECT [Trans].* FROM {joinDef.TranslationDbObject?.GetSqlDefinition(includeAlias: false)} AS [Trans] WHERE [Trans].[Id] = [{joinDef.BaseDbObject.Alias}].[Id] AND [Trans].[Language] = @Language) AS [{joinDef.TranslationDbObject?.Alias}]");
        }

        sb.AppendLine($"WHERE [TResult].[{filterColumn}] = @Id");
        string query = sb.ToString();
        if (useTranslation)
        {
            param.Add(new GenericFilter("Language", options.Language));
        }

        if (options.AsOf.HasValue)
        {
            param.Add(new GenericFilter("_AsOf", options.AsOf.Value));
        }

        string jsonResult = await ExecuteForJson(query, [.. param], singleResult: false);

        if (string.IsNullOrEmpty(jsonResult))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<IEnumerable<TResult>>(jsonResult) ?? throw new Exception("Unable to deserialize data");
        }
        catch (Exception ex)
        {
            Console.WriteLine(query);
            Console.WriteLine(ex.Message);
            Console.WriteLine(jsonResult);
            throw;
        }
    }
}
