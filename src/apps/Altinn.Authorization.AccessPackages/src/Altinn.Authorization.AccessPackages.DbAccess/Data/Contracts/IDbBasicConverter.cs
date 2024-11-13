using System.Data;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;

/// <summary>
/// Converters for rows to objects
/// </summary>
/// <typeparam name="T">BasicType</typeparam>
public interface IDbBasicConverter<T>
{
    /// <summary>
    /// Convert list of basic object from DataReader
    /// </summary>
    /// <param name="reader">IDataReader</param>
    /// <returns></returns>
    List<T> ConvertBasic(IDataReader reader);

    /// <summary>
    /// Convert single basic object from DataReader
    /// </summary>
    /// <param name="reader">IDataReader</param>
    /// <param name="prefix">Column prefix</param>
    /// <returns></returns>
    T? ConvertSingleBasic(IDataReader reader, string prefix);
}
