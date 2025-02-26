using System.Data;
using System.Runtime.CompilerServices;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Utilities;
using Altinn.Authorization.Host.Database;
using Npgsql;

namespace Altinn.AccessMgmt.Persistence.Core.Executors;

/// <summary>
/// Responsible for executing SQL commands and queries.
/// </summary>
public class PostgresDbExecutor(IAltinnDatabase databaseFactory, NpgsqlDataSource connection, IDbConverter dbConverter) : IDbExecutor
{
    private readonly IAltinnDatabase _databaseFactory = databaseFactory;
    private readonly NpgsqlDataSource _connection = connection;
    private readonly IDbConverter _dbConverter = dbConverter;

    /// <summary>
    /// Executes a non-query command (INSERT, UPDATE, DELETE) and returns the number of affected rows.
    /// </summary>
    public async Task<int> ExecuteMigrationCommand(string query, List<GenericParameter> parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var cmd = _databaseFactory.CreatePgsqlConnection(SourceType.Migration).CreateCommand();
            cmd.CommandText = query;
            foreach (var parameter in parameters)
            {
                cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }

            return await cmd.ExecuteNonQueryAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(query);
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.White;
            throw;
        }
    }

    /// <summary>
    /// Executes a non-query command (INSERT, UPDATE, DELETE) and returns the number of affected rows.
    /// </summary>
    public async Task<int> ExecuteCommand(string query, List<GenericParameter> parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var cmd = _connection.CreateCommand(query);
            foreach (var parameter in parameters)
            {
                cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }

            return await cmd.ExecuteNonQueryAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(query);
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.White;
            throw;
        }
    }

    /// <summary>
    /// Executes a non-query command (INSERT, UPDATE, DELETE) and returns the number of affected rows.
    /// </summary>
    public async Task<int> ExecuteCommand(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var cmd = _connection.CreateCommand(query);
            return await cmd.ExecuteNonQueryAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(query);
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.White;
            throw;
        }
    }

    /// <summary>
    /// Executes a query and maps the result to objects of type T.
    /// </summary>
    public async Task<IEnumerable<T>> ExecuteQuery<T>(string query, List<GenericParameter> parameters, CancellationToken cancellationToken = default)
        where T : new()
    {
        await using var cmd = _connection.CreateCommand(query);
        foreach (var parameter in parameters)
        {
            cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
        }
        return _dbConverter.ConvertToObjects<T>(await cmd.ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken));
    }

    /// <summary>
    /// Executes a query and maps the result to objects of type T.
    /// </summary>
    public async Task<IEnumerable<T>> ExecuteQuery<T>(string query, CancellationToken cancellationToken = default)
        where T : new()
    {
        await using var cmd = _connection.CreateCommand(query);
        return _dbConverter.ConvertToObjects<T>(await cmd.ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken));
    }
}
