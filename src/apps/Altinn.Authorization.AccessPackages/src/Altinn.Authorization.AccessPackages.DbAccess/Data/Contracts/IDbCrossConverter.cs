using System.Data;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;

/// <summary>
/// Converters for rows to extendedobjects
/// </summary>
/// <typeparam name="TA">A Table</typeparam>
/// <typeparam name="T">Cross join table</typeparam>
/// <typeparam name="TB">B Table</typeparam>
public interface IDbCrossConverter<TA, T, TB> : IDbBasicConverter<T>
{
    /// <summary>
    /// Convert list of TA objects from DataReader
    /// </summary>
    /// <param name="reader">IDataReader</param>
    /// <returns></returns>
    List<TA> ConvertA(IDataReader reader);

    /// <summary>
    /// Convert list of TB objects from DataReader
    /// </summary>
    /// <param name="reader">IDataReader</param>
    /// <returns></returns>
    List<TB> ConvertB(IDataReader reader);
}