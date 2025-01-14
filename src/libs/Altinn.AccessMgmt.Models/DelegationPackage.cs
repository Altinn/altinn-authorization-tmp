namespace Altinn.AccessMgmt.Models;

/// <summary>
/// Delegation packages
/// </summary>
public class DelegationPackage
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
    public Guid PackageId { get; set; }
}

/// <summary>
/// Extended DelegationPackage
/// </summary>
public class ExtDelegationPackage : DelegationPackage
{
    /// <summary>
    /// Delegation
    /// </summary>
    public Delegation Delegation { get; set; }

    /// <summary>
    /// Package
    /// </summary>
    public Package Package { get; set; }
}
