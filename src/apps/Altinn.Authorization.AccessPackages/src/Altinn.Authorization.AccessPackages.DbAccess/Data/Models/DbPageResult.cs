namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

/// <summary>
/// DbPageResult
/// Used to extract data from query result
/// </summary>
public class DbPageResult
{
    /// <summary>
    /// Total pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Total items
    /// </summary>
    public int TotalItems { get; set; }
}
