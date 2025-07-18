namespace Altinn.AccessManagement.Core.Models.Connection;

/// <summary>
/// Package permission core model for business logic
/// </summary>
public class PackagePermission
{
    /// <summary>
    /// Package identifier
    /// </summary>
    public Guid PackageId { get; set; }

    /// <summary>
    /// Package name
    /// </summary>
    public string PackageName { get; set; } = string.Empty;

    /// <summary>
    /// Package description
    /// </summary>
    public string PackageDescription { get; set; } = string.Empty;

    /// <summary>
    /// Available permissions for this package
    /// </summary>
    public List<Permission> Permissions { get; set; } = new();

    /// <summary>
    /// Associated resources
    /// </summary>
    public List<Resource> Resources { get; set; } = new();
}

/// <summary>
/// Permission core model
/// </summary>
public class Permission
{
    /// <summary>
    /// Permission identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Permission name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Permission description
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Resource core model
/// </summary>
public class Resource
{
    /// <summary>
    /// Resource identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Resource name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Resource type
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;
}