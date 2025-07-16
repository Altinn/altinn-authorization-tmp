using System.Text.Json;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Persistence.Entities;

namespace Altinn.AccessManagement.Persistence.Mappers;

/// <summary>
/// Maps between delegation core models and database entities
/// </summary>
public static class DelegationPersistenceMappers
{
    public static DelegationEntity ToEntity(DelegationModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new DelegationEntity
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
            Status = (int)model.Status,
            Rights = model.Rights.Select(ToEntity).ToList(),
            ResourceReferences = model.ResourceReferences.Select(ToEntity).ToList(),
            CompetentAuthority = model.CompetentAuthority != null ? ToEntity(model.CompetentAuthority, model.Id.Value) : null
        };
    }

    public static DelegationModel ToModel(DelegationEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return DelegationModel.FromPersistence(
            id: new DelegationId(entity.Id),
            offeredBy: new PartyId(entity.OfferedByPartyId),
            offeredByName: entity.OfferedByName,
            offeredByOrganizationNumber: entity.OfferedByOrganizationNumber,
            coveredBy: new PartyId(entity.CoveredByPartyId),
            coveredByName: entity.CoveredByName,
            coveredByOrganizationNumber: entity.CoveredByOrganizationNumber,
            resourceId: new ResourceId(entity.ResourceId),
            resourceType: entity.ResourceType,
            created: entity.Created,
            updated: entity.Updated,
            performedBy: new UserId(entity.PerformedByUserId),
            status: (DelegationStatus)entity.Status,
            rights: entity.Rights.Select(ToModel).ToList(),
            resourceReferences: entity.ResourceReferences.Select(ToModel).ToList(),
            competentAuthority: entity.CompetentAuthority != null ? ToModel(entity.CompetentAuthority) : null
        );
    }

    public static List<DelegationModel> ToModel(List<DelegationEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        return entities.Select(ToModel).ToList();
    }

    public static void UpdateEntity(DelegationEntity entity, DelegationModel model)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(model);

        entity.OfferedByName = model.OfferedByName;
        entity.OfferedByOrganizationNumber = model.OfferedByOrganizationNumber;
        entity.CoveredByName = model.CoveredByName;
        entity.CoveredByOrganizationNumber = model.CoveredByOrganizationNumber;
        entity.ResourceType = model.ResourceType;
        entity.Updated = model.Updated;
        entity.PerformedByUserId = model.PerformedBy.Value;
        entity.Status = (int)model.Status;
        
        // Update collections
        entity.Rights.Clear();
        entity.Rights.AddRange(model.Rights.Select(ToEntity));
        
        entity.ResourceReferences.Clear();
        entity.ResourceReferences.AddRange(model.ResourceReferences.Select(ToEntity));

        // Update competent authority
        if (model.CompetentAuthority != null)
        {
            if (entity.CompetentAuthority == null)
            {
                entity.CompetentAuthority = ToEntity(model.CompetentAuthority, model.Id.Value);
            }
            else
            {
                entity.CompetentAuthority.Orgcode = model.CompetentAuthority.Orgcode;
                entity.CompetentAuthority.Organization = model.CompetentAuthority.Organization;
                entity.CompetentAuthority.Name = model.CompetentAuthority.Name;
            }
        }
        else
        {
            entity.CompetentAuthority = null;
        }
    }

    private static DelegationEntity.DelegationRightEntity ToEntity(DelegationModel.DelegationRight model)
    {
        return new DelegationEntity.DelegationRightEntity
        {
            Id = Guid.NewGuid(),
            Action = model.Action,
            Resource = model.Resource,
            AttributeMatches = model.AttributeMatches.Select(ToEntity).ToList()
        };
    }

    private static DelegationModel.DelegationRight ToModel(DelegationEntity.DelegationRightEntity entity)
    {
        return new DelegationModel.DelegationRight(
            entity.Action,
            entity.Resource,
            entity.AttributeMatches.Select(ToModel).ToList()
        );
    }

    private static DelegationEntity.DelegationRightEntity.AttributeMatchEntity ToEntity(AttributeMatch model)
    {
        return new DelegationEntity.DelegationRightEntity.AttributeMatchEntity
        {
            Id = Guid.NewGuid(),
            AttributeId = model.Id,
            AttributeValue = model.Value,
            MatchType = (int)model.Type,
            DataType = model.DataType
        };
    }

    private static AttributeMatch ToModel(DelegationEntity.DelegationRightEntity.AttributeMatchEntity entity)
    {
        return new AttributeMatch(
            entity.AttributeId,
            entity.AttributeValue,
            (AttributeMatchType)entity.MatchType,
            entity.DataType
        );
    }

    private static DelegationEntity.ResourceReferenceEntity ToEntity(DelegationModel.ResourceReference model)
    {
        return new DelegationEntity.ResourceReferenceEntity
        {
            Id = Guid.NewGuid(),
            ReferenceType = model.ReferenceType,
            Reference = model.Reference,
            ReferenceSource = model.ReferenceSource
        };
    }

    private static DelegationModel.ResourceReference ToModel(DelegationEntity.ResourceReferenceEntity entity)
    {
        return new DelegationModel.ResourceReference(
            entity.ReferenceType,
            entity.Reference,
            entity.ReferenceSource
        );
    }

    private static DelegationEntity.CompetentAuthorityEntity ToEntity(DelegationModel.CompetentAuthority model, Guid delegationId)
    {
        return new DelegationEntity.CompetentAuthorityEntity
        {
            Id = Guid.NewGuid(),
            DelegationId = delegationId,
            Orgcode = model.Orgcode,
            Organization = model.Organization,
            Name = model.Name
        };
    }

    private static DelegationModel.CompetentAuthority ToModel(DelegationEntity.CompetentAuthorityEntity entity)
    {
        return new DelegationModel.CompetentAuthority(
            entity.Orgcode,
            entity.Organization,
            entity.Name
        );
    }
}