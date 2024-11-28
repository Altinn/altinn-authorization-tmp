namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// PackageDelegation
/// </summary>
public class PackageDelegation
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
    /// ForId
    /// </summary>
    public Guid ForId { get; set; }

    /// <summary>
    /// ToId
    /// </summary>
    public Guid ToId { get; set; }

    /// <summary>
    /// ById
    /// </summary>
    public Guid ById { get; set; }
}

/// <summary>
/// Extended PackageDelegation
/// </summary>
public class ExtPackageDelegation : PackageDelegation
{
    /// <summary>
    /// Package
    /// </summary>
    public Package Package { get; set; }

    /// <summary>
    /// For (Entity)
    /// </summary>
    public Entity For { get; set; }

    /// <summary>
    /// ActiveTo (Entity)
    /// </summary>
    public Entity To { get; set; }

    /// <summary>
    /// By (Entity)
    /// </summary>
    public Entity By { get; set; }
}