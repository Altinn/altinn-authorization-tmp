using Altinn.AccessMgmt.Core.Models;

namespace Altinn.AccessMgmt.Integration.Models;

/// <summary>
/// Connection from one party to another
/// </summary>
public class CompactRelationDto
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
    public List<CompactRelationDto> Connections { get; set; } = new();
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
    public List<RelationDto> Connections { get; set; } = new();

    /// <summary>
    /// Packages the party has
    /// </summary>
    public List<CompactPackage> Packages { get; set; } = new();

    /// <summary>
    /// Resources the party has
    /// </summary>
    public List<CompactResource> Resources { get; set; } = new();
}

/// <summary>
/// Resource permission
/// </summary>
public class ResourcePermission
{
    /// <summary>
    /// Resource the permissions are for
    /// </summary>
    public CompactResource Resource { get; set; }

    /// <summary>
    /// Parties with permissions
    /// </summary>
    public List<ConnectionPermission> Permissions { get; set; }
}

/// <summary>
/// Package permissions
/// </summary>
public class PackagePermission
{
    /// <summary>
    /// Package the permissions are for
    /// </summary>
    public CompactPackage Package { get; set; }

    /// <summary>
    /// Parties with permissions
    /// </summary>
    public List<ConnectionPermission> Permissions { get; set; }
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
    /// <summary>
    /// From party
    /// </summary>
    public CompactEntity From { get; set; }

    /// <summary>
    /// To party
    /// </summary>
    public CompactEntity To { get; set; }

    /// <summary>
    /// Via party
    /// </summary>
    public CompactEntity Via { get; set; }

    /// <summary>
    /// Role
    /// </summary>
    public CompactRole Role { get; set; }

    /// <summary>
    /// Via role
    /// </summary>
    public CompactRole ViaRole { get; set; }
}
