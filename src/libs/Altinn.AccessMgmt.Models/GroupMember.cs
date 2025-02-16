namespace Altinn.AccessMgmt.Models;

/// <summary>
/// EntityGroup member
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
    /// EntityGroup refrence
    /// </summary>
    public Guid GroupId { get; set; } // EntityGroup

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
/// Extended EntityGroup Member
/// </summary>
public class ExtGroupMember : GroupMember
{
    /// <summary>
    /// EntityGroup
    /// </summary>
    public EntityGroup Group { get; set; }

    /// <summary>
    /// Member entity
    /// </summary>
    public Entity Member { get; set; }
}
