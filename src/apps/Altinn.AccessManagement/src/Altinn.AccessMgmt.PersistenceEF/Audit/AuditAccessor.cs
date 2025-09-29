using Altinn.AccessMgmt.PersistenceEF.Extensions;

namespace Altinn.AccessMgmt.PersistenceEF.Audit;

public class AuditAccessor : IAuditAccessor
{
    public AuditValues Current { get; set; }
}

public interface IAuditAccessor
{
    AuditValues? Current { get; set; }
}
