using System.Data;
using Altinn.AccessMgmt.DbAccess.Contracts;
using Npgsql;

namespace Altinn.AccessMgmt.DbAccess.Helpers;

/// <summary>
/// Responsible for executing SQL commands and queries.
/// </summary>
public class DbExecutor(NpgsqlDataSource connection, IDbConverter dbConverter)
{
    private readonly NpgsqlDataSource _connection = connection;
    private readonly IDbConverter _dbConverter = dbConverter;

    /// <summary>
    /// Executes a non-query command (INSERT, UPDATE, DELETE) and returns the number of affected rows.
    /// </summary>
    public async Task<int> ExecuteCommand(string query, List<NpgsqlParameter> parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var cmd = _connection.CreateCommand(query);
            cmd.Parameters.AddRange(parameters.ToArray());
            return await cmd.ExecuteNonQueryAsync(cancellationToken: cancellationToken);
        }
        catch
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(query);
            Console.ForegroundColor = ConsoleColor.White;
            throw;
        }
    }

    /// <summary>
    /// Executes a query and maps the result to objects of type T.
    /// </summary>
    public async Task<IEnumerable<T>> ExecuteQuery<T>(string query, Dictionary<string, object> parameters, CancellationToken cancellationToken = default) where T : new()
    {
        await using var cmd = _connection.CreateCommand(query);
        var param = new List<NpgsqlParameter>();

        if (parameters != null)
        {
            foreach (var p in parameters)
            {
                param.Add(new NpgsqlParameter(p.Key, p.Value));
            }
        }

        return await ExecuteQuery<T>(query, param, cancellationToken);
    }

    /// <summary>
    /// Executes a query and maps the result to objects of type T.
    /// </summary>
    public async Task<IEnumerable<T>> ExecuteQuery<T>(string query, List<NpgsqlParameter> parameters, CancellationToken cancellationToken = default) where T : new()
    {
        await using var cmd = _connection.CreateCommand(query);
        cmd.Parameters.AddRange(parameters.ToArray());
        return _dbConverter.ConvertToObjects<T>(await cmd.ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken));
    }
}

