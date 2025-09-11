using System.ComponentModel.DataAnnotations.Schema;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Base;

/// <summary>
/// Area to organize accesspackages stuff
/// </summary>
[NotMapped]
public class BaseRequest : BaseAudit
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Status
    /// </summary>
    public Guid StatusId { get; set; }

    /// <summary>
    /// Requested is created by
    /// </summary>
    public Guid RequestedById { get; set; }

    /// <summary>
    /// Assignment from entity identity
    /// </summary>
    public Guid FromId { get; set; }

    /// <summary>
    /// Assignment to entity identity
    /// </summary>
    public Guid ToId { get; set; }

    /// <summary>
    /// Assignment role identity
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Delegation via entity identity
    /// </summary>
    public Guid? ViaId { get; set; }

    /// <summary>
    /// Delegation via role identity
    /// </summary>
    public Guid? ViaRoleId { get; set; }
}
