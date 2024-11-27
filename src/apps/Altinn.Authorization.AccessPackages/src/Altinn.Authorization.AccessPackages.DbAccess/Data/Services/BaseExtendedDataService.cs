using System.Linq.Expressions;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Services;

/// <inheritdoc/>
public class BaseExtendedDataService<T, TExtended> : BaseDataService<T>, IDbExtendedDataService<T, TExtended>
{
    /// <summary>
    /// Extended repo
    /// </summary>
    public IDbExtendedRepo<T, TExtended> ExtendedRepo { get; }

    /// <summary>
    /// Base data service
    /// </summary>
    /// <param name="repo">ExtendedRepo</param>
    public BaseExtendedDataService(IDbExtendedRepo<T, TExtended> repo) : base(repo)
    {
        ExtendedRepo = repo;
    }

    /// <inheritdoc/>
    public async Task<TExtended?> GetExtended(Guid id, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var a = DbAccess.DbAccessTelemetry.StartActivity<T>("GetExtended");
        var result = await ExtendedRepo.GetExtended([new GenericFilter("Id", id)], options, cancellationToken);
        if (result != null && result.Any())
        {
            return result.First();
        }

        throw new Exception("Not found");
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TExtended>> GetExtended<TProperty>(Expression<Func<TExtended, TProperty>> property, TProperty value, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var a = DbAccess.DbAccessTelemetry.StartActivity<T>("GetExtended");
        string propertyName = ExtractPropertyInfo(property).Name;
        var filters = new List<GenericFilter>
        {
            new GenericFilter(propertyName, value, FilterComparer.Equals)
        };
        return await ExtendedRepo.GetExtended(filters, options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TExtended>> GetExtended(RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        using var a = DbAccess.DbAccessTelemetry.StartActivity<T>("GetExtended");
        return await ExtendedRepo.GetExtended(filters: [], options: options, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IEnumerable<TExtended> Data, PagedResult PageInfo)> SearchExtended(string term, RequestOptions? options = null, bool startsWith = false, CancellationToken cancellationToken = default)
    {
        using var a = DbAccess.DbAccessTelemetry.StartActivity<T>("SearchExtended");
        return await ExtendedRepo.SearchExtended(term, options ?? new RequestOptions(), startsWith, cancellationToken);
    }
}
