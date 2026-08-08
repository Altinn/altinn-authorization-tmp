namespace Altinn.AccessMgmt.PersistenceEF.Models.Consent;

/// <summary>
/// Maps the <c>consent.context</c> table.
/// </summary>
public class ConsentContext
{
    /// <summary>Primary key (<c>contextid</c>).</summary>
    public Guid ContextId { get; set; }

    /// <summary>The consent request this context belongs to.</summary>
    public Guid ConsentRequestId { get; set; }

    /// <summary>The language of the context.</summary>
    public string Language { get; set; }
}
