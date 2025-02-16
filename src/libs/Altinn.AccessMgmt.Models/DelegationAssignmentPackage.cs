namespace Altinn.AccessMgmt.Models;

/// <summary>
/// DelegationAssignmentPackage
/// </summary>
public class DelegationAssignmentPackage
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
    /// AssignmentPackage identity
    /// </summary>
    public Guid AssignmentPackageId { get; set; }
}

/// <summary>
/// Extended DelegationAssignmentPackage
/// </summary>
public class ExtDelegationAssignmentPackage : DelegationAssignmentPackage
{
    /// <summary>
    /// Delegation
    /// </summary>
    public Delegation Delegation { get; set; }

    /// <summary>
    /// AssignmentPackage
    /// </summary>
    public AssignmentPackage AssignmentPackage { get; set; }
}
