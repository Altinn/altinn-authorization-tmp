using System.ComponentModel.DataAnnotations.Schema;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Base;

/// <summary>
/// Area to organize accesspackages stuff
/// </summary>
[NotMapped]
public class BaseRequestPackage : BaseAudit
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
    /// Requested package
    /// </summary>
    public Guid PackageId { get; set; }
}
