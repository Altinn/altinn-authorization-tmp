namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// Role Assignment
/// </summary>
[Obsolete("Use Assignment")]
public class RoleAssignment
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
    /// ForId
    /// </summary>
    public Guid ForId { get; set; }

    /// <summary>
    /// ToId
    /// </summary>
    public Guid ToId { get; set; }
}

/// <summary>
/// Extended RoleAssignment
/// </summary>
[Obsolete("Use ExtAssignment")]
public class ExtRoleAssignment : RoleAssignment
{
    /// <summary>
    /// Role
    /// </summary>
    public Role Role { get; set; }

    /// <summary>
    /// For (Entity)
    /// </summary>
    public Entity For { get; set; }

    /// <summary>
    /// ActiveTo (Entity)
    /// </summary>
    public Entity To { get; set; }
}
