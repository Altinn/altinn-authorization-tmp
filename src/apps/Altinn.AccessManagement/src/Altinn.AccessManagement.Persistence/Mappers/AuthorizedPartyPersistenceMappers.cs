using System.Text.Json;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Persistence.Entities;

namespace Altinn.AccessManagement.Persistence.Mappers;

/// <summary>
/// Maps between authorized party core models and database entities
/// </summary>
public static class AuthorizedPartyPersistenceMappers
{
    public static AuthorizedPartyEntity ToEntity(AuthorizedPartyModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new AuthorizedPartyEntity
        {
            Id = Guid.NewGuid(),
            PartyId = model.PartyId.Value,
            Name = model.Name,
            OrganizationNumber = model.OrganizationNumber,
            PersonIdentifier = model.PersonIdentifier,
            PartyType = (int)model.PartyType,
            Created = DateTime.UtcNow,
            Rights = model.Rights.Select(ToEntity).ToList(),
            Resources = model.Resources.Select(ToEntity).ToList()
        };
    }

    public static AuthorizedPartyModel ToModel(AuthorizedPartyEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var model = new AuthorizedPartyModel(
            new PartyId(entity.PartyId),
            entity.Name,
            (PartyType)entity.PartyType,
            entity.OrganizationNumber,
            entity.PersonIdentifier
        );

        foreach (var right in entity.Rights.Select(ToModel))
        {
            model.AddRight(right);
        }

        foreach (var resource in entity.Resources.Select(ToModel))
        {
            model.AddResource(resource);
        }

        return model;
    }

    public static List<AuthorizedPartyModel> ToModel(List<AuthorizedPartyEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        return entities.Select(ToModel).ToList();
    }

    private static AuthorizedPartyEntity.AuthorizedPartyRightEntity ToEntity(AuthorizedPartyModel.Right model)
    {
        return new AuthorizedPartyEntity.AuthorizedPartyRightEntity
        {
            Id = Guid.NewGuid(),
            Action = model.Action,
            Resource = model.Resource,
            Source = (int)model.Source,
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            AttributeMatches = model.AttributeMatches.Select(ToEntity).ToList()
        };
    }

    private static AuthorizedPartyModel.Right ToModel(AuthorizedPartyEntity.AuthorizedPartyRightEntity entity)
    {
        return new AuthorizedPartyModel.Right(
            entity.Action,
            entity.Resource,
            (RightSourceType)entity.Source,
            entity.AttributeMatches.Select(ToModel).ToList(),
            entity.ValidFrom,
            entity.ValidTo
        );
    }

    private static AuthorizedPartyEntity.AuthorizedPartyRightEntity.RightAttributeMatchEntity ToEntity(AttributeMatch model)
    {
        return new AuthorizedPartyEntity.AuthorizedPartyRightEntity.RightAttributeMatchEntity
        {
            Id = Guid.NewGuid(),
            AttributeId = model.Id,
            AttributeValue = model.Value,
            MatchType = (int)model.Type,
            DataType = model.DataType
        };
    }

    private static AttributeMatch ToModel(AuthorizedPartyEntity.AuthorizedPartyRightEntity.RightAttributeMatchEntity entity)
    {
        return new AttributeMatch(
            entity.AttributeId,
            entity.AttributeValue,
            (AttributeMatchType)entity.MatchType,
            entity.DataType
        );
    }

    private static AuthorizedPartyEntity.AuthorizedPartyResourceEntity ToEntity(AuthorizedPartyModel.Resource model)
    {
        return new AuthorizedPartyEntity.AuthorizedPartyResourceEntity
        {
            Id = Guid.NewGuid(),
            ResourceId = model.ResourceId.Value,
            ResourceType = model.ResourceType,
            Title = model.Title,
            Description = model.Description,
            AvailableActions = JsonSerializer.Serialize(model.AvailableActions)
        };
    }

    private static AuthorizedPartyModel.Resource ToModel(AuthorizedPartyEntity.AuthorizedPartyResourceEntity entity)
    {
        var availableActions = string.IsNullOrEmpty(entity.AvailableActions) 
            ? new List<string>() 
            : JsonSerializer.Deserialize<List<string>>(entity.AvailableActions) ?? new List<string>();

        return new AuthorizedPartyModel.Resource(
            new ResourceId(entity.ResourceId),
            entity.ResourceType,
            entity.Title,
            entity.Description,
            availableActions
        );
    }
}