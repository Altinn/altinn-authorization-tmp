using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Audit;

/// <inheritdoc />
public class AuditEntity : Entity, IAudit 
{
    /// <inheritdoc />
    public DateTime ValidFrom { get; set; }

    /// <inheritdoc />
    public DateTime? ValidTo { get; set; }

    /// <inheritdoc />
    public Guid? ChangedBy { get; set; }

    /// <inheritdoc />
    public Guid? ChangedBySystem { get; set; }

    /// <inheritdoc />
    public string ChangeOperation { get; set; }

    /// <inheritdoc />
    public Guid? DeletedBy { get; set; }

    /// <inheritdoc />
    public Guid? DeletedBySystem { get; set; }

    /// <inheritdoc />
    public string DeleteOperation { get; set; }
}
