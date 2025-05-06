using System.Collections;
using System.Data;

namespace Altinn.AccessMgmt.Persistence.Core.Utilities;

/// <summary>
/// Defines methods for converting data from an <see cref="IDataReader"/> into a collection of objects.
/// </summary>
public interface IDbConverter
{
    /// <summary>
    /// Converts the data from the provided <see cref="IDataReader"/> into a list of objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of objects to create. The type must have a parameterless constructor.
    /// </typeparam>
    /// <param name="reader">The data reader that contains the data to convert.</param>
    /// <returns>
    /// A list of objects of type <typeparamref name="T"/> constructed from the data read.
    /// </returns>
    QueryResponse<T> ConvertToResult<T>(IDataReader reader)
        where T : new();
}

/// <summary>
/// Response from db query
/// </summary>
/// <typeparam name="T"></typeparam>
public class QueryResponse<T> : IEnumerable<T>
{
    /// <summary>
    /// Rows converted to objects
    /// </summary>
    public IEnumerable<T> Data { get; set; }
    
    /// <summary>
    /// Page information
    /// </summary>
    public QueryPageInfo Page { get; set; }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => Data.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Query pageing information
/// </summary>
public class QueryPageInfo
{
    /// <summary>
    /// Current page
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Intended page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total result size
    /// </summary>
    public int TotalSize { get; set; }

    /// <summary>
    /// First rownumber on page
    /// </summary>
    public int FirstRowOnPage { get; set; }

    /// <summary>
    /// Last rownumber on page
    /// </summary>
    public int LastRowOnPage { get; set; }
}
