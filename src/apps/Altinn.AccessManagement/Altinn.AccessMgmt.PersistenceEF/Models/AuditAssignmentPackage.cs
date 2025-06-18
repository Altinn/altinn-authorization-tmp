using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models;

/// <inheritdoc />
public class AuditAssignmentPackage : AssignmentPackage, IAudit 
{
    /// <inheritdoc />
    public AuditInfo Audit { get; set; }
}
