namespace Altinn.AccessMgmt.Models;

/// <summary>
/// DelegationAssignmentResource
/// </summary>
public class DelegationAssignmentResource
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
    /// AssignmentResource identity
    /// </summary>
    public Guid AssignmentResourceId { get; set; }
}

/// <summary>
/// Extended DelegationAssignmentResource
/// </summary>
public class ExtDelegationAssignmentResource : DelegationAssignmentResource
{
    /// <summary>
    /// Delegation
    /// </summary>
    public Delegation Delegation { get; set; }

    /// <summary>
    /// AssignmentPackage
    /// </summary>
    public AssignmentResource AssignmentResource { get; set; }
}
