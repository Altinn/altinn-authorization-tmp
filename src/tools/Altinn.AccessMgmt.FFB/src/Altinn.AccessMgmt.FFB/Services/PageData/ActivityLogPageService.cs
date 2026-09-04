using Altinn.AccessMgmt.FFB.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Queries;

namespace Altinn.AccessMgmt.FFB.Services.PageData;

/// <summary>
/// Runs ActivityLogQuery against an environment for the activity log page.
/// </summary>
public sealed class ActivityLogPageService(IEnvironmentDbContextFactory dbFactory)
{
    public async Task<ActivityLogQueryPage> QueryAsync(
        string environment,
        ActivityLogQueryFilter filter,
        int pageSize,
        int pageNumber,
        CancellationToken ct = default)
    {
        using var db = dbFactory.CreateContext(environment);
        var query = new ActivityLogQuery(db);

        return await query.GetAsync(filter, pageSize, pageNumber, ct);
    }
}
