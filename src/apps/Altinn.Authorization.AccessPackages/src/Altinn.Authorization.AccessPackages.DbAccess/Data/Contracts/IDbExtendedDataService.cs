using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;

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
    Task<IEnumerable<TExtended>> GetExtended(RequestOptions? options = null);

    /// <summary>
    /// Get based on property
    /// </summary>
    /// <param name="property">Filter property</param>
    /// <param name="value">Filter value</param>
    /// <param name="options">RequestOptions</param>
    Task<IEnumerable<TExtended>> GetExtended(string property, Guid value, RequestOptions? options = null);

    /// <summary>
    /// Get based on property
    /// </summary>
    /// <param name="property">Filter property</param>
    /// <param name="value">Filter value</param>
    /// <param name="options">RequestOptions</param>
    Task<IEnumerable<TExtended>> GetExtended(string property, int value, RequestOptions? options = null);

    /// <summary>
    /// Get based on property
    /// </summary>
    /// <param name="property">Filter property</param>
    /// <param name="value">Filter value</param>
    /// <param name="options">RequestOptions</param>
    Task<IEnumerable<TExtended>> GetExtended(string property, string value, RequestOptions? options = null);

    /// <summary>
    /// Get based on identifer
    /// </summary>
    /// <param name="id">Identifier</param>
    /// <param name="options">RequestOptions</param>
    Task<TExtended?> GetExtended(Guid id, RequestOptions? options = null);

    /// <summary>
    /// Search
    /// </summary>
    /// <param name="term">Searchterm</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="startsWith">Starts with</param>
    Task<(IEnumerable<TExtended> Data, PagedResult PageInfo)> SearchExtended(string term, RequestOptions options, bool startsWith = false);
}