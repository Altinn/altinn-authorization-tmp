namespace Altinn.AccessMgmt.Models;

/// <summary>
/// EntityGroup administrator
/// Members here are defined as administrators of the refrenced group
/// ActiveFrom/ActiveTo are optional for temporary memberships or delayed startup or end
/// </summary>
public class GroupAdmin
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
/// Extended EntityGroup Administrator
/// </summary>
public class ExtGroupAdmin : GroupAdmin
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
