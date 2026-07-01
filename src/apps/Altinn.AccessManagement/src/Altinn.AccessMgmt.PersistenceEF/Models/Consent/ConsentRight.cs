namespace Altinn.AccessMgmt.PersistenceEF.Models.Consent;

/// <summary>
/// Maps the <c>consent.consentright</c> table.
/// </summary>
public class ConsentRight
{
    /// <summary>Primary key (<c>consentrightid</c>).</summary>
    public Guid ConsentRightId { get; set; }

    /// <summary>The consent request this right belongs to.</summary>
    public Guid ConsentRequestId { get; set; }

    /// <summary>The actions the right covers, stored as <c>text[]</c>.</summary>
    public string[]? Action { get; set; }
}
