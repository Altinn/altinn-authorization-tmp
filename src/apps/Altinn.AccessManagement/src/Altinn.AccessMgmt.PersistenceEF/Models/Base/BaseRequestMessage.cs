using System.ComponentModel.DataAnnotations.Schema;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Base;

/// <summary>
/// Messages for request communication
/// </summary>
[NotMapped]
public class BaseRequestMessage : BaseAudit
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Request
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Author of the message
    /// </summary>
    public Guid AuthorId { get; set; }

    /// <summary>
    /// Message content
    /// </summary>
    public string Content { get; set; }
}
