namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// Group administrator
/// Members here are defined as administrators of the refrenced group
/// From/To are optional for temporary memberships or delayed startup or end
/// </summary>
public class GroupAdmin
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
/// Extended Group Administrator
/// </summary>
public class ExtGroupAdmin : GroupAdmin
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
