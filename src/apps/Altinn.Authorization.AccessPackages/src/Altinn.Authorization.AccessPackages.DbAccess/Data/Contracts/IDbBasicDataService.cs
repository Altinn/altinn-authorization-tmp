using System.Linq.Expressions;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;

/// <summary>
/// Basic data service
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDbBasicDataService<T>
{
    /// <summary>
    /// Data repository
    /// </summary>
    IDbBasicRepo<T> Repo { get; }

    /// <summary>
    /// Get all entities
    /// </summary>
    /// <param name="options">RequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>T<see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<IEnumerable<T>> Get(RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get single entity based on identity
    /// </summary>
    /// <param name="id">Identity</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>T<see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<T?> Get(Guid id, RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Entities based on property and value
    /// </summary>
    /// <param name="property">Filter property</param>
    /// <param name="value">Filter value</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>T<see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<IEnumerable<T>> Get<TProperty>(Expression<Func<T, TProperty>> property, TProperty value, RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Entities based on multiple property and value pairs
    /// </summary>
    /// <param name="filters">GenericFilter</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task<IEnumerable<T>> Get(IEnumerable<GenericFilter> filters, RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Entities based on filters
    /// </summary>
    /// <param name="parameters">Filters</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>T<see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<IEnumerable<T>> Get(Dictionary<string, object> parameters, RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for entities
    /// </summary>
    /// <param name="term">Term to filter on</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>T<see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<IEnumerable<T>> Search(string term, RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for entities
    /// </summary>
    /// <param name="term">Term to filter on</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>T<see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<(IEnumerable<T> Data, PagedResult PageInfo)> SearchPaged(string term, RequestOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create entity
    /// </summary>
    /// <param name="entity">Entity to create</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<int> Create(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create entities
    /// </summary>
    /// <param name="entities">Entities to create</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<int> Create(List<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update entity
    /// </summary>
    /// <param name="id">Identity</param>
    /// <param name="entity">Entity to update</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<int> Update(Guid id, T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update single property on entity
    /// </summary>
    /// <param name="id">Identity</param>
    /// <param name="property">Property to update</param>
    /// <param name="value">Updated value</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<int> Update(Guid id, string property, string value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update single property on entity
    /// </summary>
    /// <param name="id">Identity</param>
    /// <param name="property">Property to update</param>
    /// <param name="value">Updated value</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<int> Update(Guid id, string property, int value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update single property on entity
    /// </summary>
    /// <param name="id">Identity</param>
    /// <param name="property">Property to update</param>
    /// <param name="value">Updated value</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<int> Update(Guid id, string property, Guid value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete entity
    /// </summary>
    /// <param name="id">Identity</param>
    /// <param name="cascade">Cascading delete</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<int> Delete(Guid id, CancellationToken cancellationToken = default);
}
