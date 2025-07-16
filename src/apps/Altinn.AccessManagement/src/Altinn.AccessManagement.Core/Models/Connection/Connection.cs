namespace Altinn.AccessManagement.Core.Models.Connection;

/// <summary>
/// Represents a connection between parties in the access management system
/// </summary>
public class Connection
{
    /// <summary>
    /// The party making the request
    /// </summary>
    public required string Party { get; set; }

    /// <summary>
    /// The party the connection is from
    /// </summary>
    public required string From { get; set; }

    /// <summary>
    /// The party the connection is to
    /// </summary>
    public required string To { get; set; }

    /// <summary>
    /// Whether the connection is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the connection was created
    /// </summary>
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the connection was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}