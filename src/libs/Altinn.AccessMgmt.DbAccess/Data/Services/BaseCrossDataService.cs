using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Models;

namespace Altinn.AccessMgmt.DbAccess.Data.Services;

/// <inheritdoc/>
public class BaseCrossDataService<TA, T, TB> : BaseDataService<T>, IDbCrossDataService<TA, T, TB>
{
    /// <summary>
    /// Cross repo
    /// </summary>
    public IDbCrossRepo<TA, T, TB> CrossRepo { get; }

    /// <summary>
    /// Base data service
    /// </summary>
    /// <param name="repo">ExtendedRepo</param>
    public BaseCrossDataService(IDbCrossRepo<TA, T, TB> repo) : base(repo)
    {
        CrossRepo = repo;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TA>> GetA(Guid bId, RequestOptions? options = null)
    {
        return await CrossRepo.ExecuteForA(bId, options);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TB>> GetB(Guid aId, RequestOptions? options = null)
    {
        return await CrossRepo.ExecuteForB(aId, options);
    }
}
