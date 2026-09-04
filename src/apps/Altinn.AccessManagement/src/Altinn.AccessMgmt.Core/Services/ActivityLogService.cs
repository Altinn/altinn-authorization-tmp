using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Queries;
using Altinn.Authorization.Api.Contracts.AccessManagement.ActivityLog;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc />
public class ActivityLogService(ActivityLogQuery activityLogQuery) : IActivityLogService
{
    /// <inheritdoc />
    public async Task<ActivityLogPage> GetActivityLog(Guid party, ActivityLogDirection? direction, ActivityLogQueryFilter filter, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        if (party == Guid.Empty)
        {
            throw new ArgumentException("Party must be a non-empty guid.", nameof(party));
        }

        var baseFilter = filter ?? new ActivityLogQueryFilter();
        var anchoredFilter = direction switch
        {
            ActivityLogDirection.From => baseFilter with { FromIds = [party], InvolvedIds = null },
            ActivityLogDirection.To => baseFilter with { ToIds = [party], InvolvedIds = null },
            ActivityLogDirection.Via => baseFilter with { ViaIds = [party], InvolvedIds = null },
            _ => baseFilter with { InvolvedIds = [party] },
        };

        var page = await activityLogQuery.GetAsync(anchoredFilter, pageSize, pageNumber, cancellationToken);

        var items = page.Items.Select(DtoMapper.Convert).ToList();
        return new ActivityLogPage(items, page.HasMore);
    }
}
