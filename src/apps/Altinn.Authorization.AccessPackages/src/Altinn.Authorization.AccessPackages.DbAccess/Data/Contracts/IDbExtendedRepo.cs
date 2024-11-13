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
    /// Adds join to configuration
    /// </summary>
    /// <typeparam name="TJoin"></typeparam>
    /// <param name="alias">Alias</param>
    /// <param name="baseJoinProperty">Property on base object to join from</param>
    /// <param name="joinProperty">Property on join object to join to</param>
    /// <param name="optional">Is this optional</param>
    void Join<TJoin>(string alias = "", string baseJoinProperty = "", string joinProperty = "Id", bool optional = false);
}