using System.Text;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Services.Postgres;

/// <inheritdoc/>
public class PostgresCrossRepo<TA, T, TB> : PostgresBasicRepo<T>, IDbCrossRepo<TA, T, TB>
    where TA : class
    where T : class
    where TB : class
{
    private string XAColumn { get; set; }
    private string XBColumn { get; set; }

    private readonly IDbCrossConverter<TA, T, TB> dbCrossConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresCrossRepo{A, T, B}"/> class.
    /// </summary>
    /// <param name="config">IConfiguration</param>
    /// <param name="dbConverter">IDbBasicConverter</param>
    public PostgresCrossRepo(IConfiguration config, IDbCrossConverter<TA, T, TB> dbConverter) : base(config, dbConverter)
    {
        dbCrossConverter = dbConverter;
        XAColumn = typeof(TA).Name + "Id";
        XBColumn = typeof(TB).Name + "Id";
    }

    /// <summary>
    /// Override columnnames
    /// </summary>
    /// <param name="xAColumn">Ref column for table A</param>
    /// <param name="xBColumn">Ref column for table B</param>
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
            using var connection = new NpgsqlConnection(ConnectionString);
            CommandDefinition cmd = new CommandDefinition(query.Query, query.Parameters, cancellationToken: cancellationToken);
            return dbCrossConverter.ConvertA(await connection.ExecuteReaderAsync(cmd));
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
            using var connection = new NpgsqlConnection(ConnectionString);
            CommandDefinition cmd = new CommandDefinition(query.Query, query.Parameters, cancellationToken: cancellationToken);
            return dbCrossConverter.ConvertB(await connection.ExecuteReaderAsync(cmd));
        }
        catch
        {
            Console.WriteLine(query);
            throw;
        }
    }
    
    private (string Query, Dictionary<string, object> Parameters) GenerateQuery<TResult>(Guid id, string sourceColumn, string filterColumn, RequestOptions? options = null)
    {
        options ??= new RequestOptions();
        var param = new Dictionary<string, object>();
        param.Add("Id", id);

        var baseObjDef = DbDefinitions.Get(typeof(T)) ?? throw new Exception($"Definition for '{typeof(T).Name}' not found");
        baseObjDef.BaseDbObject.Alias = "X";

        var objDef = DbDefinitions.Get(typeof(TResult)) ?? throw new Exception($"Definition for '{typeof(TResult).Name}' not found");
        objDef.BaseDbObject.Alias = "TResult";

        StringBuilder sb = new StringBuilder();
        sb.Append("SELECT ");
        sb.AppendLine(GenerateColumns(objDef, options));
        sb.AppendLine($"FROM {GenerateSource(baseObjDef, options)}");
        sb.AppendLine($"INNER JOIN {objDef.BaseDbObject.GetPostgresDefinition(includeAlias: false)} AS TResult ON X.{sourceColumn} = TResult.Id");
        if (!string.IsNullOrEmpty(options.Language) && objDef.UseTranslation)
        {
            sb.AppendLine($"LEFT JOIN LATERAL (select * from {objDef.TranslationDbObject?.GetPostgresDefinition(includeAlias: false, useAsOf: options.AsOf.HasValue)} as t where t.Id = {objDef.BaseDbObject.Alias}.Id AND t.Language = @Language) as {objDef.TranslationDbObject?.Alias} on 1=1");
        }

        sb.AppendLine($"WHERE X.{filterColumn} = @Id");

        string query = sb.ToString();

        if (options.Language != null)
        {
            param.Add("Language", options.Language);
        }

        if (options.AsOf.HasValue)
        {
            param.Add("_AsOf", options.AsOf.Value);
        }

        return (query, param);
    }
}
