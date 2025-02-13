namespace Altinn.AccessMgmt.Models;

/// <summary>
/// Delegate assignment to group
/// </summary>
public class GroupDelegation
{
    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Assignment to delegate from
    /// </summary>
    public Guid FromId { get; set; }

    /// <summary>
    /// Group to delegate to
    /// </summary>
    public Guid ToId { get; set; }

    /// <summary>
    /// Entity between from and to
    /// </summary>
    public Guid ViaId { get; set; }

    /// <summary>
    /// Entity origin
    /// </summary>
    public Guid SourceId { get; set; }
}

/// <summary>
/// Extended Group delegation
/// </summary>
public class ExtGroupDelegation : GroupDelegation
{
    /// <summary>
    /// Assignment
    /// </summary>
    public Assignment From { get; set; }

    /// <summary>
    /// Group
    /// </summary>
    public EntityGroup To { get; set; }

    /// <summary>
    /// Entity between from and to
    /// </summary>
    public Entity Via { get; set; }

    /// <summary>
    /// Entity origin
    /// </summary>
    public Entity Source { get; set; }
}
