namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// Entity Delegation
/// This will give a single entity the same assignment as refrenced
/// Following the same principal as GroupDelegation, RoleDelegation, EntityDelegation and AssignmentDelegation
/// </summary>
public class AssignmentDelegation
{
    /// <summary>
    /// Identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Assignment refrence
    /// </summary>
    public Guid FromAssignmentId { get; set; }

    /// <summary>
    /// Entity refrence
    /// </summary>
    public Guid ToAssignmentId { get; set; }
}

/// <summary>
/// Extended Group Delegation
/// </summary>
public class ExtAssignmentDelegation : AssignmentDelegation
{
    /// <summary>
    /// Assignment
    /// </summary>
    public Assignment FromAssignment { get; set; }

    /// <summary>
    /// Role
    /// </summary>
    public Assignment ToAssignment { get; set; }
}
