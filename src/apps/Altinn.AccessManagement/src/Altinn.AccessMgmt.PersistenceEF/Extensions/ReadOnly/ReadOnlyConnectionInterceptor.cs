using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.Common;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions.ReadOnly;

public sealed class ReadOnlyConnectionInterceptor : DbConnectionInterceptor
{
    private readonly IReadOnlySelector selector;
    private readonly ILogger<ReadOnlyConnectionInterceptor> logger;

    public ReadOnlyConnectionInterceptor(IReadOnlySelector selector, ILogger<ReadOnlyConnectionInterceptor> logger)
    {
        this.selector = selector;
        this.logger = logger;
    }

    public override InterceptionResult ConnectionOpening(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result)
    {
        TrySwitchToReadReplica(connection);
        return base.ConnectionOpening(connection, eventData, result);
    }

    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        TrySwitchToReadReplica(connection);
        return await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
    }

    private void TrySwitchToReadReplica(DbConnection connection)
    {
        if (connection is not NpgsqlConnection)
        {
            return;
        }

        // Hent ønsket readonly-connectionstring fra selector
        var target = selector.GetConnectionString();

        // Unngå unødvendige endringer
        if (string.Equals(connection.ConnectionString, target, StringComparison.Ordinal))
        {
            return;
        }

        logger.LogDebug("Switching connection to read-only replica.");

        connection.ConnectionString = target;
    }
}

public sealed class ReadOnlySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IReadOnlyHintService hintService;

    public ReadOnlySaveChangesInterceptor(IReadOnlyHintService hintService)
    {
        this.hintService = hintService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        EnforceReadOnlyIfNeeded(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        EnforceReadOnlyIfNeeded(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void EnforceReadOnlyIfNeeded(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        // Ingen hint => ikke readonly => la SaveChanges gå som normalt
        var hint = hintService.GetHint();
        if (string.IsNullOrWhiteSpace(hint))
        {
            return;
        }

        // Vi er i "read-only mode": blokker hvis det er modifikasjoner
        var hasModifications = context.ChangeTracker.Entries()
            .Any(e => e.State == EntityState.Added
                   || e.State == EntityState.Modified
                   || e.State == EntityState.Deleted);

        if (!hasModifications)
        {
            return;
        }

        throw new DbUpdateException("Writing is disabled in read-only scope.");
    }
}
