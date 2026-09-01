using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Queries;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc />
public class ActivityLogService(ActivityLogQuery activityLogQuery) : IActivityLogService
{
    /// <inheritdoc />
    public async Task<ActivityLogPage> GetActivityLog(Guid party, ActivityLogQueryFilter filter, int pageSize, ActivityLogQueryCursor cursor, CancellationToken cancellationToken = default)
    {
        if (party == Guid.Empty)
        {
            throw new ArgumentException("Party must be a non-empty guid.", nameof(party));
        }

        var anchoredFilter = (filter ?? new ActivityLogQueryFilter()) with { InvolvedIds = [party] };

        var page = await activityLogQuery.GetAsync(anchoredFilter, pageSize, cursor, cancellationToken);

        var items = page.Items.Select(DtoMapper.Convert).ToList();
        return new ActivityLogPage(items, ActivityLogTokens.Encode(page.Next));
    }
}
