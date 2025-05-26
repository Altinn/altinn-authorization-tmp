using Altinn.AccessMgmt.Core.Models;

namespace Altinn.AccessMgmt.Integration.Models;

/// <summary>
/// Connection from one party to another
/// </summary>
public class RelationPermissionDto
{
    /// <summary>
    /// Party
    /// </summary>
    public CompactEntity Party { get; set; } = new();

    /// <summary>
    /// Roles the party has for given filter
    /// </summary>
    public List<CompactRole> Roles { get; set; } = new();

    /// <summary>
    /// Packages the party has
    /// </summary>
    public List<CompactPackage> Packages { get; set; } = new();

    /// <summary>
    /// Resources the party has
    /// </summary>
    public List<CompactResource> Resources { get; set; } = new();

    /// <summary>
    /// Connections the party has
    /// </summary>
    public List<RelationPermissionDto> Connections { get; set; } = new();
}

/// <summary>
/// Connection from one party to another
/// </summary>
public class RelationDto
{
    /// <summary>
    /// Party
    /// </summary>
    public CompactEntity Party { get; set; } = new();

    /// <summary>
    /// Roles the party has for given filter
    /// </summary>
    public List<CompactRole> Roles { get; set; } = new();

    /// <summary>
    /// Connections the party has
    /// </summary>
    public List<RelationPermissionDto> Connections { get; set; } = new();
}

/// <summary>
/// Connection Permission
/// </summary>
public class ConnectionPermission
{
    /// <summary>
    /// The party with permission
    /// </summary>
    public CompactEntity Party { get; set; }

    /// <summary>
    /// KeyRole permissions
    /// </summary>
    public List<Permission> KeyRoles { get; set; }

    /// <summary>
    /// Delegated permissions
    /// </summary>
    public List<Permission> Delegations { get; set; }
}

/// <summary>
/// Permission
/// </summary>
public class Permission
{
    public CompactEntity From { get; set; }
    public CompactEntity To { get; set; }
    public CompactEntity Via { get; set; }
    public CompactRole Role { get; set; }
    public CompactRole ViaRole { get; set; }
}
