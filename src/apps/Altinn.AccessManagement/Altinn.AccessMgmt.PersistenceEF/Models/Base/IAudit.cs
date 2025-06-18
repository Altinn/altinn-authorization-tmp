namespace Altinn.AccessMgmt.PersistenceEF.Models.Base;

/// <summary>
/// Adds audit properties to model
/// </summary>
public interface IAudit
{
    AuditInfo Audit { get; set; }
}
