namespace Altinn.AccessMgmt.Models;

/// <summary>
/// Delegation assignments
/// </summary>
public class DelegationAssignment
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
    /// Assignment identity
    /// </summary>
    public Guid AssignmentId { get; set; }
}

/// <summary>
/// Extended DelegationAssignment
/// </summary>
public class ExtDelegationAssignment : DelegationAssignment
{
    /// <summary>
    /// Delegation
    /// </summary>
    public Delegation Delegation { get; set; }

    /// <summary>
    /// Assignment
    /// </summary>
    public Assignment Assignment { get; set; }
}
