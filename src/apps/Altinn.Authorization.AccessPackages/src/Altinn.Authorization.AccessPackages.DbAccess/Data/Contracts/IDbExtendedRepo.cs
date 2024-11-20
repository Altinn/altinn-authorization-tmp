using System.Linq.Expressions;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;

/// <summary>
/// Extended repo for extendable objects
/// </summary>
/// <typeparam name="T">BaseType (e.g EntityType)</typeparam>
/// <typeparam name="TExtended">ExtendedType (e.g ExtEntityType)</typeparam>
public interface IDbExtendedRepo<T, TExtended> : IDbBasicRepo<T>
{
    /// <summary>
    /// Get extended object
    /// </summary>
    /// <param name="filters">GenericFilter</param>
    /// <param name="options">RequestOptions</param>
    Task<IEnumerable<TExtended>> GetExtended(List<GenericFilter>? filters = null, RequestOptions? options = null);

    /// <summary>
    /// Search
    /// </summary>
    /// <param name="term">Searchterm</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="startsWith">Starts with</param>
    Task<(IEnumerable<TExtended> Data, PagedResult PageInfo)> SearchExtended(string term, RequestOptions options, bool startsWith = false);

    /// <summary>
    /// Add join to configuration
    /// </summary>
    /// <typeparam name="TJoin">Type to join</typeparam>
    /// <param name="TProperty">Refrence property on T for join</param>
    /// <param name="TJoinProperty">Refrence property on TJoin for join</param>
    /// <param name="TExtendedProperty">Result property on TExtended</param>
    /// <param name="optional">Is this optional</param>
    void Join<TJoin>(Expression<Func<T, object?>> TProperty, Expression<Func<TJoin, object>> TJoinProperty, Expression<Func<TExtended, object?>> TExtendedProperty, bool optional = false);
}
