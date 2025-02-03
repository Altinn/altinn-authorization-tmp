namespace Altinn.AccessMgmt.Models;

/// <summary>
/// Delegation Resource
/// </summary>
public class DelegationResource
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
    /// Assignment resource identity
    /// </summary>
    public Guid? AssignmentResourceId { get; set; }

    /// <summary>
    /// Role resource identity
    /// </summary>
    public Guid? RoleResourceId { get; set; }
}

/// <summary>
/// Extended Delegation Resource
/// </summary>
public class ExtDelegationResource : DelegationResource
{
    /// <summary>
    /// Delegation
    /// </summary>
    public Delegation Delegation { get; set; }

    /// <summary>
    /// Assignment resource
    /// </summary>
    public AssignmentResource AssignmentResource { get; set; }

    /// <summary>
    /// Role resource
    /// </summary>
    public RoleResource RoleResource { get; set; }
}
