namespace Altinn.AccessMgmt.Models;

/// <summary>
/// DelegationRoleResource
/// </summary>
public class DelegationRoleResource
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
    /// RoleResource identity
    /// </summary>
    public Guid RoleResourceId { get; set; }
}

/// <summary>
/// Extended DelegationRoleResource
/// </summary>
public class ExtDelegationRoleResource : DelegationRoleResource
{
    /// <summary>
    /// Delegation
    /// </summary>
    public Delegation Delegation { get; set; }

    /// <summary>
    /// RolePackage
    /// </summary>
    public RoleResource RoleResource { get; set; }
}
