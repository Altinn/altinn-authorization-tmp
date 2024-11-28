namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// Entity Delegation
/// This will give a single entity the same assignment as refrenced
/// Following the same principal as GroupDelegation, RoleDelegation, EntityDelegation and AssignmentDelegation
/// </summary>
public class EntityDelegation
{
    /// <summary>
    /// Identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Assignment refrence
    /// </summary>
    public Guid AssignmentId { get; set; }

    /// <summary>
    /// Entity refrence
    /// </summary>
    public Guid EntityId { get; set; }
}

/// <summary>
/// Extended Group Delegation
/// </summary>
public class ExtEntityDelegation : EntityDelegation
{
    /// <summary>
    /// Assignment
    /// </summary>
    public Assignment Assignment { get; set; }

    /// <summary>
    /// Role
    /// </summary>
    public Entity Entity { get; set; }
}
