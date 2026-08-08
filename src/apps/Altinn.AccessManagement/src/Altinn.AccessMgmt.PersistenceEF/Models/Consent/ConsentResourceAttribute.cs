namespace Altinn.AccessMgmt.PersistenceEF.Models.Consent;

/// <summary>
/// Maps the <c>consent.resourceattribute</c> table.
/// </summary>
public class ConsentResourceAttribute
{
    /// <summary>Primary key (<c>consentrightid</c>).</summary>
    public Guid ConsentRightId { get; set; }

    /// <summary>The attribute type.</summary>
    public string? Type { get; set; }

    /// <summary>The attribute value.</summary>
    public string? Value { get; set; }

    /// <summary>The attribute version.</summary>
    public string? Version { get; set; }
}
