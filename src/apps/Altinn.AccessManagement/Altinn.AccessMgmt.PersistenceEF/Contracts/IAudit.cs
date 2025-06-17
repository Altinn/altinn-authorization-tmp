namespace Altinn.AccessMgmt.PersistenceEF.Contracts;

/// <summary>
/// Adds audit properties to model
/// </summary>
public interface IAudit
{
    /// <summary>
    /// ValidFrom
    /// </summary>
    DateTime ValidFrom { get; set; }

    /// <summary>
    /// ValidTo
    /// </summary>
    DateTime? ValidTo { get; set; }

    /// <summary>
    /// ChangedBy
    /// </summary>
    Guid? ChangedBy { get; set; }

    /// <summary>
    /// ChangedBySystem
    /// </summary>
    Guid? ChangedBySystem { get; set; }

    /// <summary>
    /// ChangeOperation
    /// </summary>
    string ChangeOperation { get; set; }

    /// <summary>
    /// DeletedBy
    /// </summary>
    Guid? DeletedBy { get; set; }

    /// <summary>
    /// DeletedBySystem
    /// </summary>
    Guid? DeletedBySystem { get; set; }

    /// <summary>
    /// DeleteOperation
    /// </summary>
    string DeleteOperation { get; set; }
}
