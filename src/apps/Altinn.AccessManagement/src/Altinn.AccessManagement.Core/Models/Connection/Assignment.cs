namespace Altinn.AccessManagement.Core.Models.Connection;

/// <summary>
/// Assignment core model for business logic
/// </summary>
public class Assignment
{
    /// <summary>
    /// Assignment identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Role identifier
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// From party identifier
    /// </summary>
    public Guid FromId { get; set; }

    /// <summary>
    /// To party identifier
    /// </summary>
    public Guid ToId { get; set; }

    /// <summary>
    /// Assignment status
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