namespace Altinn.AccessMgmt.Persistence.Core.Models;

/// <summary>
/// Holds audit information
/// </summary>
public class DbAudit
{
    /// <summary>
    /// When this version is valid from
    /// </summary>
    public DateTimeOffset ValidFrom { get; set; }

    /// <summary>
    /// When this version is valid to
    /// </summary>
    public DateTimeOffset ValidTo { get; set; }

    /// <summary>
    /// User responsible for latest version
    /// </summary>
    public Guid ChangedBy { get; set; }

    /// <summary>
    /// The system used to create latest version
    /// </summary>
    public Guid ChangedVia { get; set; }

    /// <summary>
    /// User responsible for deleting record
    /// </summary>
    public Guid DeletedBy { get; set; }

    /// <summary>
    /// System used for deleting record
    /// </summary>
    public Guid DeletedVia { get; set; }
}

/// <summary>
/// Base interface for extending classes with audit information
/// </summary>
public interface IBaseAudit
{
    /// <summary>
    /// Holds audit information
    /// </summary>
    public DbAudit Audit { get; set; }
}
