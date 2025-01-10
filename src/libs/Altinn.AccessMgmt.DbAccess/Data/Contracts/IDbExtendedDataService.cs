using Altinn.AccessMgmt.DbAccess.Data.Models;
using System.Linq.Expressions;

namespace Altinn.AccessMgmt.DbAccess.Data.Contracts;

/// <summary>
/// Data repo interface for extended objects
/// </summary>
/// <typeparam name="T">Base Type</typeparam>
/// <typeparam name="TExtended">Extendtet Type</typeparam>
public interface IDbExtendedDataService<T, TExtended> : IDbBasicDataService<T>
{
    /// <summary>
    /// Actual repo implementation
    /// </summary>
    IDbExtendedRepo<T, TExtended> ExtendedRepo { get; }

    /// <summary>
    /// Get all
    /// </summary>
    /// <param name="options">RequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<IEnumerable<TExtended>> GetExtended(RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get Entities based on property and value
    /// </summary>
    /// <param name="property">Filter property</param>
    /// <param name="value">Filter value</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>T<see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<IEnumerable<TExtended>> GetExtended<TProperty>(Expression<Func<TExtended, TProperty>> property, TProperty value, RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get based on identifer
    /// </summary>
    /// <param name="id">Identifier</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<TExtended?> GetExtended(Guid id, RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search
    /// </summary>
    /// <param name="term">Searchterm</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="startsWith">Starts with</param>
    /// <param name="cancellationToken">CancellationToken</param>
    Task<(IEnumerable<TExtended> Data, PagedResult PageInfo)> SearchExtended(string term, RequestOptions options, bool startsWith = false, CancellationToken cancellationToken = default);
}
