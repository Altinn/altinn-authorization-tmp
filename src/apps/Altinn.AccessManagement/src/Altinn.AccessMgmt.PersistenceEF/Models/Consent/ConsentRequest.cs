using Altinn.Authorization.Api.Contracts.Consent;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Consent;

/// <summary>
/// Maps the <c>consent.consentrequest</c> table.
/// </summary>
public class ConsentRequest
{
    /// <summary>Primary key (<c>consentrequestid</c>).</summary>
    public Guid ConsentRequestId { get; set; }

    /// <summary>The party the consent is given from.</summary>
    public Guid? FromPartyUuid { get; set; }

    /// <summary>The delegator required to give the consent, when set.</summary>
    public Guid? RequiredDelegatorUuid { get; set; }

    /// <summary>The party the consent is given to.</summary>
    public Guid? ToPartyUuid { get; set; }

    /// <summary>The party that handled the request on behalf of the receiver, when set.</summary>
    public Guid? HandledByPartyUuid { get; set; }

    /// <summary>Localized request messages, stored as <c>hstore</c>.</summary>
    public Dictionary<string, string>? RequestMessage { get; set; }

    /// <summary>Redirect URL used by the portal flow.</summary>
    public string? RedirectUrl { get; set; }

    /// <summary>Soft-delete flag.</summary>
    public bool? IsDeleted { get; set; }

    /// <summary>When the request was created.</summary>
    public DateTimeOffset Created { get; set; }

    /// <summary>The consent template the request is based on.</summary>
    public string TemplateId { get; set; }

    /// <summary>The template version, when set.</summary>
    public int? TemplateVersion { get; set; }

    /// <summary>Current status of the request.</summary>
    public ConsentRequestStatusType Status { get; set; }

    /// <summary>When the consent is valid to.</summary>
    public DateTimeOffset ValidTo { get; set; }

    /// <summary>When the consent was accepted, when set.</summary>
    public DateTimeOffset? Consented { get; set; }

    /// <summary>When the consent was revoked, when set.</summary>
    public DateTimeOffset? Revoked { get; set; }

    /// <summary>When the request was rejected, when set.</summary>
    public DateTimeOffset? Rejected { get; set; }

    /// <summary>Whether the request is shown in the portal.</summary>
    public ConsentPortalViewMode PortalViewMode { get; set; }
}
