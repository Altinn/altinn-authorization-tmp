namespace Altinn.AccessMgmt.PersistenceEF.Models.Audit.Base;

/// <summary>
/// Adds audit properties to model
/// </summary>
public interface IAudit
{
    /// <summary>
    /// ValidFrom
    /// </summary>
    public DateTime ValidFrom { get; set; }

    /// <summary>
    /// ValidTo
    /// </summary>
    public DateTime? ValidTo { get; set; }

    /// <summary>
    /// ChangedBy
    /// </summary>
    public Guid? ChangedBy { get; set; }

    /// <summary>
    /// ChangedBySystem
    /// </summary>
    public Guid? ChangedBySystem { get; set; }

    /// <summary>
    /// ChangeOperation
    /// </summary>
    public string ChangeOperation { get; set; }

    /// <summary>
    /// DeletedBy
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// DeletedBySystem
    /// </summary>
    public Guid? DeletedBySystem { get; set; }

    /// <summary>
    /// DeleteOperation
    /// </summary>
    public string DeleteOperation { get; set; }
}
