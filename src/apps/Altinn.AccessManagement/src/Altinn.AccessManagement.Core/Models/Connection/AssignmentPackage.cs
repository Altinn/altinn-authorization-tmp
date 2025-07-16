namespace Altinn.AccessManagement.Core.Models.Connection;

/// <summary>
/// Assignment package core model for business logic
/// </summary>
public class AssignmentPackage
{
    /// <summary>
    /// Assignment package identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Associated assignment identifier
    /// </summary>
    public Guid AssignmentId { get; set; }

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
    /// Package status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime ModifiedAt { get; set; }
}