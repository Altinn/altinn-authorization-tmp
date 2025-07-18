using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.AccessManagement.Enums;
using Altinn.AccessManagement.Models;
using Altinn.Authorization.ABAC.Constants;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Mappers;

/// <summary>
/// Manual mappers for Access Management external models
/// </summary>
public static class AccessManagementExternalMappers
{
    // Party mappings
    public static PartyExternal ToExternal(this Party party)
    {
        if (party == null)
            return null;

        return new PartyExternal
        {
            PartyId = party.PartyId,
            PartyUuid = party.PartyUuid,
            PartyTypeName = party.PartyTypeName,
            OrgNumber = party.OrgNumber,
            SSN = party.SSN,
            UnitType = party.UnitType,
            Name = party.Name,
            IsDeleted = party.IsDeleted,
            OnlyHierarchyElementWithNoAccess = party.OnlyHierarchyElementWithNoAccess,
            PersonName = party.PersonName,
            OrganizationName = party.OrganizationName,
            UnitStatus = party.UnitStatus,
            ChildParties = party.ChildParties?.Select(ToExternal).ToList()
        };
    }

    // Delegation mappings
    public static DelegationExternal ToExternal(this Delegation delegation)
    {
        if (delegation == null)
            return null;

        return new DelegationExternal
        {
            DelegationId = delegation.DelegationId,
            OfferedByPartyId = delegation.OfferedByPartyId,
            OfferedByName = delegation.OfferedByName,
            OfferedByOrganizationNumber = delegation.OfferedByOrganizationNumber,
            CoveredByPartyId = delegation.CoveredByPartyId,
            CoveredByName = delegation.CoveredByName,
            CoveredByOrganizationNumber = delegation.CoveredByOrganizationNumber,
            PerformedByUserId = delegation.PerformedByUserId,
            BlobStoragePolicyPath = delegation.BlobStoragePolicyPath,
            BlobStorageVersionId = delegation.BlobStorageVersionId,
            CreatedDateTime = delegation.CreatedDateTime,
            ResourceId = delegation.ResourceId,
            ResourceType = delegation.ResourceType,
            ResourceTitle = delegation.ResourceTitle,
            ResourceReferences = delegation.ResourceReferences?.Select(ToExternal).ToList(),
            CompetentAuthority = delegation.CompetentAuthority?.ToExternal()
        };
    }

    public static MaskinportenSchemaDelegationExternal ToMaskinportenSchemaExternal(this Delegation delegation)
    {
        if (delegation == null)
            return null;

        return new MaskinportenSchemaDelegationExternal
        {
            OfferedByPartyId = delegation.OfferedByPartyId,
            OfferedByName = delegation.OfferedByName,
            OfferedByOrganizationNumber = delegation.OfferedByOrganizationNumber,
            CoveredByPartyId = delegation.CoveredByPartyId,
            CoveredByName = delegation.CoveredByName,
            CoveredByOrganizationNumber = delegation.CoveredByOrganizationNumber,
            PerformedByUserId = delegation.PerformedByUserId,
            CreatedDateTime = delegation.CreatedDateTime,
            ResourceId = delegation.ResourceId,
            ResourceType = delegation.ResourceType,
            ResourceTitle = delegation.ResourceTitle
        };
    }

    public static MPDelegationExternal ToMPDelegationExternal(this Delegation delegation)
    {
        if (delegation == null)
            return null;

        return new MPDelegationExternal
        {
            SupplierOrg = delegation.CoveredByOrganizationNumber,
            ConsumerOrg = delegation.OfferedByOrganizationNumber,
            DelegationSchemeId = delegation.ResourceReferences?.Find(rf => rf.ReferenceType == ReferenceType.DelegationSchemeId)?.Reference,
            Scopes = delegation.ResourceReferences?.Where(rf => string.Equals(rf.ReferenceType, ReferenceType.MaskinportenScope))?.Select(rf => rf.Reference).ToList(),
            Created = delegation.CreatedDateTime,
            ResourceId = delegation.ResourceId
        };
    }

    // Resource Reference mappings
    public static ResourceReferenceExternal ToExternal(this ResourceReference reference)
    {
        if (reference == null)
            return null;

        return new ResourceReferenceExternal
        {
            ReferenceType = reference.ReferenceType,
            ReferenceSource = reference.ReferenceSource,
            Reference = reference.Reference
        };
    }

    // Competent Authority mappings
    public static CompetentAuthorityExternal ToExternal(this CompetentAuthority authority)
    {
        if (authority == null)
            return null;

        return new CompetentAuthorityExternal
        {
            Orgcode = authority.Orgcode,
            Organization = authority.Organization,
            Name = authority.Name
        };
    }

    // Attribute mappings
    public static AttributeMatchExternal ToExternal(this AttributeMatch match)
    {
        if (match == null)
            return null;

        return new AttributeMatchExternal
        {
            Id = match.Id,
            Value = match.Value,
            Type = match.Type,
            DataType = match.DataType
        };
    }

    public static AttributeMatch ToCore(this AttributeMatchExternal external)
    {
        if (external == null)
            return null;

        return new AttributeMatch
        {
            Id = external.Id,
            Value = external.Value,
            Type = external.Type,
            DataType = external.DataType
        };
    }

    public static BaseAttributeExternal ToExternal(this BaseAttribute attribute)
    {
        if (attribute == null)
            return null;

        return new BaseAttributeExternal
        {
            Id = attribute.Id,
            Value = attribute.Value,
            Type = attribute.Type,
            DataType = attribute.DataType
        };
    }

    public static BaseAttribute ToCore(this BaseAttributeExternal external)
    {
        if (external == null)
            return null;

        return new BaseAttribute
        {
            Id = external.Id,
            Value = external.Value,
            Type = external.Type,
            DataType = external.DataType
        };
    }

    public static PolicyAttributeMatchExternal ToExternal(this PolicyAttributeMatch match)
    {
        if (match == null)
            return null;

        return new PolicyAttributeMatchExternal
        {
            Id = match.Id,
            Value = match.Value,
            Type = match.Type,
            DataType = match.DataType
        };
    }

    public static PolicyAttributeMatch ToCore(this PolicyAttributeMatchExternal external)
    {
        if (external == null)
            return null;

        return new PolicyAttributeMatch
        {
            Id = external.Id,
            Value = external.Value,
            Type = external.Type,
            DataType = external.DataType
        };
    }

    // Rights mappings
    public static RightSourceExternal ToExternal(this RightSource source)
    {
        if (source == null)
            return null;

        return new RightSourceExternal
        {
            Id = source.Id,
            Value = source.Value,
            Type = source.Type,
            DataType = source.DataType
        };
    }

    public static RightSource ToCore(this RightSourceExternal external)
    {
        if (external == null)
            return null;

        return new RightSource
        {
            Id = external.Id,
            Value = external.Value,
            Type = external.Type,
            DataType = external.DataType
        };
    }

    public static RightsDelegationCheckRequest ToCore(this RightsDelegationCheckRequestExternal external)
    {
        if (external == null)
            return null;

        return new RightsDelegationCheckRequest
        {
            // Map properties here based on the actual structure
        };
    }

    public static RightDelegationCheckResultExternal ToExternal(this RightDelegationCheckResult result)
    {
        if (result == null)
            return null;

        return new RightDelegationCheckResultExternal
        {
            Action = result.Action?.Value,
            // Map other properties as needed
        };
    }

    public static DetailExternal ToExternal(this Detail detail)
    {
        if (detail == null)
            return null;

        return new DetailExternal
        {
            // Map properties based on actual structure
        };
    }

    public static RightExternal ToExternal(this Right right)
    {
        if (right == null)
            return null;

        return new RightExternal
        {
            Action = right.Action?.Value,
            // Map other properties as needed
        };
    }

    public static Right ToCore(this BaseRightExternal external)
    {
        if (external == null)
            return null;

        return new Right
        {
            Action = new AttributeMatch
            {
                Id = XacmlConstants.MatchAttributeIdentifiers.ActionId,
                Value = external.Action
            }
        };
    }

    public static BaseRightExternal ToBaseExternal(this Right right)
    {
        if (right == null)
            return null;

        return new BaseRightExternal
        {
            Action = right.Action?.Value
        };
    }

    public static RightDelegationExternal ToExternal(this RightDelegation delegation)
    {
        if (delegation == null)
            return null;

        return new RightDelegationExternal
        {
            // Map properties based on actual structure
        };
    }

    // Delegation Change mappings
    public static DelegationChangeExternal ToExternal(this DelegationChange change)
    {
        if (change == null)
            return null;

        return new DelegationChangeExternal
        {
            DelegationChangeId = change.DelegationChangeId,
            DelegationChangeType = change.DelegationChangeType.ToExternal(),
            OfferedByPartyId = change.OfferedByPartyId,
            OfferedByName = change.OfferedByName,
            OfferedByOrganizationNumber = change.OfferedByOrganizationNumber,
            CoveredByPartyId = change.CoveredByPartyId,
            CoveredByName = change.CoveredByName,
            CoveredByOrganizationNumber = change.CoveredByOrganizationNumber,
            PerformedByUserId = change.PerformedByUserId,
            BlobStoragePolicyPath = change.BlobStoragePolicyPath,
            BlobStorageVersionId = change.BlobStorageVersionId,
            CreatedDateTime = change.CreatedDateTime,
            ResourceId = change.ResourceId,
            ResourceType = change.ResourceType,
            ResourceTitle = change.ResourceTitle,
            InstanceId = change.InstanceId
        };
    }

    public static List<DelegationChangeExternal> ToExternal(this List<DelegationChange> changes)
    {
        if (changes == null)
            return new List<DelegationChangeExternal>();

        return changes.Select(ToExternal).ToList();
    }

    public static DelegationChangeTypeExternal ToExternal(this DelegationChangeType type)
    {
        return type switch
        {
            DelegationChangeType.Grant => DelegationChangeTypeExternal.Grant,
            DelegationChangeType.Revoke => DelegationChangeTypeExternal.Revoke,
            DelegationChangeType.RevokeLast => DelegationChangeTypeExternal.RevokeLast,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown delegation change type")
        };
    }

    // Delegation lookup mappings
    public static DelegationLookup ToCore(this RightsDelegationRequestExternal external)
    {
        if (external == null)
            return null;

        return new DelegationLookup
        {
            // Map properties based on actual structure
        };
    }

    public static DelegationLookup ToCore(this RevokeOfferedDelegationExternal external)
    {
        if (external == null)
            return null;

        return new DelegationLookup
        {
            // Map properties based on actual structure
        };
    }

    public static DelegationLookup ToCore(this RevokeReceivedDelegationExternal external)
    {
        if (external == null)
            return null;

        return new DelegationLookup
        {
            // Map properties based on actual structure
        };
    }

    // Rights delegation response mappings
    public static RightsDelegationResponseExternal ToExternal(this DelegationActionResult result)
    {
        if (result == null)
            return null;

        return new RightsDelegationResponseExternal
        {
            RightDelegationResults = result.Rights?.Select(ToExternal).ToList()
        };
    }

    public static RightDelegationResultExternal ToExternal(this RightDelegationResult result)
    {
        if (result == null)
            return null;

        return new RightDelegationResultExternal
        {
            Action = result.Action?.Value,
            // Map other properties as needed
        };
    }

    // Apps Instance Delegation mappings
    public static AppsInstanceDelegationRequest ToCore(this AppsInstanceDelegationRequestDto dto)
    {
        if (dto == null)
            return null;

        return new AppsInstanceDelegationRequest
        {
            From = dto.From.Value,
            To = dto.To.Value,
            // Map other properties as needed
        };
    }

    public static RightInternal ToCore(this RightDto dto)
    {
        if (dto == null)
            return null;

        return new RightInternal
        {
            Action = dto.Action?.Value,
            // Map other properties as needed
        };
    }

    public static AppsInstanceDelegationResponseDto ToDto(this AppsInstanceDelegationResponse response)
    {
        if (response == null)
            return null;

        return new AppsInstanceDelegationResponseDto
        {
            // Map properties based on actual structure
        };
    }

    public static RightDelegationResultDto ToDto(this InstanceRightDelegationResult result)
    {
        if (result == null)
            return null;

        return new RightDelegationResultDto
        {
            // Map properties based on actual structure
        };
    }

    public static InstanceDelegationMode ToCore(this InstanceDelegationModeExternal external)
    {
        return external switch
        {
            InstanceDelegationModeExternal.Normal => InstanceDelegationMode.Normal,
            InstanceDelegationModeExternal.ParallelSigning => InstanceDelegationMode.ParallelSigning,
            _ => throw new ArgumentOutOfRangeException(nameof(external), external, "Unknown instance delegation mode")
        };
    }

    public static AppsInstanceRevokeResponseDto ToDto(this AppsInstanceRevokeResponse response)
    {
        if (response == null)
            return null;

        return new AppsInstanceRevokeResponseDto
        {
            // Map properties based on actual structure
        };
    }

    public static RightRevokeResultDto ToDto(this InstanceRightRevokeResult result)
    {
        if (result == null)
            return null;

        return new RightRevokeResultDto
        {
            // Map properties based on actual structure
        };
    }

    public static ResourceRightDelegationCheckResultDto ToDto(this ResourceRightDelegationCheckResult result)
    {
        if (result == null)
            return null;

        return new ResourceRightDelegationCheckResultDto
        {
            // Map properties based on actual structure
        };
    }

    public static AuthorizedPartyTypeExternal ToExternal(this AuthorizedPartyType type)
    {
        return type switch
        {
            AuthorizedPartyType.Person => AuthorizedPartyTypeExternal.Person,
            AuthorizedPartyType.Organization => AuthorizedPartyTypeExternal.Organization,
            AuthorizedPartyType.SubUnit => AuthorizedPartyTypeExternal.SubUnit,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown authorized party type")
        };
    }
}