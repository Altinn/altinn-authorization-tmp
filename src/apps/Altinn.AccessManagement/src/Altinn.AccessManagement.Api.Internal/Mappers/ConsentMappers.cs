using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.Authorization.Api.Contracts.AccessManagement.Consent;

namespace Altinn.AccessManagement.Api.Internal.Mappers;

public static class ConsentMappers
{
    public static ConsentRequest ToCore(this ConsentRequestDto dto)
    {
        return new ConsentRequest
        {
            Id = dto.Id,
            From = dto.From.ToCore(),
            To = dto.To.ToCore(),
            RequiredDelegator = dto.RequiredDelegator?.ToCore(),
            ValidTo = dto.ValidTo,
            ConsentRights = dto.ConsentRights?.Select(r => r.ToCore()).ToList() ?? [],
            RequestMessage = dto.RequestMessage,
            RedirectUrl = dto.RedirectUrl
        };
    }

    public static ConsentRequestDto ToDto(this ConsentRequest core)
    {
        return new ConsentRequestDto
        {
            Id = core.Id,
            From = core.From.ToDto(),
            To = core.To.ToDto(),
            RequiredDelegator = core.RequiredDelegator?.ToDto(),
            ValidTo = core.ValidTo,
            ConsentRights = core.ConsentRights?.Select(r => r.ToDto()).ToList() ?? [],
            RequestMessage = core.RequestMessage,
            RedirectUrl = core.RedirectUrl
        };
    }

    public static ConsentRequestDetails ToCore(this ConsentRequestDetailsDto dto)
    {
        return new ConsentRequestDetails
        {
            Id = dto.Id,
            From = dto.From.ToCore(),
            To = dto.To.ToCore(),
            RequiredDelegator = dto.RequiredDelegator?.ToCore(),
            ValidTo = dto.ValidTo,
            ConsentRights = dto.ConsentRights?.Select(r => r.ToCore()).ToList() ?? [],
            RequestMessage = dto.RequestMessage,
            RedirectUrl = dto.RedirectUrl,
            Status = dto.Status switch
            {
                ConsentRequestStatusDto.Pending => ConsentStatus.Pending,
                ConsentRequestStatusDto.Approved => ConsentStatus.Approved,
                ConsentRequestStatusDto.Rejected => ConsentStatus.Rejected,
                ConsentRequestStatusDto.Expired => ConsentStatus.Expired,
                _ => ConsentStatus.Pending
            }
        };
    }

    public static ConsentRequestDetailsDto ToDto(this ConsentRequestDetails core)
    {
        return new ConsentRequestDetailsDto
        {
            Id = core.Id,
            From = core.From.ToDto(),
            To = core.To.ToDto(),
            RequiredDelegator = core.RequiredDelegator?.ToDto(),
            ValidTo = core.ValidTo,
            ConsentRights = core.ConsentRights?.Select(r => r.ToDto()).ToList() ?? [],
            RequestMessage = core.RequestMessage,
            RedirectUrl = core.RedirectUrl,
            Status = core.Status switch
            {
                ConsentStatus.Pending => ConsentRequestStatusDto.Pending,
                ConsentStatus.Approved => ConsentRequestStatusDto.Approved,
                ConsentStatus.Rejected => ConsentRequestStatusDto.Rejected,
                ConsentStatus.Expired => ConsentRequestStatusDto.Expired,
                _ => ConsentRequestStatusDto.Pending
            }
        };
    }

    public static ConsentRight ToCore(this ConsentRightDto dto)
    {
        return new ConsentRight
        {
            ResourceId = dto.ResourceId,
            Action = dto.Action,
            ResourceAttributes = dto.ResourceAttributes?.Select(a => a.ToCore()).ToList() ?? []
        };
    }

    public static ConsentRightDto ToDto(this ConsentRight core)
    {
        return new ConsentRightDto
        {
            ResourceId = core.ResourceId,
            Action = core.Action,
            ResourceAttributes = core.ResourceAttributes?.Select(a => a.ToDto()).ToList() ?? []
        };
    }

    public static ConsentParty ToCore(this ConsentPartyUrn dto)
    {
        return new ConsentParty
        {
            PartyUrn = dto.Value,
            PartyType = dto.PartyType switch
            {
                "Person" => ConsentPartyType.Person,
                "Organization" => ConsentPartyType.Organization,
                _ => ConsentPartyType.Person
            }
        };
    }

    public static ConsentPartyUrn ToDto(this ConsentParty core)
    {
        return new ConsentPartyUrn
        {
            Value = core.PartyUrn,
            PartyType = core.PartyType switch
            {
                ConsentPartyType.Person => "Person",
                ConsentPartyType.Organization => "Organization",
                _ => "Person"
            }
        };
    }

    public static ConsentResourceAttribute ToCore(this ConsentResourceAttributeDto dto)
    {
        return new ConsentResourceAttribute
        {
            AttributeId = dto.AttributeId,
            AttributeValue = dto.AttributeValue
        };
    }

    public static ConsentResourceAttributeDto ToDto(this ConsentResourceAttribute core)
    {
        return new ConsentResourceAttributeDto
        {
            AttributeId = core.AttributeId,
            AttributeValue = core.AttributeValue
        };
    }
}