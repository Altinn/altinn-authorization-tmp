namespace Altinn.Authorization.Api.Contracts.AccessManagement.Connection;

/// <summary>
/// Package permission data transfer object
/// </summary>
public class PackagePermissionDto
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
    public List<PermissionDto> Permissions { get; set; } = new();

    /// <summary>
    /// Associated resources
    /// </summary>
    public List<ResourceDto> Resources { get; set; } = new();
}

/// <summary>
/// Permission information
/// </summary>
public class PermissionDto
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
/// Resource information
/// </summary>
public class ResourceDto
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