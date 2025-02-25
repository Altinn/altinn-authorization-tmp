namespace Altinn.AccessMgmt.Core.Models;

/// <summary>
/// PackageTag
/// </summary>
public class PackageTag
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// PackageId
    /// </summary>
    public Guid PackageId { get; set; }

    /// <summary>
    /// TagId
    /// </summary>
    public Guid TagId { get; set; }
}

/// <summary>
/// Extended PackageTag
/// </summary>
public class ExtPackageTag : PackageTag
{
    /// <summary>
    /// Package
    /// </summary>
    public Package Package { get; set; }

    /// <summary>
    /// Tag
    /// </summary>
    public Tag Tag { get; set; }
}
