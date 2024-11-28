namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// Group member
/// Members of the refrenced group
/// From/To are optional for temporary memberships or delayed startup or end
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
    /// Indicate for when this membership is valid from
    /// </summary>
    public DateTimeOffset? From { get; set; }

    /// <summary>
    /// Indicate for when this membership is valid to
    /// </summary>
    public DateTimeOffset? To { get; set; }
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
