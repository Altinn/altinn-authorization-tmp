namespace Altinn.AccessMgmt.PersistenceEF.Models.Consent;

/// <summary>
/// Maps the <c>consent.metadata</c> table. The table has no primary key constraint;
/// the entity uses (<c>consentrightid</c>, <c>id</c>) as its key, which is what the
/// consuming code treats as unique, so rows can be tracked and inserted.
/// </summary>
public class ConsentMetadata
{
    /// <summary>The consent right this metadata belongs to.</summary>
    public Guid ConsentRightId { get; set; }

    /// <summary>The metadata key (<c>id</c>).</summary>
    public string Id { get; set; }

    /// <summary>The metadata value.</summary>
    public string? Value { get; set; }
}
