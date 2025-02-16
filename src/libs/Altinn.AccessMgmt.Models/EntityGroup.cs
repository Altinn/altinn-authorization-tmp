namespace Altinn.AccessMgmt.Models;

/// <summary>
/// EntityGroup for grouping entities on an entity
/// Members and Admins are stored in GroupMember and GroupAdmin
/// RequireRole is a hint that members without an existing role should not be allowed as a member or admin
/// </summary>
public class EntityGroup
{
    /// <summary>
    /// Identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Distinct name for Owner
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Entity that ownes the group
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Hint to membership requirement
    /// </summary>
    public bool RequireRole { get; set; }
}

/// <summary>
/// Extended EntityGroup
/// </summary>
public class ExtEntityGroup : EntityGroup
{
    /// <summary>
    /// Owner Entity
    /// </summary>
    public Entity Owner { get; set; }

    /// <summary>
    /// Experimental
    /// List of members with valid information
    /// </summary>
    public List<GroupMember> Members { get; set; } // Based on GroupMember.GroupId

    /// <summary>
    /// Experimental
    /// List of Administrators
    /// </summary>
    public List<GroupAdmin> Administrators { get; set; } // Based on GroupAdmin.GroupId
}
