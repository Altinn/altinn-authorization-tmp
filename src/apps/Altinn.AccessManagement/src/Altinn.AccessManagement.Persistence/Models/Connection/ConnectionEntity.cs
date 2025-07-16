using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Altinn.AccessManagement.Persistence.Models.Connection;

/// <summary>
/// Database entity for connection between parties
/// </summary>
[Table("connections", Schema = "accessmanagement")]
public class ConnectionEntity
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// The party making the request
    /// </summary>
    [Required]
    [Column("party")]
    [MaxLength(255)]
    public string Party { get; set; } = string.Empty;

    /// <summary>
    /// The party the connection is from
    /// </summary>
    [Required]
    [Column("from_party")]
    [MaxLength(255)]
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// The party the connection is to
    /// </summary>
    [Required]
    [Column("to_party")]
    [MaxLength(255)]
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Whether the connection is active
    /// </summary>
    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the connection was created
    /// </summary>
    [Column("created")]
    public DateTime Created { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the connection was last updated
    /// </summary>
    [Column("last_updated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}