namespace Altinn.AccessMgmt.Models;

/// <summary>
/// DelegationRolePackageResource
/// </summary>
public class DelegationRolePackageResource
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
    /// RolePackage identity
    /// </summary>
    public Guid RolePackageId { get; set; }

    /// <summary>
    /// PackageResource identity
    /// </summary>
    public Guid PackageResourceId { get; set; }
}


/// <summary>
/// Extended DelegationRolePackageResource
/// </summary>
public class ExtDelegationRolePackageResource : DelegationRolePackageResource
{
    /// <summary>
    /// Delegation
    /// </summary>
    public Delegation Delegation { get; set; }

    /// <summary>
    /// RolePackage
    /// </summary>
    public RolePackage RolePackage { get; set; }

    /// <summary>
    /// PackageResource
    /// </summary>
    public PackageResource PackageResource { get; set; }
}
