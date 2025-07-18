using Altinn.AccessManagement.Core.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement.AuthorizedParty;
using Altinn.Authorization.Api.Contracts.AccessManagement.Common;

namespace Altinn.AccessManagement.Mappers;

/// <summary>
/// Maps between authorized party DTOs and core models
/// </summary>
public static class AuthorizedPartyApiMappers
{
    public static AuthorizedPartyDto ToDto(AuthorizedPartyModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new AuthorizedPartyDto
        {
            PartyId = model.PartyId.Value,
            Name = model.Name,
            OrganizationNumber = model.OrganizationNumber,
            PersonIdentifier = model.PersonIdentifier,
            PartyType = ToDto(model.PartyType),
            Rights = model.Rights.Select(ToDto).ToList(),
            Resources = model.Resources.Select(ToDto).ToList(),
            Delegations = model.Delegations.Select(DelegationApiMappers.ToDto).ToList()
        };
    }

    public static List<AuthorizedPartyDto> ToDto(List<AuthorizedPartyModel> models)
    {
        ArgumentNullException.ThrowIfNull(models);
        return models.Select(ToDto).ToList();
    }

    private static AuthorizedPartyDto.RightDto ToDto(AuthorizedPartyModel.Right model)
    {
        return new AuthorizedPartyDto.RightDto
        {
            Action = model.Action,
            Resource = model.Resource,
            Source = ToDto(model.Source),
            AttributeMatches = model.AttributeMatches.Select(ToDto).ToList()
        };
    }

    private static AuthorizedPartyDto.ResourceDto ToDto(AuthorizedPartyModel.Resource model)
    {
        return new AuthorizedPartyDto.ResourceDto
        {
            ResourceId = model.ResourceId.Value,
            ResourceType = model.ResourceType,
            Title = model.Title,
            Description = model.Description,
            Actions = model.AvailableActions
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

    private static PartyTypeDto ToDto(PartyType type)
    {
        return type switch
        {
            PartyType.Person => PartyTypeDto.Person,
            PartyType.Organization => PartyTypeDto.Organization,
            PartyType.SubUnit => PartyTypeDto.SubUnit,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown party type")
        };
    }

    private static RightSourceTypeDto ToDto(RightSourceType source)
    {
        return source switch
        {
            RightSourceType.Role => RightSourceTypeDto.Role,
            RightSourceType.Delegation => RightSourceTypeDto.Delegation,
            RightSourceType.AccessList => RightSourceTypeDto.AccessList,
            RightSourceType.SystemUser => RightSourceTypeDto.SystemUser,
            RightSourceType.Maskinporten => RightSourceTypeDto.Maskinporten,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown right source type")
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
}