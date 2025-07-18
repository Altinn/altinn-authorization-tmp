using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Enums;

namespace Altinn.AccessManagement.Mappers;

/// <summary>
/// Maps between AuthorizedParty and AuthorizedPartyExternal
/// </summary>
public static class AuthorizedPartyExternalMappers
{
    public static AuthorizedPartyExternal ToExternal(this AuthorizedParty model)
    {
        if (model == null)
            return null;

        return new AuthorizedPartyExternal
        {
            PartyUuid = model.PartyUuid,
            Name = model.Name,
            OrganizationNumber = model.OrganizationNumber,
            PersonId = model.PersonId,
            PartyId = model.PartyId,
            Type = model.Type.ToExternal(),
            UnitType = model.UnitType,
            IsDeleted = model.IsDeleted,
            OnlyHierarchyElementWithNoAccess = model.OnlyHierarchyElementWithNoAccess,
            AuthorizedAccessPackages = model.AuthorizedAccessPackages?.ToList() ?? new List<string>(),
            AuthorizedResources = model.AuthorizedResources?.ToList() ?? new List<string>(),
            AuthorizedRoles = model.AuthorizedRoles?.ToList() ?? new List<string>(),
            AuthorizedInstances = model.AuthorizedInstances?.Select(ToExternal).ToList() ?? new List<AuthorizedPartyExternal.AuthorizedResource>(),
            Subunits = model.Subunits?.Select(ToExternal).ToList() ?? new List<AuthorizedPartyExternal>()
        };
    }

    public static List<AuthorizedPartyExternal> ToExternal(this List<AuthorizedParty> models)
    {
        if (models == null)
            return new List<AuthorizedPartyExternal>();

        return models.Select(ToExternal).ToList();
    }

    private static AuthorizedPartyExternal.AuthorizedResource ToExternal(this AuthorizedParty.AuthorizedResource model)
    {
        if (model == null)
            return null;

        return new AuthorizedPartyExternal.AuthorizedResource
        {
            ResourceId = model.ResourceId,
            InstanceId = model.InstanceId
        };
    }

    private static AuthorizedPartyTypeExternal ToExternal(this AuthorizedPartyType type)
    {
        return type switch
        {
            AuthorizedPartyType.Person => AuthorizedPartyTypeExternal.Person,
            AuthorizedPartyType.Organization => AuthorizedPartyTypeExternal.Organization,
            AuthorizedPartyType.SubUnit => AuthorizedPartyTypeExternal.SubUnit,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown party type")
        };
    }
}