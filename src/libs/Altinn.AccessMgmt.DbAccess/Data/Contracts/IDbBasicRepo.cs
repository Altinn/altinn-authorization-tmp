using Altinn.AccessMgmt.DbAccess.Data.Models;
using Npgsql;

namespace Altinn.AccessMgmt.DbAccess.Data.Contracts;

/// <summary>
/// Basic data access
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDbBasicRepo<T>
{
    #region NEW

    /// <summary>
    /// Get single entity based on identity
    /// </summary>
    /// <param name="filters">GenericFilter</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>T<see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<IEnumerable<T>> Get(List<GenericFilter>? filters = null, RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create entity
    /// </summary>
    /// <param name="entity">Entity to create</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<int> Create(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update single property on entity
    /// </summary>
    /// <param name="id">Identity</param>
    /// <param name="parameters">GenericParameters</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<int> Update(Guid id, List<GenericParameter> parameters, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update entity
    /// </summary>
    /// <param name="id">Identity</param>
    /// <param name="entity">Entity to update</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<int> Update(Guid id, T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Insert or update entity
    /// </summary>
    /// <param name="id">Identity</param>
    /// <param name="entity">Entity to insert or update</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<int> Upsert(Guid id, T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete entity
    /// </summary>
    /// <param name="id">Identity</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<int> Delete(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for entities
    /// </summary>
    /// <param name="term">Term to filter on</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>T<see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<(IEnumerable<T> Data, PagedResult PageInfo)> Search(string term, RequestOptions options, CancellationToken cancellationToken = default);
    #endregion

    #region OLD

    /// <summary>
    /// Ingest entities using bulk methods
    /// </summary>
    /// <param name="data">Entities to ingest</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task Ingest(List<T> data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates translation for entity
    /// </summary>
    /// <param name="entity">Entity to create translation for</param>
    /// <param name="language">Languagecode (e.g nob, bbo, eng)</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<int> CreateTranslation(T entity, string language, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates translation for entity
    /// </summary>
    /// <param name="id">Identity</param>
    /// <param name="entity">Entity to update translation for</param>
    /// <param name="language">Languagecode (e.g nob, bbo, eng)</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<int> UpdateTranslation(Guid id, T entity, string language, CancellationToken cancellationToken = default);

    #endregion
}
