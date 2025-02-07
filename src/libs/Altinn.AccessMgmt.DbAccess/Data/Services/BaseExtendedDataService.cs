using System.Linq.Expressions;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Models;

namespace Altinn.AccessMgmt.DbAccess.Data.Services;

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
        //// using var a = DbAccessTelemetry.StartActivity<T>("GetExtended");
        var result = await ExtendedRepo.GetExtended([new GenericFilter("Id", id)], options, cancellationToken);
        if (result != null && result.Any())
        {
            return result.First();
        }

        throw new Exception("Not found");
    }

    public async Task<IEnumerable<TExtended>> GetExtended(
   IEnumerable<GenericFilter> filters,
   RequestOptions? options = null,
   CancellationToken cancellationToken = default)
    {
        //// using var a = DbAccessTelemetry.StartActivity<T>("GetExtended");
        return await ExtendedRepo.GetExtended(filters.ToList(), options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public GenericFilterBuilder<TExtended> CreateFilterBuilder<TExtended>() { return new GenericFilterBuilder<TExtended>(); }

    public async Task<IEnumerable<TExtended>> GetExtended(GenericFilterBuilder<TExtended> filterBuilder, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        //// using var a = DbAccessTelemetry.StartActivity<T>("GetExtended");
        return await ExtendedRepo.GetExtended(filterBuilder.ToList(), options, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<TExtended>> GetExtended(
    Func<GenericFilterBuilder<TExtended>, GenericFilterBuilder<TExtended>> configureFilters,
    RequestOptions? options = null,
    CancellationToken cancellationToken = default)
    {
        //// using var a = DbAccessTelemetry.StartActivity<T>("GetExtended");

        var filterBuilder = CreateFilterBuilder<TExtended>();
        var filters = configureFilters(filterBuilder).ToList();

        return await ExtendedRepo.GetExtended(filters, options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TExtended>> GetExtended<TProperty>(Expression<Func<TExtended, TProperty>> property, TProperty value, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        //// using var a = DbAccessTelemetry.StartActivity<T>("GetExtended");
        string propertyName = ExtractPropertyInfo(property).Name;
        var filters = new List<GenericFilter>
        {
            new GenericFilter(propertyName, value, FilterComparer.Equals)
        };
        return await ExtendedRepo.GetExtended(filters, options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TExtended>> GetExtended2<TProperty>(IEnumerable<Expression<Func<TExtended, TProperty>>> properties, TProperty value, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        //// using var a = DbAccessTelemetry.StartActivity<T>("GetExtended");
        var filters = new List<GenericFilter>();

        foreach (var property in properties)
        {
            string propertyName = ExtractPropertyInfo(property).Name;
            new GenericFilter(propertyName, value, FilterComparer.Equals);
        }
        
        return await ExtendedRepo.GetExtended(filters, options, cancellationToken: cancellationToken);
    }



    /// <inheritdoc/>
    public async Task<IEnumerable<TExtended>> GetExtended(RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        //// using var a = DbAccessTelemetry.StartActivity<T>("GetExtended");
        return await ExtendedRepo.GetExtended(filters: [], options: options, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IEnumerable<TExtended> Data, PagedResult PageInfo)> SearchExtended(string term, RequestOptions? options = null, bool startsWith = false, CancellationToken cancellationToken = default)
    {
        //// using var a = DbAccessTelemetry.StartActivity<T>("SearchExtended");
        return await ExtendedRepo.SearchExtended(term, options ?? new RequestOptions(), startsWith, cancellationToken);
    }
}
