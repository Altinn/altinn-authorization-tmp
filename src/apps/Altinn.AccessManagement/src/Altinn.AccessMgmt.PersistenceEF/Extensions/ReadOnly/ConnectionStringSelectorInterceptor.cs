using Altinn.AccessMgmt.PersistenceEF.Extensions.Hint;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.Common;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions.ReadOnly;

public sealed class ConnectionStringSelectorInterceptor : DbConnectionInterceptor
{
    private readonly IConnectionStringSelector selector;
    private readonly ILogger<ConnectionStringSelectorInterceptor> logger;

    public ConnectionStringSelectorInterceptor(IConnectionStringSelector selector, ILogger<ConnectionStringSelectorInterceptor> logger)
    {
        this.selector = selector;
        this.logger = logger;
    }

    public override InterceptionResult ConnectionOpening(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result)
    {
        TrySwitchConnection(connection);
        return base.ConnectionOpening(connection, eventData, result);
    }

    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        TrySwitchConnection(connection);
        return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
    }

    private void TrySwitchConnection(DbConnection connection)
    {
        if (connection is not NpgsqlConnection)
        {
            return;
        }

        var target = selector.GetConnectionString();

        if (string.Equals(connection.ConnectionString, target, StringComparison.Ordinal))
        {
            return;
        }

        logger.LogDebug("Switching connection to read-only replica.");

        connection.ConnectionString = target;
    }
}
