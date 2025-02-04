namespace Altinn.AccessMgmt.Models;

/// <summary>
/// DelegationRolePackage
/// </summary>
public class DelegationRolePackage
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
}

/// <summary>
/// Extended DelegationRolePackage
/// </summary>
public class ExtDelegationRolePackage : DelegationRolePackage
{
    /// <summary>
    /// Delegation
    /// </summary>
    public Delegation Delegation { get; set; }

    /// <summary>
    /// RolePackage
    /// </summary>
    public RolePackage RolePackage { get; set; }
}
