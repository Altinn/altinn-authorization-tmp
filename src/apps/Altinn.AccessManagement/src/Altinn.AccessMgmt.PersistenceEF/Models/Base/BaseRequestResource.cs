using System.ComponentModel.DataAnnotations.Schema;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Base;

/// <summary>
/// Area to organize accesspackages stuff
/// </summary>
[NotMapped]
public class BaseRequestResource : BaseAudit
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
    /// Status for requested element
    /// </summary>
    public Guid StatusId { get; set; }

    /// <summary>
    /// Requested resource
    /// </summary>
    public Guid ResourceId { get; set; }
}
