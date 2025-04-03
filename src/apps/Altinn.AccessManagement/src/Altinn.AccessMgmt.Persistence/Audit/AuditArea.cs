using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Models;

namespace Altinn.AccessMgmt.Persistence.Audit;

/// <summary>
/// Area model with audit fields
/// </summary>
public class AuditArea : Area, IBaseAudit
{
    /// <inheritdoc />
    public DbAudit Audit { get; set; }
}

/// <summary>
/// Area model with extended audit fields
/// </summary>
public class ExtAuditArea : Area
{
    public ExtBaseAudit Audit { get; set; }
}
