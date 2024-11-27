using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;

/// <summary>
/// Data repo interface for cross joined objects
/// </summary>
/// <typeparam name="TA">TA Type</typeparam>
/// <typeparam name="T">Cross Type</typeparam>
/// <typeparam name="TB">TB Type</typeparam>
public interface IDbCrossDataService<TA, T, TB> : IDbBasicDataService<T>
{
    /// <summary>
    /// Actual repo implementation
    /// </summary>
    IDbCrossRepo<TA, T, TB> CrossRepo { get; }

    /// <summary>
    /// Using a cross join table to return TA objects based on TB.Id
    /// </summary>
    /// <param name="BId">Identity for TB object</param>
    /// <param name="options">RequestOptions</param>
    /// <returns></returns>
    Task<IEnumerable<TA>> GetA(Guid BId, RequestOptions? options = null);

    /// <summary>
    /// Using a cross join table to return TB objects based on TA.Id
    /// </summary>
    /// <param name="AId">Identity for TA object</param>
    /// <param name="options">RequestOptions</param>
    /// <returns></returns>
    Task<IEnumerable<TB>> GetB(Guid AId, RequestOptions? options = null);
}
