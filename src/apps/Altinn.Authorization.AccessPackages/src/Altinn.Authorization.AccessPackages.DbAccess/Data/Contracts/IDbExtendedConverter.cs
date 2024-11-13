using System.Data;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;

/// <summary>
/// Converters for rows to extendedobjects
/// </summary>
/// <typeparam name="T">BasicType</typeparam>
/// <typeparam name="TExtended">ExtendedType</typeparam>
public interface IDbExtendedConverter<T, TExtended> : IDbBasicConverter<T>
{
    /// <summary>
    /// Convert list of extended objects from DataReader
    /// </summary>
    /// <param name="reader">IDataReader</param>
    /// <returns></returns>
    List<TExtended> ConvertExtended(IDataReader reader);
}
