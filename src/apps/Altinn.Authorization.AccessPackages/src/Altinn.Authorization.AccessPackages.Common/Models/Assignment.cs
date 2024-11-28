namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// Assignment
/// </summary>
public class Assignment
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// RoleId
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// FromId
    /// </summary>
    public Guid FromId { get; set; }

    /// <summary>
    /// ToId
    /// </summary>
    public Guid ToId { get; set; }
}

/// <summary>
/// Extended RoleAssignment
/// </summary>
public class ExtAssignment : Assignment
{
    /// <summary>
    /// Role
    /// </summary>
    public Role Role { get; set; }

    /// <summary>
    /// From (Entity)
    /// </summary>
    public Entity From { get; set; }

    /// <summary>
    /// To (Entity)
    /// </summary>
    public Entity To { get; set; }
}
