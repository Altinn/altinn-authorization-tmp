using Altinn.Authorization.Api.Contracts.Consent;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Consent;

/// <summary>
/// Maps the <c>consent.consentevent</c> table.
/// </summary>
public class ConsentEvent
{
    /// <summary>Primary key (<c>consenteventid</c>).</summary>
    public Guid ConsentEventId { get; set; }

    /// <summary>The consent request this event belongs to.</summary>
    public Guid ConsentRequestId { get; set; }

    /// <summary>The type of event.</summary>
    public ConsentRequestEventType EventType { get; set; }

    /// <summary>When the event occurred.</summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>The party that performed the event.</summary>
    public Guid PerformedByParty { get; set; }
}
