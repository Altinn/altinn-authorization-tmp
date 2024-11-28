namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// Role Delegation
/// This will give entities with the refrences role assigned the same assignment as refrenced
/// Following the same principal as GroupDelegation, RoleDelegation, EntityDelegation and AssignmentDelegation
/// </summary>
public class RoleDelegation
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
    /// Role refrence
    /// </summary>
    public Guid RoleId { get; set; }
}

/// <summary>
/// Extended Group Delegation
/// </summary>
public class ExtRoleDelegation : RoleDelegation
{
    /// <summary>
    /// Assignment
    /// </summary>
    public Assignment Assignment { get; set; }

    /// <summary>
    /// Role
    /// </summary>
    public Role Role { get; set; }
}
