using System.Text.Json;
using Altinn.Authorization.Api.Contracts.Register;
using Altinn.Register.Core.Models;
using Altinn.Register.Persistence.Entities;
using Altinn.Authorization.Shared;

namespace Altinn.Register.Application.Mappers;

#region API Mappers (DTO ↔ Model)

/// <summary>
/// Maps between party DTOs and models
/// </summary>
public static class PartyApiMapper
{
    public static RegisterPartyDto ToDto(PartyModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new RegisterPartyDto
        {
            PartyId = model.PartyId.Value,
            Name = model.Name,
            PartyType = ToDto(model.PartyType),
            OrganizationNumber = model.OrganizationNumber,
            PersonIdentifier = model.PersonIdentifier,
            IsDeleted = model.IsDeleted,
            Created = model.Created,
            Updated = model.Updated,
            Contacts = model.Contacts.Select(ToDto).ToList(),
            Addresses = model.Addresses.Select(ToDto).ToList(),
            Details = model.Details != null ? ToDto(model.Details) : null
        };
    }

    public static List<RegisterPartyDto> ToDto(List<PartyModel> models)
    {
        ArgumentNullException.ThrowIfNull(models);
        return models.Select(ToDto).ToList();
    }

    public static PartyRelationshipDto ToDto(PartyRelationshipModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new PartyRelationshipDto
        {
            FromPartyId = model.FromPartyId.Value,
            ToPartyId = model.ToPartyId.Value,
            RelationshipType = ToDto(model.RelationshipType),
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            IsActive = model.IsActive
        };
    }

    private static RegisterPartyDto.PartyContactDto ToDto(PartyModel.PartyContact model)
    {
        return new RegisterPartyDto.PartyContactDto
        {
            ContactType = ToDto(model.ContactType),
            Value = model.Value,
            IsPrimary = model.IsPrimary
        };
    }

    private static RegisterPartyDto.PartyAddressDto ToDto(PartyModel.PartyAddress model)
    {
        return new RegisterPartyDto.PartyAddressDto
        {
            AddressType = ToDto(model.AddressType),
            StreetAddress = model.StreetAddress,
            PostalCode = model.PostalCode,
            City = model.City,
            Country = model.Country,
            IsPrimary = model.IsPrimary
        };
    }

    private static RegisterPartyDto.PartyDetailsDto ToDto(PartyModel.PartyDetails model)
    {
        return new RegisterPartyDto.PartyDetailsDto
        {
            Description = model.Description,
            Website = model.Website,
            Industry = model.Industry,
            EmployeeCount = model.EmployeeCount,
            FoundedDate = model.FoundedDate,
            CustomAttributes = model.CustomAttributes.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
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

    private static ContactTypeDto ToDto(ContactType type)
    {
        return type switch
        {
            ContactType.Email => ContactTypeDto.Email,
            ContactType.Phone => ContactTypeDto.Phone,
            ContactType.Mobile => ContactTypeDto.Mobile,
            ContactType.Fax => ContactTypeDto.Fax,
            ContactType.Website => ContactTypeDto.Website,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown contact type")
        };
    }

    private static AddressTypeDto ToDto(AddressType type)
    {
        return type switch
        {
            AddressType.Business => AddressTypeDto.Business,
            AddressType.Postal => AddressTypeDto.Postal,
            AddressType.Visiting => AddressTypeDto.Visiting,
            AddressType.Home => AddressTypeDto.Home,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown address type")
        };
    }

    private static RelationshipTypeDto ToDto(RelationshipType type)
    {
        return type switch
        {
            RelationshipType.Parent => RelationshipTypeDto.Parent,
            RelationshipType.Child => RelationshipTypeDto.Child,
            RelationshipType.Subsidiary => RelationshipTypeDto.Subsidiary,
            RelationshipType.Branch => RelationshipTypeDto.Branch,
            RelationshipType.Representative => RelationshipTypeDto.Representative,
            RelationshipType.Owner => RelationshipTypeDto.Owner,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown relationship type")
        };
    }
}

/// <summary>
/// Maps between user profile DTOs and models
/// </summary>
public static class UserProfileApiMapper
{
    public static UserProfileDto ToDto(UserProfileModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new UserProfileDto
        {
            UserId = model.UserId.Value,
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            PreferredLanguage = model.PreferredLanguage,
            Created = model.Created,
            LastLogin = model.LastLogin,
            IsActive = model.IsActive,
            PartyRelations = model.PartyRelations.Select(ToDto).ToList(),
            Preferences = model.Preferences != null ? ToDto(model.Preferences) : null
        };
    }

    public static List<UserProfileDto> ToDto(List<UserProfileModel> models)
    {
        ArgumentNullException.ThrowIfNull(models);
        return models.Select(ToDto).ToList();
    }

    private static UserProfileDto.UserPartyRelationDto ToDto(UserProfileModel.UserPartyRelation model)
    {
        return new UserProfileDto.UserPartyRelationDto
        {
            PartyId = model.PartyId.Value,
            RelationType = ToDto(model.RelationType),
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            IsActive = model.IsActive
        };
    }

    private static UserProfileDto.UserPreferencesDto ToDto(UserProfileModel.UserPreferences model)
    {
        return new UserProfileDto.UserPreferencesDto
        {
            TimeZone = model.TimeZone,
            DateFormat = model.DateFormat,
            NumberFormat = model.NumberFormat,
            EmailNotifications = model.EmailNotifications,
            SmsNotifications = model.SmsNotifications,
            CustomSettings = model.CustomSettings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }

    private static UserPartyRelationTypeDto ToDto(UserPartyRelationType type)
    {
        return type switch
        {
            UserPartyRelationType.Employee => UserPartyRelationTypeDto.Employee,
            UserPartyRelationType.Representative => UserPartyRelationTypeDto.Representative,
            UserPartyRelationType.Owner => UserPartyRelationTypeDto.Owner,
            UserPartyRelationType.Authorized => UserPartyRelationTypeDto.Authorized,
            UserPartyRelationType.Contact => UserPartyRelationTypeDto.Contact,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown user party relation type")
        };
    }
}

/// <summary>
/// Maps between role DTOs and models
/// </summary>
public static class RoleApiMapper
{
    public static RoleDto ToDto(RoleModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new RoleDto
        {
            RoleCode = model.RoleCode,
            Name = model.Name,
            Description = model.Description,
            RoleType = ToDto(model.RoleType),
            IsDelegable = model.IsDelegable,
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            IsActive = model.IsActive,
            Rights = model.Rights.Select(ToDto).ToList(),
            RequiredRoles = model.RequiredRoles.ToList()
        };
    }

    public static List<RoleDto> ToDto(List<RoleModel> models)
    {
        ArgumentNullException.ThrowIfNull(models);
        return models.Select(ToDto).ToList();
    }

    public static RoleAssignmentDto ToDto(RoleAssignmentModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new RoleAssignmentDto
        {
            Id = model.Id,
            PartyId = model.PartyId.Value,
            UserId = model.UserId.Value,
            RoleCode = model.RoleCode,
            AssignedDate = model.AssignedDate,
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            AssignedByUserId = model.AssignedByUserId.Value,
            IsActive = model.IsActive
        };
    }

    public static List<RoleAssignmentDto> ToDto(List<RoleAssignmentModel> models)
    {
        ArgumentNullException.ThrowIfNull(models);
        return models.Select(ToDto).ToList();
    }

    private static RoleDto.RoleRightDto ToDto(RoleModel.RoleRight model)
    {
        return new RoleDto.RoleRightDto
        {
            Action = model.Action,
            Resource = model.Resource,
            Conditions = model.Conditions.Select(ToDto).ToList(),
            IsMandatory = model.IsMandatory
        };
    }

    private static AttributeMatchDto ToDto(AttributeMatch model)
    {
        return new AttributeMatchDto
        {
            Id = model.Id,
            Value = model.Value,
            Type = ToDto(model.Type),
            DataType = model.DataType
        };
    }

    private static RoleTypeDto ToDto(RoleType type)
    {
        return type switch
        {
            RoleType.System => RoleTypeDto.System,
            RoleType.Business => RoleTypeDto.Business,
            RoleType.Delegation => RoleTypeDto.Delegation,
            RoleType.AccessList => RoleTypeDto.AccessList,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown role type")
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

/// <summary>
/// Maps between main unit DTOs and models
/// </summary>
public static class MainUnitApiMapper
{
    public static MainUnitDto ToDto(MainUnitModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new MainUnitDto
        {
            OrganizationNumber = model.OrganizationNumber.Value,
            Name = model.Name,
            BusinessAddress = model.BusinessAddress,
            PostalAddress = model.PostalAddress,
            IndustryCode = model.IndustryCode,
            IndustryDescription = model.IndustryDescription,
            EmployeeCount = model.EmployeeCount,
            FoundedDate = model.FoundedDate,
            RegistrationDate = model.RegistrationDate,
            IsActive = model.IsActive,
            SubUnits = model.SubUnits.Select(ToDto).ToList(),
            CompanyDetails = model.CompanyDetails != null ? ToDto(model.CompanyDetails) : null
        };
    }

    public static List<MainUnitDto> ToDto(List<MainUnitModel> models)
    {
        ArgumentNullException.ThrowIfNull(models);
        return models.Select(ToDto).ToList();
    }

    private static MainUnitDto.SubUnitDto ToDto(MainUnitModel.SubUnit model)
    {
        return new MainUnitDto.SubUnitDto
        {
            OrganizationNumber = model.OrganizationNumber.Value,
            Name = model.Name,
            BusinessAddress = model.BusinessAddress,
            IsActive = model.IsActive,
            RegistrationDate = model.RegistrationDate
        };
    }

    private static MainUnitDto.CompanyDetailsDto ToDto(MainUnitModel.CompanyDetails model)
    {
        return new MainUnitDto.CompanyDetailsDto
        {
            CompanyForm = model.CompanyForm,
            ShareCapital = model.ShareCapital,
            Currency = model.Currency,
            LastAccountsDate = model.LastAccountsDate,
            BusinessCodes = model.BusinessCodes.ToList(),
            RegisterDetails = model.RegisterDetails.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }
}

#endregion

#region Persistence Mappers (Model ↔ Entity)

/// <summary>
/// Maps between party models and entities
/// </summary>
public static class PartyPersistenceMapper
{
    public static PartyEntity ToEntity(PartyModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new PartyEntity
        {
            PartyId = model.PartyId.Value,
            Name = model.Name,
            PartyType = (int)model.PartyType,
            OrganizationNumber = model.OrganizationNumber,
            PersonIdentifier = model.PersonIdentifier,
            IsDeleted = model.IsDeleted,
            Created = model.Created,
            Updated = model.Updated,
            Contacts = model.Contacts.Select(ToEntity).ToList(),
            Addresses = model.Addresses.Select(ToEntity).ToList(),
            Details = model.Details != null ? ToEntity(model.Details, model.PartyId.Value) : null
        };
    }

    public static PartyModel ToModel(PartyEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var model = entity.PartyType switch
        {
            (int)PartyType.Person => PartyModel.FromPersistence(
                new PartyId(entity.PartyId),
                entity.Name,
                (PartyType)entity.PartyType,
                entity.OrganizationNumber,
                entity.PersonIdentifier,
                entity.IsDeleted,
                entity.Created,
                entity.Updated
            ),
            (int)PartyType.Organization => PartyModel.FromPersistence(
                new PartyId(entity.PartyId),
                entity.Name,
                (PartyType)entity.PartyType,
                entity.OrganizationNumber,
                entity.PersonIdentifier,
                entity.IsDeleted,
                entity.Created,
                entity.Updated
            ),
            _ => PartyModel.FromPersistence(
                new PartyId(entity.PartyId),
                entity.Name,
                (PartyType)entity.PartyType,
                entity.OrganizationNumber,
                entity.PersonIdentifier,
                entity.IsDeleted,
                entity.Created,
                entity.Updated
            )
        };

        // Add contacts and addresses through the model methods
        foreach (var contact in entity.Contacts.Select(ToModel))
        {
            model.AddContact(contact);
        }

        foreach (var address in entity.Addresses.Select(ToModel))
        {
            model.AddAddress(address);
        }

        if (entity.Details != null)
        {
            model.SetDetails(ToModel(entity.Details));
        }

        return model;
    }

    public static List<PartyModel> ToModel(List<PartyEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        return entities.Select(ToModel).ToList();
    }

    public static PartyRelationshipEntity ToEntity(PartyRelationshipModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new PartyRelationshipEntity
        {
            Id = model.Id,
            FromPartyId = model.FromPartyId.Value,
            ToPartyId = model.ToPartyId.Value,
            RelationshipType = (int)model.RelationshipType,
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            IsActive = model.IsActive,
            Created = model.Created,
            Updated = model.Updated
        };
    }

    public static PartyRelationshipModel ToModel(PartyRelationshipEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return PartyRelationshipModel.FromPersistence(
            entity.Id,
            new PartyId(entity.FromPartyId),
            new PartyId(entity.ToPartyId),
            (RelationshipType)entity.RelationshipType,
            entity.ValidFrom,
            entity.ValidTo,
            entity.IsActive,
            entity.Created,
            entity.Updated
        );
    }

    private static PartyEntity.PartyContactEntity ToEntity(PartyModel.PartyContact model)
    {
        return new PartyEntity.PartyContactEntity
        {
            Id = Guid.NewGuid(),
            ContactType = (int)model.ContactType,
            Value = model.Value,
            IsPrimary = model.IsPrimary
        };
    }

    private static PartyModel.PartyContact ToModel(PartyEntity.PartyContactEntity entity)
    {
        return new PartyModel.PartyContact(
            (ContactType)entity.ContactType,
            entity.Value,
            entity.IsPrimary
        );
    }

    private static PartyEntity.PartyAddressEntity ToEntity(PartyModel.PartyAddress model)
    {
        return new PartyEntity.PartyAddressEntity
        {
            Id = Guid.NewGuid(),
            AddressType = (int)model.AddressType,
            StreetAddress = model.StreetAddress,
            PostalCode = model.PostalCode,
            City = model.City,
            Country = model.Country,
            IsPrimary = model.IsPrimary
        };
    }

    private static PartyModel.PartyAddress ToModel(PartyEntity.PartyAddressEntity entity)
    {
        return new PartyModel.PartyAddress(
            (AddressType)entity.AddressType,
            entity.StreetAddress,
            entity.PostalCode,
            entity.City,
            entity.Country,
            entity.IsPrimary
        );
    }

    private static PartyEntity.PartyDetailsEntity ToEntity(PartyModel.PartyDetails model, int partyId)
    {
        return new PartyEntity.PartyDetailsEntity
        {
            PartyId = partyId,
            Description = model.Description,
            Website = model.Website,
            Industry = model.Industry,
            EmployeeCount = model.EmployeeCount,
            FoundedDate = model.FoundedDate,
            CustomAttributes = JsonSerializer.Serialize(model.CustomAttributes)
        };
    }

    private static PartyModel.PartyDetails ToModel(PartyEntity.PartyDetailsEntity entity)
    {
        var customAttributes = string.IsNullOrEmpty(entity.CustomAttributes)
            ? new Dictionary<string, string>()
            : JsonSerializer.Deserialize<Dictionary<string, string>>(entity.CustomAttributes) ?? new Dictionary<string, string>();

        return new PartyModel.PartyDetails(
            entity.Description,
            entity.Website,
            entity.Industry,
            entity.EmployeeCount,
            entity.FoundedDate,
            customAttributes
        );
    }
}

/// <summary>
/// Maps between user profile models and entities
/// </summary>
public static class UserProfilePersistenceMapper
{
    public static UserProfileEntity ToEntity(UserProfileModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new UserProfileEntity
        {
            UserId = model.UserId.Value,
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            PreferredLanguage = model.PreferredLanguage,
            Created = model.Created,
            LastLogin = model.LastLogin,
            IsActive = model.IsActive,
            PartyRelations = model.PartyRelations.Select(ToEntity).ToList(),
            Preferences = model.Preferences != null ? ToEntity(model.Preferences, model.UserId.Value) : null
        };
    }

    public static UserProfileModel ToModel(UserProfileEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var model = UserProfileModel.FromPersistence(
            new UserId(entity.UserId),
            entity.UserName,
            entity.Email,
            entity.PhoneNumber,
            entity.PreferredLanguage,
            entity.Created,
            entity.LastLogin,
            entity.IsActive
        );

        foreach (var relation in entity.PartyRelations.Select(ToModel))
        {
            model.AddPartyRelation(relation);
        }

        if (entity.Preferences != null)
        {
            model.SetPreferences(ToModel(entity.Preferences));
        }

        return model;
    }

    private static UserProfileEntity.UserPartyRelationEntity ToEntity(UserProfileModel.UserPartyRelation model)
    {
        return new UserProfileEntity.UserPartyRelationEntity
        {
            Id = Guid.NewGuid(),
            PartyId = model.PartyId.Value,
            RelationType = (int)model.RelationType,
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            IsActive = model.IsActive,
            Created = model.Created
        };
    }

    private static UserProfileModel.UserPartyRelation ToModel(UserProfileEntity.UserPartyRelationEntity entity)
    {
        return UserProfileModel.UserPartyRelation.FromPersistence(
            new PartyId(entity.PartyId),
            (UserPartyRelationType)entity.RelationType,
            entity.ValidFrom,
            entity.ValidTo,
            entity.IsActive,
            entity.Created
        );
    }

    private static UserProfileEntity.UserPreferencesEntity ToEntity(UserProfileModel.UserPreferences model, int userId)
    {
        return new UserProfileEntity.UserPreferencesEntity
        {
            UserId = userId,
            TimeZone = model.TimeZone,
            DateFormat = model.DateFormat,
            NumberFormat = model.NumberFormat,
            EmailNotifications = model.EmailNotifications,
            SmsNotifications = model.SmsNotifications,
            CustomSettings = JsonSerializer.Serialize(model.CustomSettings)
        };
    }

    private static UserProfileModel.UserPreferences ToModel(UserProfileEntity.UserPreferencesEntity entity)
    {
        var customSettings = string.IsNullOrEmpty(entity.CustomSettings)
            ? new Dictionary<string, object>()
            : JsonSerializer.Deserialize<Dictionary<string, object>>(entity.CustomSettings) ?? new Dictionary<string, object>();

        return new UserProfileModel.UserPreferences(
            entity.TimeZone,
            entity.DateFormat,
            entity.NumberFormat,
            entity.EmailNotifications,
            entity.SmsNotifications,
            customSettings
        );
    }
}

/// <summary>
/// Maps between role models and entities
/// </summary>
public static class RolePersistenceMapper
{
    public static RoleEntity ToEntity(RoleModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new RoleEntity
        {
            RoleCode = model.RoleCode,
            Name = model.Name,
            Description = model.Description,
            RoleType = (int)model.RoleType,
            IsDelegable = model.IsDelegable,
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            IsActive = model.IsActive,
            Created = model.Created,
            Updated = model.Updated,
            Rights = model.Rights.Select(ToEntity).ToList(),
            RequiredRoles = model.RequiredRoles.Select(role => new RoleEntity.RoleRequiredRoleEntity
            {
                Id = Guid.NewGuid(),
                RequiredRoleCode = role
            }).ToList()
        };
    }

    public static RoleModel ToModel(RoleEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var model = RoleModel.FromPersistence(
            entity.RoleCode,
            entity.Name,
            (RoleType)entity.RoleType,
            entity.Description,
            entity.IsDelegable,
            entity.ValidFrom,
            entity.ValidTo,
            entity.IsActive,
            entity.Created,
            entity.Updated
        );

        foreach (var right in entity.Rights.Select(ToModel))
        {
            model.AddRight(right);
        }

        foreach (var requiredRole in entity.RequiredRoles.Select(r => r.RequiredRoleCode))
        {
            model.AddRequiredRole(requiredRole);
        }

        return model;
    }

    public static RoleAssignmentEntity ToEntity(RoleAssignmentModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new RoleAssignmentEntity
        {
            Id = model.Id,
            PartyId = model.PartyId.Value,
            UserId = model.UserId.Value,
            RoleCode = model.RoleCode,
            AssignedDate = model.AssignedDate,
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo,
            AssignedByUserId = model.AssignedByUserId.Value,
            IsActive = model.IsActive
        };
    }

    public static RoleAssignmentModel ToModel(RoleAssignmentEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return RoleAssignmentModel.FromPersistence(
            entity.Id,
            new PartyId(entity.PartyId),
            new UserId(entity.UserId),
            entity.RoleCode,
            new UserId(entity.AssignedByUserId),
            entity.AssignedDate,
            entity.ValidFrom,
            entity.ValidTo,
            entity.IsActive
        );
    }

    private static RoleEntity.RoleRightEntity ToEntity(RoleModel.RoleRight model)
    {
        return new RoleEntity.RoleRightEntity
        {
            Id = Guid.NewGuid(),
            Action = model.Action,
            Resource = model.Resource,
            IsMandatory = model.IsMandatory,
            Conditions = model.Conditions.Select(ToEntity).ToList()
        };
    }

    private static RoleModel.RoleRight ToModel(RoleEntity.RoleRightEntity entity)
    {
        return new RoleModel.RoleRight(
            entity.Action,
            entity.Resource,
            entity.Conditions.Select(ToModel).ToList(),
            entity.IsMandatory
        );
    }

    private static RoleEntity.RoleRightEntity.RoleRightConditionEntity ToEntity(AttributeMatch model)
    {
        return new RoleEntity.RoleRightEntity.RoleRightConditionEntity
        {
            Id = Guid.NewGuid(),
            AttributeId = model.Id,
            AttributeValue = model.Value,
            MatchType = (int)model.Type,
            DataType = model.DataType
        };
    }

    private static AttributeMatch ToModel(RoleEntity.RoleRightEntity.RoleRightConditionEntity entity)
    {
        return new AttributeMatch(
            entity.AttributeId,
            entity.AttributeValue,
            (AttributeMatchType)entity.MatchType,
            entity.DataType
        );
    }
}

/// <summary>
/// Maps between main unit models and entities
/// </summary>
public static class MainUnitPersistenceMapper
{
    public static MainUnitEntity ToEntity(MainUnitModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return new MainUnitEntity
        {
            OrganizationNumber = model.OrganizationNumber.Value,
            Name = model.Name,
            BusinessAddress = model.BusinessAddress,
            PostalAddress = model.PostalAddress,
            IndustryCode = model.IndustryCode,
            IndustryDescription = model.IndustryDescription,
            EmployeeCount = model.EmployeeCount,
            FoundedDate = model.FoundedDate,
            RegistrationDate = model.RegistrationDate,
            IsActive = model.IsActive,
            SubUnits = model.SubUnits.Select(ToEntity).ToList(),
            CompanyDetails = model.CompanyDetails != null ? ToEntity(model.CompanyDetails, model.OrganizationNumber.Value) : null
        };
    }

    public static MainUnitModel ToModel(MainUnitEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var model = MainUnitModel.FromPersistence(
            new OrganizationNumber(entity.OrganizationNumber),
            entity.Name,
            entity.RegistrationDate,
            entity.BusinessAddress,
            entity.PostalAddress,
            entity.IndustryCode,
            entity.IndustryDescription,
            entity.EmployeeCount,
            entity.FoundedDate,
            entity.IsActive
        );

        foreach (var subUnit in entity.SubUnits.Select(ToModel))
        {
            model.AddSubUnit(subUnit);
        }

        if (entity.CompanyDetails != null)
        {
            model.SetCompanyDetails(ToModel(entity.CompanyDetails));
        }

        return model;
    }

    private static MainUnitEntity.SubUnitEntity ToEntity(MainUnitModel.SubUnit model)
    {
        return new MainUnitEntity.SubUnitEntity
        {
            OrganizationNumber = model.OrganizationNumber.Value,
            Name = model.Name,
            BusinessAddress = model.BusinessAddress,
            IsActive = model.IsActive,
            RegistrationDate = model.RegistrationDate
        };
    }

    private static MainUnitModel.SubUnit ToModel(MainUnitEntity.SubUnitEntity entity)
    {
        return MainUnitModel.SubUnit.FromPersistence(
            new OrganizationNumber(entity.OrganizationNumber),
            entity.Name,
            entity.BusinessAddress,
            entity.RegistrationDate,
            entity.IsActive
        );
    }

    private static MainUnitEntity.CompanyDetailsEntity ToEntity(MainUnitModel.CompanyDetails model, string organizationNumber)
    {
        return new MainUnitEntity.CompanyDetailsEntity
        {
            OrganizationNumber = organizationNumber,
            CompanyForm = model.CompanyForm,
            ShareCapital = model.ShareCapital,
            Currency = model.Currency,
            LastAccountsDate = model.LastAccountsDate,
            BusinessCodes = JsonSerializer.Serialize(model.BusinessCodes),
            RegisterDetails = JsonSerializer.Serialize(model.RegisterDetails)
        };
    }

    private static MainUnitModel.CompanyDetails ToModel(MainUnitEntity.CompanyDetailsEntity entity)
    {
        var businessCodes = string.IsNullOrEmpty(entity.BusinessCodes)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(entity.BusinessCodes) ?? new List<string>();

        var registerDetails = string.IsNullOrEmpty(entity.RegisterDetails)
            ? new Dictionary<string, string>()
            : JsonSerializer.Deserialize<Dictionary<string, string>>(entity.RegisterDetails) ?? new Dictionary<string, string>();

        return new MainUnitModel.CompanyDetails(
            entity.CompanyForm,
            entity.ShareCapital,
            entity.Currency,
            entity.LastAccountsDate,
            businessCodes,
            registerDetails
        );
    }
}

#endregion

#region Extension Methods for Fluent API

/// <summary>
/// Extension methods for party mapping
/// </summary>
public static class PartyMappingExtensions
{
    // Core → API
    public static RegisterPartyDto ToDto(this PartyModel model)
        => PartyApiMapper.ToDto(model);

    public static PartyRelationshipDto ToDto(this PartyRelationshipModel model)
        => PartyApiMapper.ToDto(model);

    // Core → Database
    public static PartyEntity ToEntity(this PartyModel model)
        => PartyPersistenceMapper.ToEntity(model);

    public static PartyRelationshipEntity ToEntity(this PartyRelationshipModel model)
        => PartyPersistenceMapper.ToEntity(model);

    // Database → Core
    public static PartyModel ToModel(this PartyEntity entity)
        => PartyPersistenceMapper.ToModel(entity);

    public static PartyRelationshipModel ToModel(this PartyRelationshipEntity entity)
        => PartyPersistenceMapper.ToModel(entity);

    // Collections
    public static List<RegisterPartyDto> ToDto(this List<PartyModel> models)
        => PartyApiMapper.ToDto(models);

    public static List<PartyModel> ToModel(this List<PartyEntity> entities)
        => PartyPersistenceMapper.ToModel(entities);
}

/// <summary>
/// Extension methods for user profile mapping
/// </summary>
public static class UserProfileMappingExtensions
{
    // Core → API
    public static UserProfileDto ToDto(this UserProfileModel model)
        => UserProfileApiMapper.ToDto(model);

    // Core → Database
    public static UserProfileEntity ToEntity(this UserProfileModel model)
        => UserProfilePersistenceMapper.ToEntity(model);

    // Database → Core
    public static UserProfileModel ToModel(this UserProfileEntity entity)
        => UserProfilePersistenceMapper.ToModel(entity);

    // Collections
    public static List<UserProfileDto> ToDto(this List<UserProfileModel> models)
        => UserProfileApiMapper.ToDto(models);
}

/// <summary>
/// Extension methods for role mapping
/// </summary>
public static class RoleMappingExtensions
{
    // Core → API
    public static RoleDto ToDto(this RoleModel model)
        => RoleApiMapper.ToDto(model);

    public static RoleAssignmentDto ToDto(this RoleAssignmentModel model)
        => RoleApiMapper.ToDto(model);

    // Core → Database
    public static RoleEntity ToEntity(this RoleModel model)
        => RolePersistenceMapper.ToEntity(model);

    public static RoleAssignmentEntity ToEntity(this RoleAssignmentModel model)
        => RolePersistenceMapper.ToEntity(model);

    // Database → Core
    public static RoleModel ToModel(this RoleEntity entity)
        => RolePersistenceMapper.ToModel(entity);

    public static RoleAssignmentModel ToModel(this RoleAssignmentEntity entity)
        => RolePersistenceMapper.ToModel(entity);

    // Collections
    public static List<RoleDto> ToDto(this List<RoleModel> models)
        => RoleApiMapper.ToDto(models);

    public static List<RoleAssignmentDto> ToDto(this List<RoleAssignmentModel> models)
        => RoleApiMapper.ToDto(models);
}

/// <summary>
/// Extension methods for main unit mapping
/// </summary>
public static class MainUnitMappingExtensions
{
    // Core → API
    public static MainUnitDto ToDto(this MainUnitModel model)
        => MainUnitApiMapper.ToDto(model);

    // Core → Database
    public static MainUnitEntity ToEntity(this MainUnitModel model)
        => MainUnitPersistenceMapper.ToEntity(model);

    // Database → Core
    public static MainUnitModel ToModel(this MainUnitEntity entity)
        => MainUnitPersistenceMapper.ToModel(entity);

    // Collections
    public static List<MainUnitDto> ToDto(this List<MainUnitModel> models)
        => MainUnitApiMapper.ToDto(models);
}

#endregion