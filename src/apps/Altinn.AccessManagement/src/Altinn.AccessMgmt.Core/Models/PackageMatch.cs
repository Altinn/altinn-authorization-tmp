namespace Altinn.AccessMgmt.Core.Models;

/// <summary>
/// How a packages filter selects clients.
/// </summary>
public enum PackageMatch
{
    /// <summary>
    /// A client is selected when it holds at least one of the filter packages.
    /// </summary>
    Any,

    /// <summary>
    /// A client is selected when it holds every filter package. Packages held through a role and
    /// packages delegated directly count as one set, so a client can cover the filter through a
    /// combination of both.
    /// </summary>
    All,
}
