using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Contracts;

namespace Altinn.AccessMgmt.PersistenceEF.Models;

/// <summary>
/// Audit extension of Package
/// </summary>
public class AuditPackage : Package, IAudit
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
