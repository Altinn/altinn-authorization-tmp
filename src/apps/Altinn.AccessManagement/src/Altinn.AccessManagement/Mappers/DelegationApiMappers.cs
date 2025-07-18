using Altinn.AccessManagement.Core.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement.Common;
using Altinn.Authorization.Api.Contracts.AccessManagement.Delegation;

namespace Altinn.AccessManagement.Mappers;

/// <summary>
/// Maps between delegation DTOs and core models
/// </summary>
public static class DelegationApiMappers
{
    public static DelegationModel ToModel(CreateDelegationDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new DelegationModel(
            id: DelegationId.New(),
            offeredBy: new PartyId(dto.OfferedByPartyId),
            offeredByName: string.Empty, // Will be populated by service
            coveredBy: new PartyId(dto.CoveredByPartyId),
            coveredByName: string.Empty, // Will be populated by service
            resourceId: new ResourceId(dto.ResourceId),
            resourceType: string.Empty, // Will be populated by service
            performedBy: new UserId(dto.PerformedByUserId),
            rights: dto.Rights.Select(ToModel).ToList()
        );
    }

    public static DelegationModel UpdateModel(UpdateDelegationDto dto, DelegationModel existing)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(existing);

        return existing.UpdateRights(
            dto.Rights.Select(ToModel).ToList(),
            new UserId(dto.PerformedByUserId)
        );
    }

    public static DelegationDto ToDto(DelegationModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new DelegationDto
        {
            Id = model.Id.Value,
            OfferedByPartyId = model.OfferedBy.Value,
            OfferedByName = model.OfferedByName,
            OfferedByOrganizationNumber = model.OfferedByOrganizationNumber,
            CoveredByPartyId = model.CoveredBy.Value,
            CoveredByName = model.CoveredByName,
            CoveredByOrganizationNumber = model.CoveredByOrganizationNumber,
            ResourceId = model.ResourceId.Value,
            ResourceType = model.ResourceType,
            Created = model.Created,
            Updated = model.Updated,
            PerformedByUserId = model.PerformedBy.Value,
            Status = ToDto(model.Status),
            Rights = model.Rights.Select(ToDto).ToList(),
            ResourceReferences = model.ResourceReferences.Select(ToDto).ToList(),
            CompetentAuthority = model.CompetentAuthority != null ? ToDto(model.CompetentAuthority) : null
        };
    }

    public static List<DelegationDto> ToDto(List<DelegationModel> models)
    {
        ArgumentNullException.ThrowIfNull(models);
        return models.Select(ToDto).ToList();
    }

    private static DelegationModel.DelegationRight ToModel(CreateDelegationDto.DelegationRightDto dto)
    {
        return new DelegationModel.DelegationRight(
            dto.Action,
            dto.Resource,
            dto.AttributeMatches?.Select(ToModel).ToList()
        );
    }

    private static DelegationModel.DelegationRight ToModel(UpdateDelegationDto.DelegationRightDto dto)
    {
        return new DelegationModel.DelegationRight(
            dto.Action,
            dto.Resource,
            dto.AttributeMatches?.Select(ToModel).ToList()
        );
    }

    private static DelegationDto.DelegationRightDto ToDto(DelegationModel.DelegationRight model)
    {
        return new DelegationDto.DelegationRightDto
        {
            Action = model.Action,
            Resource = model.Resource,
            AttributeMatches = model.AttributeMatches.Select(ToDto).ToList()
        };
    }

    private static ResourceReferenceDto ToDto(DelegationModel.ResourceReference model)
    {
        return new ResourceReferenceDto
        {
            ReferenceType = model.ReferenceType,
            Reference = model.Reference,
            ReferenceSource = model.ReferenceSource
        };
    }

    private static DelegationDto.CompetentAuthorityDto ToDto(DelegationModel.CompetentAuthority model)
    {
        return new DelegationDto.CompetentAuthorityDto
        {
            Orgcode = model.Orgcode,
            Organization = model.Organization,
            Name = model.Name
        };
    }

    private static AttributeMatchDto ToDto(AttributeMatch model)
    {
        return new AttributeMatchDto
        {
            Id = model.Id,
            Value = model.Value,
            Type = ToDto(model.Type)
        };
    }

    private static AttributeMatch ToModel(AttributeMatchDto dto)
    {
        return new AttributeMatch(
            dto.Id,
            dto.Value,
            ToModel(dto.Type),
            dto.DataType
        );
    }

    private static DelegationStatusDto ToDto(DelegationStatus status)
    {
        return status switch
        {
            DelegationStatus.Active => DelegationStatusDto.Active,
            DelegationStatus.Revoked => DelegationStatusDto.Revoked,
            DelegationStatus.Expired => DelegationStatusDto.Expired,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown delegation status")
        };
    }

    private static AttributeMatchTypeDto ToDto(AttributeMatchType type)
    {
        return type switch
        {
            AttributeMatchType.Equals => AttributeMatchTypeDto.Equals,
            AttributeMatchType.Contains => AttributeMatchTypeDto.Contains,
            AttributeMatchType.StartsWith => AttributeMatchTypeDto.StartsWith,
            AttributeMatchType.EndsWith => AttributeMatchTypeDto.EndsWith,
            AttributeMatchType.GreaterThan => AttributeMatchTypeDto.GreaterThan,
            AttributeMatchType.LessThan => AttributeMatchTypeDto.LessThan,
            AttributeMatchType.GreaterThanOrEqual => AttributeMatchTypeDto.GreaterThanOrEqual,
            AttributeMatchType.LessThanOrEqual => AttributeMatchTypeDto.LessThanOrEqual,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown attribute match type")
        };
    }

    private static AttributeMatchType ToModel(AttributeMatchTypeDto type)
    {
        return type switch
        {
            AttributeMatchTypeDto.Equals => AttributeMatchType.Equals,
            AttributeMatchTypeDto.Contains => AttributeMatchType.Contains,
            AttributeMatchTypeDto.StartsWith => AttributeMatchType.StartsWith,
            AttributeMatchTypeDto.EndsWith => AttributeMatchType.EndsWith,
            AttributeMatchTypeDto.GreaterThan => AttributeMatchType.GreaterThan,
            AttributeMatchTypeDto.LessThan => AttributeMatchType.LessThan,
            AttributeMatchTypeDto.GreaterThanOrEqual => AttributeMatchType.GreaterThanOrEqual,
            AttributeMatchTypeDto.LessThanOrEqual => AttributeMatchType.LessThanOrEqual,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown attribute match type")
        };
    }
}