using Altinn.AccessMgmt.PersistenceEF.Extensions.Hint;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions.ReadOnly;

public sealed class HintSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHintService hintService;

    public HintSaveChangesInterceptor(IHintService hintService)
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

        var hint = hintService.GetHint();
        if (hint is null || !hint.Value.StartsWith(HintKind.ConnectionReadOnly.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

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
