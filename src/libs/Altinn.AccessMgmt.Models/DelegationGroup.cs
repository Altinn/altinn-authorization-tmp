namespace Altinn.AccessMgmt.Models;

/// <summary>
/// DelegationGroup
/// </summary>
public class DelegationGroup
{
    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Delegation Identity
    /// </summary>
    public Guid DelegationId { get; set; }

    /// <summary>
    /// Group Identity
    /// </summary>
    public Guid GroupId { get; set; }
}

/// <summary>
/// ExtDelegationGroup
/// </summary>
public class ExtDelegationGroup : DelegationGroup
{
    /// <summary>
    /// Delegation
    /// </summary>
    public Delegation Delegation { get; set; }

    /// <summary>
    /// Group
    /// </summary>
    public EntityGroup Group { get; set; }
}
