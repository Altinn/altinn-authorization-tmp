namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

/// <summary>
/// To be formated to HTTP Headers
/// </summary>
public class PagedResult
{
    /// <summary>
    /// Current page
    /// Header: x-page-current
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Item count
    /// Header: x-page-itemcount
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// Page size
    /// Header: x-page-size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Page count
    /// Header: x-page-count
    /// </summary>
    public int PageCount { get; set; }

    /// <summary>
    /// Links
    /// </summary>
    public Dictionary<string, string> Links { get; set; }
}
