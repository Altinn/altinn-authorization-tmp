namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// RoleAssignment
/// </summary>
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
    /// To (Entity)
    /// </summary>
    public Entity To { get; set; }
}