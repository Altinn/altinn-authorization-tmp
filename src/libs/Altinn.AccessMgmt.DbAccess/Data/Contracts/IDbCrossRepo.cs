using Altinn.AccessMgmt.DbAccess.Data.Models;

namespace Altinn.AccessMgmt.DbAccess.Data.Contracts;

/// <summary>
/// Interface to retrive data from cross join tables
/// </summary>
/// <typeparam name="TA">A Table</typeparam>
/// <typeparam name="T">Cross join table</typeparam>
/// <typeparam name="TB">B Table</typeparam>
public interface IDbCrossRepo<TA, T, TB> : IDbBasicRepo<T>
{
    /// <summary>
    /// Override default column names for the cross table
    /// </summary>
    /// <param name="xAColumn">Columnname in TResult for TA</param>
    /// <param name="xBColumn">Columnname in TResult for TB</param>
    void SetCrossColumns(string xAColumn, string xBColumn);

    /// <summary>
    /// Using a cross join table to return TA objects based on TB.Id
    /// </summary>
    /// <param name="BId">Identity for TB object</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task<IEnumerable<TA>> ExecuteForA(Guid BId, RequestOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Using a cross join table to return TB objects based on TA.Id
    /// </summary>
    /// <param name="AId">Identity for TA object</param>
    /// <param name="options">RequestOptions</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    Task<IEnumerable<TB>> ExecuteForB(Guid AId, RequestOptions? options = null, CancellationToken cancellationToken = default);
}
