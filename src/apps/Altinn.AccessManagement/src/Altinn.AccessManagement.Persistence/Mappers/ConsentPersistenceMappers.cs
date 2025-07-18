using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Persistence.Models.Consent;

namespace Altinn.AccessManagement.Persistence.Mappers;

public static class ConsentPersistenceMappers
{
    public static ConsentRequest ToCore(this ConsentRequestEntity entity)
    {
        return new ConsentRequest
        {
            Id = entity.Id,
            From = entity.From.ToCore(),
            To = entity.To.ToCore(),
            RequiredDelegator = entity.RequiredDelegator?.ToCore(),
            ValidTo = entity.ValidTo,
            ConsentRights = entity.ConsentRights?.Select(r => r.ToCore()).ToList() ?? [],
            RequestMessage = entity.RequestMessage,
            RedirectUrl = entity.RedirectUrl,
            Status = entity.Status switch
            {
                "Pending" => ConsentStatus.Pending,
                "Approved" => ConsentStatus.Approved,
                "Rejected" => ConsentStatus.Rejected,
                "Expired" => ConsentStatus.Expired,
                _ => ConsentStatus.Pending
            },
            CreatedDateTime = entity.CreatedDateTime,
            LastChangedDateTime = entity.LastChangedDateTime
        };
    }

    public static ConsentRequestEntity ToEntity(this ConsentRequest core)
    {
        return new ConsentRequestEntity
        {
            Id = core.Id,
            From = core.From.ToEntity(),
            To = core.To.ToEntity(),
            RequiredDelegator = core.RequiredDelegator?.ToEntity(),
            ValidTo = core.ValidTo,
            ConsentRights = core.ConsentRights?.Select(r => r.ToEntity()).ToList() ?? [],
            RequestMessage = core.RequestMessage,
            RedirectUrl = core.RedirectUrl,
            Status = core.Status switch
            {
                ConsentStatus.Pending => "Pending",
                ConsentStatus.Approved => "Approved",
                ConsentStatus.Rejected => "Rejected",
                ConsentStatus.Expired => "Expired",
                _ => "Pending"
            },
            CreatedDateTime = core.CreatedDateTime,
            LastChangedDateTime = core.LastChangedDateTime
        };
    }

    public static ConsentRight ToCore(this ConsentRightEntity entity)
    {
        return new ConsentRight
        {
            ResourceId = entity.ResourceId,
            Action = entity.Action,
            ResourceAttributes = entity.ResourceAttributes?.Select(a => a.ToCore()).ToList() ?? []
        };
    }

    public static ConsentRightEntity ToEntity(this ConsentRight core)
    {
        return new ConsentRightEntity
        {
            ResourceId = core.ResourceId,
            Action = core.Action,
            ResourceAttributes = core.ResourceAttributes?.Select(a => a.ToEntity()).ToList() ?? []
        };
    }

    public static ConsentParty ToCore(this ConsentPartyEntity entity)
    {
        return new ConsentParty
        {
            PartyUrn = entity.PartyUrn,
            PartyType = entity.PartyType switch
            {
                "Person" => ConsentPartyType.Person,
                "Organization" => ConsentPartyType.Organization,
                _ => ConsentPartyType.Person
            }
        };
    }

    public static ConsentPartyEntity ToEntity(this ConsentParty core)
    {
        return new ConsentPartyEntity
        {
            PartyUrn = core.PartyUrn,
            PartyType = core.PartyType switch
            {
                ConsentPartyType.Person => "Person",
                ConsentPartyType.Organization => "Organization",
                _ => "Person"
            }
        };
    }
}