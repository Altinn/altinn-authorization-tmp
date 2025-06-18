using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models;

/// <inheritdoc />
public class AuditEntityType : EntityType, IAudit 
{
    /// <inheritdoc />
    public AuditInfo Audit { get; set; }
}
