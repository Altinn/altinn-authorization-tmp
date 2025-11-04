using System.ComponentModel.DataAnnotations.Schema;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit.Base;
using Altinn.AccessMgmt.PersistenceEF.Models.Contracts;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Base;

/// <summary>
/// Area to organize accesspackages stuff
/// </summary>
[NotMapped]
public class BaseRequestStatus : BaseAudit, IEntityId, IEntityName
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Status
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Requested is created by
    /// </summary>
    public string Description { get; set; }
}
