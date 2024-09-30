using Altinn.Authorization.Configuration.Postgres.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.Authorization.Configuration.Postgres.Connection;

/// <summary>
/// Factory for creating PostgreSQL connection pools using username and password authentication.
/// This factory uses options provided through <see cref="AltinnPostgresOptions"/> to configure 
/// the connection pool, such as the host, database, username, password, and other connection parameters.
/// </summary>
/// <inheritdoc/>
public partial class UsernameAndPasswordConnectionFactory(ILogger<UsernameAndPasswordConnectionFactory> logger, IOptions<AltinnPostgresOptions> options) : ConnectionPoolFactory
{
    /// <summary>
    /// Gets the configuration options for PostgreSQL connection from <see cref="AltinnPostgresOptions"/>.
    /// These options include the database credentials, host, database name, and other connection settings.
    /// </summary>
    private IOptions<AltinnPostgresOptions> Options { get; } = options;

    /// <summary>
    /// Gets the logger instance used to log information and errors related to connection pool creation.
    /// </summary>
    private ILogger<UsernameAndPasswordConnectionFactory> Logger { get; } = logger;

    /// <summary>
    /// Creates and returns a new PostgreSQL connection pool if it doesn't already exist. 
    /// Uses the provided connection options, including host, username, and password, to configure the connection.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the task.</param>
    /// <returns>A task that represents the asynchronous operation, containing the <see cref="NpgsqlDataSource"/> representing the connection pool.</returns>
    /// <exception cref="Exception">Throws an exception if the connection pool fails to be created.</exception>
    public override Task<NpgsqlDataSource> Create(CancellationToken cancellationToken = default)
    {
        Semaphore.WaitAsync(cancellationToken);
        try
        {
            if (ConnectionPool == null)
            {
                var options = Options.Value;
                
                Log.LogNewConnectionPool(Logger, options.Host);

                var builder = new NpgsqlConnectionStringBuilder()
                {
                    Host = options.Host,
                    Database = options.Database,
                    Username = options.Username,
                    Password = options.Password,
                    SslMode = SslMode.Prefer,
                    AutoPrepareMinUsages = options.AutoPrepareMinUsages,
                    MaxAutoPrepare = options.MaxAutoPrepare,
                };

                ConnectionPool = NpgsqlDataSource.Create(builder.ToString());
            }
        }
        catch (Exception ex)
        {
            var options = Options.Value;
            Log.LogFailedConnectionPool(Logger, ex, options.Host);
            throw;
        }
        finally
        {
            Semaphore.Release();
        }

        return Task.FromResult(ConnectionPool);
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Creating new connection pool for host {host} using username and password")]
        internal static partial void LogNewConnectionPool(ILogger logger, string host);
        
        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to create connection pool for host {host} using username and password")]
        internal static partial void LogFailedConnectionPool(ILogger logger, Exception ex, string host);
    }
}
