using Altinn.AccessMgmt.PersistenceEF.Queries;
using Altinn.Authorization.Api.Contracts.AccessManagement.ActivityLog;

namespace Altinn.AccessMgmt.Core.Services.Contracts;

/// <summary>
/// Read access to the activity log over assignments, delegations and requests.
/// </summary>
public interface IActivityLogService
{
    /// <summary>
    /// Returns one page of activity log entries involving the given party, newest first.
    /// </summary>
    /// <param name="party">The party that must be involved in every entry (as from, to or via). This is the authorization anchor.</param>
    /// <param name="filter">Additional narrowing filters; any InvolvedIds on it are overwritten by <paramref name="party"/>.</param>
    /// <param name="pageSize">Maximum number of entries to return.</param>
    /// <param name="cursor">Position of the last entry of the previous page, or <see langword="null"/> for the first page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ActivityLogPage> GetActivityLog(Guid party, ActivityLogQueryFilter filter, int pageSize, ActivityLogQueryCursor cursor, CancellationToken cancellationToken = default);
}

/// <summary>
/// One page of activity log entries and the opaque token for fetching the next page.
/// </summary>
public sealed record ActivityLogPage(IReadOnlyList<ActivityLogDto> Items, string NextToken);
