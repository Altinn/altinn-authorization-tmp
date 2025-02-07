using System.Linq.Expressions;
using System.Reflection;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Models;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessMgmt.DbAccess.Data.Services;

/// <summary>
/// Logs
/// </summary>
public static class Logs
{
    /// <summary>
    /// LoggerFactory
    /// </summary>
    public static ILoggerFactory LoggerFactory { get; set; } = new LoggerFactory();
}

/// <inheritdoc/>
public class BaseDataService<T> : IDbBasicDataService<T>
{
    /// <summary>
    /// Extended repo
    /// </summary>
    public IDbBasicRepo<T> Repo { get; }

    /// <summary>
    /// Logger
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// Base data service
    /// </summary>
    /// <param name="repo">ExtendedRepo</param>
    public BaseDataService(IDbBasicRepo<T> repo)
    {
        Repo = repo;
        Logger = Logs.LoggerFactory.CreateLogger(typeof(BaseDataService<T>));
    }

    /// <inheritdoc/>
    public GenericFilterBuilder<T> CreateFilterBuilder<T>() { return new GenericFilterBuilder<T>(); }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get(RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await Repo.Get(filters: new List<GenericFilter>(), options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<T?> Get(Guid id, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var res = await Repo.Get([new GenericFilter("Id", id)], options, cancellationToken: cancellationToken);
            return res.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Console.WriteLine(id);
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get<TProperty>(Expression<Func<T, TProperty>> property, TProperty value, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        string propertyName = ExtractPropertyInfo(property).Name;
        var filters = new List<GenericFilter>
        {
            new GenericFilter(propertyName, value, FilterComparer.Equals)
        };
        return await Repo.Get(filters, options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get(IEnumerable<GenericFilter> filters, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await Repo.Get(filters.ToList(), options, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Extracts property
    /// </summary>
    /// <typeparam name="TLocal">Object</typeparam>
    /// <typeparam name="TProperty">Property</typeparam>
    /// <param name="expression">Expression</param>
    /// <returns></returns>
    protected PropertyInfo ExtractPropertyInfo<TLocal, TProperty>(Expression<Func<TLocal, TProperty>> expression)
    {
        MemberExpression? memberExpression = expression.Body switch
        {
            MemberExpression member => member,
            UnaryExpression { Operand: MemberExpression member } => member,
            _ => null
        };

        return memberExpression?.Member as PropertyInfo ?? throw new ArgumentException($"Expression '{expression}' does not refer to a valid property.");
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Get(Dictionary<string, object> parameters, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        var param = new List<GenericFilter>();
        foreach (var p in parameters)
        {
            param.Add(new GenericFilter(p.Key, p.Value));
        }

        return await Repo.Get(param, options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<T>> Search(string term, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await Repo.Get([new GenericFilter("Name", term, comparer: FilterComparer.Contains)], options, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public Task<(IEnumerable<T> Data, PagedResult PageInfo)> SearchPaged(string term, RequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        return Repo.Search(term, options ?? new RequestOptions(), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Create(List<T> entities, CancellationToken cancellationToken = default)
    {
        int count = 0;
        foreach (var entity in entities)
        {
            count += await Create(entity, cancellationToken: cancellationToken);
        }

        return count;
    }

    /// <inheritdoc/>
    public async Task<int> Create(T entity, CancellationToken cancellationToken = default)
    {
        return await Repo.Create(entity, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, T entity, CancellationToken cancellationToken = default)
    {
        return await Repo.Update(id, entity, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, string property, Guid value, CancellationToken cancellationToken = default)
    {
        return await Repo.Update(id, [new GenericParameter(property, value)], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, string property, string value, CancellationToken cancellationToken = default)
    {
        return await Repo.Update(id, [new GenericParameter(property, value)], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Update(Guid id, string property, int value, CancellationToken cancellationToken = default)
    {
        return await Repo.Update(id, [new GenericParameter(property, value)], cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        return await Repo.Delete(id, cancellationToken);
    }
}
