namespace Altinn.AccessMgmt.Models;

/// <summary>
/// Delegation packages
/// </summary>
public class DelegationPackageResource
{
    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Delegation identity
    /// </summary>
    public Guid DelegationId { get; set; }

    /// <summary>
    /// Package identity
    /// </summary>
    public Guid PackageResourceId { get; set; }
}

/// <summary>
/// Extended DelegationPackageResource
/// </summary>
public class ExtDelegationPackageResource : DelegationPackageResource
{
    /// <summary>
    /// Delegation
    /// </summary>
    public Delegation Delegation { get; set; }

    /// <summary>
    /// Package
    /// </summary>
    public PackageResource Package { get; set; }
}
