namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// Group member
/// Members of the refrenced group
/// ActiveFrom/ActiveTo are optional for temporary memberships or delayed startup or end
/// </summary>
public class GroupMember
{
    /// <summary>
    /// Identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Group refrence
    /// </summary>
    public Guid GroupId { get; set; } // Group

    /// <summary>
    /// Member refrence
    /// </summary>
    public Guid MemberId { get; set; } // Entity

    /// <summary>
    /// Indicate for when this membership is active from
    /// </summary>
    public DateTimeOffset? ActiveFrom { get; set; }

    /// <summary>
    /// Indicate for when this membership is active to
    /// </summary>
    public DateTimeOffset? ActiveTo { get; set; }
}

/// <summary>
/// Extended Group Member
/// </summary>
public class ExtGroupMember : GroupMember
{
    /// <summary>
    /// Group
    /// </summary>
    public Group Group { get; set; }

    /// <summary>
    /// Member entity
    /// </summary>
    public Entity Member { get; set; }
}
