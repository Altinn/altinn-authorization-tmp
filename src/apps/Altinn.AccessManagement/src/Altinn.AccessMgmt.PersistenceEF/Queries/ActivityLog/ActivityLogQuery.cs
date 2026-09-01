using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Queries;

/// <summary>
/// Queries <c>dbo.activitylog</c> with multi-value filtering and keyset pagination
/// ordered by <c>("when", id)</c> descending.
/// </summary>
public sealed class ActivityLogQuery(AppDbContext db)
{
    /// <summary>
    /// Returns one page of activity log entries matching the filter, newest first.
    /// </summary>
    /// <param name="filter">The filter; at least one narrowing parameter must be set.</param>
    /// <param name="pageSize">Maximum number of entries to return.</param>
    /// <param name="cursor">Position of the last entry of the previous page, or <see langword="null"/> for the first page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<ActivityLogQueryPage> GetAsync(
        ActivityLogQueryFilter filter,
        int pageSize,
        ActivityLogQueryCursor cursor = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        filter.Validate();

        var query = BuildQuery(filter);

        if (cursor is not null)
        {
            var cursorWhen = cursor.When;
            var cursorId = cursor.Id;
            query = query.Where(t => EF.Functions.LessThan(
                ValueTuple.Create(t.When, t.Id),
                ValueTuple.Create(cursorWhen, cursorId)));
        }

        var items = await query
            .OrderByDescending(t => t.When)
            .ThenByDescending(t => t.Id)
            .Take(pageSize + 1)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        ActivityLogQueryCursor next = null;
        if (items.Count > pageSize)
        {
            items.RemoveAt(pageSize);
            var last = items[^1];
            next = new ActivityLogQueryCursor(last.When, last.Id);
        }

        return new ActivityLogQueryPage(items, next);
    }

    private IQueryable<ActivityLog> BuildQuery(ActivityLogQueryFilter filter)
    {
        return db.ActivityLogs
            .InvolvedIdContains(ToSet(filter.InvolvedIds))
            .TypeContains(ToSet(filter.Types))
            .SubtypeContains(ToSet(filter.Subtypes))
            .TriggerContains(ToSet(filter.Triggers))
            .StatusContains(ToSet(filter.Statuses))
            .ByIdContains(ToSet(filter.ByIds))
            .SourceIdContains(ToSet(filter.SourceIds))
            .OperationIdContains(ToSet(filter.OperationIds))
            .FromIdContains(ToSet(filter.FromIds))
            .ToIdContains(ToSet(filter.ToIds))
            .ViaIdContains(ToSet(filter.ViaIds))
            .RoleIdContains(ToSet(filter.RoleIds))
            .PackageIdContains(ToSet(filter.PackageIds))
            .ResourceIdContains(ToSet(filter.ResourceIds))
            .InstanceIdContains(ToSet(filter.InstanceIds))
            .ItemIdContains(ToSet(filter.ItemIds))
            .ParentIdContains(ToSet(filter.ParentIds))
            .WhereIf(filter.After.HasValue, t => t.When >= filter.After.Value)
            .WhereIf(filter.Before.HasValue, t => t.When < filter.Before.Value);
    }

    private static HashSet<T> ToSet<T>(IReadOnlyCollection<T> values)
        => values?.Count > 0 ? new HashSet<T>(values) : null;
}

/// <summary>
/// Keyset position of the last entry of a page, used to fetch the next page.
/// </summary>
public sealed record ActivityLogQueryCursor(DateTimeOffset When, Guid Id);

/// <summary>
/// One page of activity log entries and the cursor for the next page.
/// </summary>
public sealed class ActivityLogQueryPage(IReadOnlyList<ActivityLog> items, ActivityLogQueryCursor next)
{
    /// <summary>
    /// The entries, ordered newest first.
    /// </summary>
    public IReadOnlyList<ActivityLog> Items { get; } = items;

    /// <summary>
    /// Cursor for the next page, or <see langword="null"/> when there are no more entries.
    /// </summary>
    public ActivityLogQueryCursor Next { get; } = next;
}
