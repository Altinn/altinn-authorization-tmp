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
    /// <param name="party">The party that must be involved in every entry. This is the authorization anchor.</param>
    /// <param name="direction">How the party anchors the entries: From (given), To (received) or Via (facilitator);
    /// <see langword="null"/> matches any involvement. The party overwrites the corresponding filter field.</param>
    /// <param name="filter">Additional narrowing filters; InvolvedIds and the anchored field are overwritten by <paramref name="party"/>.</param>
    /// <param name="pageSize">Maximum number of entries per page.</param>
    /// <param name="pageNumber">Zero-based page number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ActivityLogPage> GetActivityLog(Guid party, ActivityLogDirection? direction, ActivityLogQueryFilter filter, int pageSize, int pageNumber, CancellationToken cancellationToken = default);
}

/// <summary>
/// One page of activity log entries.
/// </summary>
public sealed record ActivityLogPage(IReadOnlyList<ActivityLogDto> Items, bool HasMore);
