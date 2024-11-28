namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// Group Delegation
/// This will give the members of the refrenced group the same Assignments as the refrenced Assignment
/// Following the same principal as GroupDelegation, RoleDelegation, EntityDelegation and AssignmentDelegation
/// </summary>
public class GroupDelegation
{
    /// <summary>
    /// Identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Assignment refrence
    /// </summary>
    public Guid AssignmentId { get; set; } // From

    /// <summary>
    /// Group refrence
    /// </summary>
    public Guid GroupId { get; set; } // To
}

/// <summary>
/// Extended Group Delegation
/// </summary>
public class ExtGroupDelegation : GroupDelegation
{
    /// <summary>
    /// Assignment
    /// </summary>
    public Assignment Assignment { get; set; }

    /// <summary>
    /// Group
    /// </summary>
    public Group Group { get; set; }
}
