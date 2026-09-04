using Altinn.AccessMgmt.FFB.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Queries;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.FFB.Services.PageData;

/// <summary>
/// Runs ActivityLogQuery against an environment for the activity log page, and provides the
/// small catalog searches (roles, packages, resources) its filter pickers need.
/// </summary>
public sealed class ActivityLogPageService(IEnvironmentDbContextFactory dbFactory)
{
    private const int MaxCatalogResults = 20;

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

    public async Task<List<CatalogItem>> SearchRolesAsync(string environment, string term, CancellationToken ct = default)
    {
        using var db = dbFactory.CreateContext(environment);
        var pattern = $"%{term}%";

        return await db.Roles.AsNoTracking()
            .Where(r => EF.Functions.ILike(r.Name, pattern) || EF.Functions.ILike(r.Code, pattern))
            .OrderBy(r => r.Name)
            .Take(MaxCatalogResults)
            .Select(r => new CatalogItem(r.Id, r.Name))
            .ToListAsync(ct);
    }

    public async Task<List<CatalogItem>> SearchPackagesAsync(string environment, string term, CancellationToken ct = default)
    {
        using var db = dbFactory.CreateContext(environment);
        var pattern = $"%{term}%";

        return await db.Packages.AsNoTracking()
            .Where(p => EF.Functions.ILike(p.Name, pattern) || EF.Functions.ILike(p.Urn, pattern))
            .OrderBy(p => p.Name)
            .Take(MaxCatalogResults)
            .Select(p => new CatalogItem(p.Id, p.Name))
            .ToListAsync(ct);
    }

    public async Task<List<CatalogItem>> SearchResourcesAsync(string environment, string term, CancellationToken ct = default)
    {
        using var db = dbFactory.CreateContext(environment);
        var pattern = $"%{term}%";

        return await db.Resources.AsNoTracking()
            .Where(r => EF.Functions.ILike(r.Name, pattern) || EF.Functions.ILike(r.RefId, pattern))
            .OrderBy(r => r.Name)
            .Take(MaxCatalogResults)
            .Select(r => new CatalogItem(r.Id, r.Name))
            .ToListAsync(ct);
    }
}

/// <summary>
/// One selectable catalog value (role, package or resource) for the filter pickers.
/// </summary>
public sealed record CatalogItem(Guid Id, string Name);
