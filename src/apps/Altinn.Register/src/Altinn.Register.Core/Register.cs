using Altinn.Authorization.Shared;

namespace Altinn.Register.Core.Models;

#region Party Models

/// <summary>
/// Party core model representing individuals and organizations
/// </summary>
public class PartyModel
{
    public PartyId PartyId { get; private set; }
    public string Name { get; private set; }
    public PartyType PartyType { get; private set; }
    public string? OrganizationNumber { get; private set; }
    public string? PersonIdentifier { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime Created { get; private set; }
    public DateTime? Updated { get; private set; }
    public List<PartyContact> Contacts { get; private set; }
    public List<PartyAddress> Addresses { get; private set; }
    public PartyDetails? Details { get; private set; }

    private PartyModel()
    {
        Contacts = new List<PartyContact>();
        Addresses = new List<PartyAddress>();
    }

    public PartyModel(
        PartyId partyId,
        string name,
        PartyType partyType,
        string? organizationNumber = null,
        string? personIdentifier = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        ValidatePartyTypeAndIdentifiers(partyType, organizationNumber, personIdentifier);

        PartyId = partyId;
        Name = name;
        PartyType = partyType;
        OrganizationNumber = organizationNumber;
        PersonIdentifier = personIdentifier;
        Created = DateTime.UtcNow;
        Contacts = new List<PartyContact>();
        Addresses = new List<PartyAddress>();
    }

    public static PartyModel CreatePerson(PartyId partyId, string name, string personIdentifier)
    {
        if (string.IsNullOrWhiteSpace(personIdentifier))
            throw new ArgumentException("Person identifier is required for persons", nameof(personIdentifier));

        return new PartyModel(partyId, name, PartyType.Person, personIdentifier: personIdentifier);
    }

    public static PartyModel CreateOrganization(PartyId partyId, string name, string organizationNumber)
    {
        if (string.IsNullOrWhiteSpace(organizationNumber))
            throw new ArgumentException("Organization number is required for organizations", nameof(organizationNumber));

        return new PartyModel(partyId, name, PartyType.Organization, organizationNumber: organizationNumber);
    }

    public static PartyModel CreateSubUnit(PartyId partyId, string name, string organizationNumber)
    {
        if (string.IsNullOrWhiteSpace(organizationNumber))
            throw new ArgumentException("Organization number is required for sub units", nameof(organizationNumber));

        return new PartyModel(partyId, name, PartyType.SubUnit, organizationNumber: organizationNumber);
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name cannot be null or empty", nameof(newName));

        Name = newName;
        Updated = DateTime.UtcNow;
    }

    public void AddContact(PartyContact contact)
    {
        if (Contacts.Any(c => c.ContactType == contact.ContactType && c.IsPrimary && contact.IsPrimary))
            throw new InvalidOperationException($"A primary {contact.ContactType} contact already exists");

        Contacts.Add(contact);
        Updated = DateTime.UtcNow;
    }

    public void RemoveContact(ContactType contactType, string value)
    {
        var contact = Contacts.FirstOrDefault(c => c.ContactType == contactType && c.Value == value);
        if (contact != null)
        {
            Contacts.Remove(contact);
            Updated = DateTime.UtcNow;
        }
    }

    public void AddAddress(PartyAddress address)
    {
        if (Addresses.Any(a => a.AddressType == address.AddressType && a.IsPrimary && address.IsPrimary))
            throw new InvalidOperationException($"A primary {address.AddressType} address already exists");

        Addresses.Add(address);
        Updated = DateTime.UtcNow;
    }

    public void RemoveAddress(AddressType addressType)
    {
        var address = Addresses.FirstOrDefault(a => a.AddressType == addressType);
        if (address != null)
        {
            Addresses.Remove(address);
            Updated = DateTime.UtcNow;
        }
    }

    public void SetDetails(PartyDetails details)
    {
        Details = details;
        Updated = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        Updated = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        Updated = DateTime.UtcNow;
    }

    public PartyContact? GetPrimaryContact(ContactType contactType)
    {
        return Contacts.FirstOrDefault(c => c.ContactType == contactType && c.IsPrimary);
    }

    public PartyAddress? GetPrimaryAddress(AddressType addressType)
    {
        return Addresses.FirstOrDefault(a => a.AddressType == addressType && a.IsPrimary);
    }

    private static void ValidatePartyTypeAndIdentifiers(PartyType partyType, string? organizationNumber, string? personIdentifier)
    {
        switch (partyType)
        {
            case PartyType.Person when string.IsNullOrWhiteSpace(personIdentifier):
                throw new ArgumentException("Person identifier is required for persons");
            case PartyType.Organization when string.IsNullOrWhiteSpace(organizationNumber):
                throw new ArgumentException("Organization number is required for organizations");
            case PartyType.SubUnit when string.IsNullOrWhiteSpace(organizationNumber):
                throw new ArgumentException("Organization number is required for sub units");
        }
    }

    /// <summary>
    /// Party contact information value object
    /// </summary>
    public class PartyContact
    {
        public ContactType ContactType { get; private set; }
        public string Value { get; private set; }
        public bool IsPrimary { get; private set; }

        private PartyContact() { }

        public PartyContact(ContactType contactType, string value, bool isPrimary = false)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Contact value cannot be null or empty", nameof(value));

            ContactType = contactType;
            Value = value;
            IsPrimary = isPrimary;
        }

        public void SetAsPrimary()
        {
            IsPrimary = true;
        }

        public void UnsetAsPrimary()
        {
            IsPrimary = false;
        }
    }

    /// <summary>
    /// Party address information value object
    /// </summary>
    public class PartyAddress
    {
        public AddressType AddressType { get; private set; }
        public string? StreetAddress { get; private set; }
        public string? PostalCode { get; private set; }
        public string? City { get; private set; }
        public string? Country { get; private set; }
        public bool IsPrimary { get; private set; }

        private PartyAddress() { }

        public PartyAddress(
            AddressType addressType,
            string? streetAddress = null,
            string? postalCode = null,
            string? city = null,
            string? country = null,
            bool isPrimary = false)
        {
            AddressType = addressType;
            StreetAddress = streetAddress;
            PostalCode = postalCode;
            City = city;
            Country = country;
            IsPrimary = isPrimary;
        }

        public void UpdateAddress(string? streetAddress, string? postalCode, string? city, string? country)
        {
            StreetAddress = streetAddress;
            PostalCode = postalCode;
            City = city;
            Country = country;
        }

        public void SetAsPrimary()
        {
            IsPrimary = true;
        }

        public void UnsetAsPrimary()
        {
            IsPrimary = false;
        }

        public string GetFormattedAddress()
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(StreetAddress)) parts.Add(StreetAddress);
            if (!string.IsNullOrWhiteSpace(PostalCode)) parts.Add(PostalCode);
            if (!string.IsNullOrWhiteSpace(City)) parts.Add(City);
            if (!string.IsNullOrWhiteSpace(Country)) parts.Add(Country);
            return string.Join(", ", parts);
        }
    }

    /// <summary>
    /// Additional party details value object
    /// </summary>
    public class PartyDetails
    {
        public string? Description { get; private set; }
        public string? Website { get; private set; }
        public string? Industry { get; private set; }
        public int? EmployeeCount { get; private set; }
        public DateTime? FoundedDate { get; private set; }
        public Dictionary<string, string> CustomAttributes { get; private set; }

        private PartyDetails()
        {
            CustomAttributes = new Dictionary<string, string>();
        }

        public PartyDetails(
            string? description = null,
            string? website = null,
            string? industry = null,
            int? employeeCount = null,
            DateTime? foundedDate = null,
            Dictionary<string, string>? customAttributes = null)
        {
            Description = description;
            Website = website;
            Industry = industry;
            EmployeeCount = employeeCount;
            FoundedDate = foundedDate;
            CustomAttributes = customAttributes ?? new Dictionary<string, string>();
        }

        public void UpdateDescription(string? description)
        {
            Description = description;
        }

        public void UpdateWebsite(string? website)
        {
            Website = website;
        }

        public void UpdateIndustry(string? industry)
        {
            Industry = industry;
        }

        public void UpdateEmployeeCount(int? employeeCount)
        {
            EmployeeCount = employeeCount;
        }

        public void UpdateFoundedDate(DateTime? foundedDate)
        {
            FoundedDate = foundedDate;
        }

        public void SetCustomAttribute(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            CustomAttributes[key] = value;
        }

        public void RemoveCustomAttribute(string key)
        {
            CustomAttributes.Remove(key);
        }

        public string? GetCustomAttribute(string key)
        {
            return CustomAttributes.TryGetValue(key, out var value) ? value : null;
        }
    }
}

/// <summary>
/// Party relationship model
/// </summary>
public class PartyRelationshipModel
{
    public Guid Id { get; private set; }
    public PartyId FromPartyId { get; private set; }
    public PartyId ToPartyId { get; private set; }
    public RelationshipType RelationshipType { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime Created { get; private set; }
    public DateTime? Updated { get; private set; }

    private PartyRelationshipModel() { }

    public PartyRelationshipModel(
        PartyId fromPartyId,
        PartyId toPartyId,
        RelationshipType relationshipType,
        DateTime? validFrom = null,
        DateTime? validTo = null)
    {
        if (fromPartyId == toPartyId)
            throw new ArgumentException("Party cannot have relationship with itself");

        Id = Guid.NewGuid();
        FromPartyId = fromPartyId;
        ToPartyId = toPartyId;
        RelationshipType = relationshipType;
        ValidFrom = validFrom ?? DateTime.UtcNow;
        ValidTo = validTo;
        IsActive = true;
        Created = DateTime.UtcNow;
    }

    public void UpdateValidityPeriod(DateTime validFrom, DateTime? validTo = null)
    {
        if (validTo.HasValue && validTo <= validFrom)
            throw new ArgumentException("ValidTo must be after ValidFrom");

        ValidFrom = validFrom;
        ValidTo = validTo;
        Updated = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        Updated = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        Updated = DateTime.UtcNow;
    }

    public bool IsValidAt(DateTime checkDate)
    {
        return IsActive && 
               ValidFrom <= checkDate && 
               (!ValidTo.HasValue || ValidTo.Value >= checkDate);
    }
}

#endregion

#region User Models

/// <summary>
/// User profile model
/// </summary>
public class UserProfileModel
{
    public UserId UserId { get; private set; }
    public string UserName { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? PreferredLanguage { get; private set; }
    public DateTime Created { get; private set; }
    public DateTime? LastLogin { get; private set; }
    public bool IsActive { get; private set; }
    public List<UserPartyRelation> PartyRelations { get; private set; }
    public UserPreferences? Preferences { get; private set; }

    private UserProfileModel()
    {
        PartyRelations = new List<UserPartyRelation>();
    }

    public UserProfileModel(
        UserId userId,
        string userName,
        string? email = null,
        string? phoneNumber = null,
        string? preferredLanguage = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be null or empty", nameof(userName));

        UserId = userId;
        UserName = userName;
        Email = email;
        PhoneNumber = phoneNumber;
        PreferredLanguage = preferredLanguage;
        Created = DateTime.UtcNow;
        IsActive = true;
        PartyRelations = new List<UserPartyRelation>();
    }

    public void UpdateProfile(string? email, string? phoneNumber, string? preferredLanguage)
    {
        Email = email;
        PhoneNumber = phoneNumber;
        PreferredLanguage = preferredLanguage;
    }

    public void UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void AddPartyRelation(UserPartyRelation relation)
    {
        if (PartyRelations.Any(r => r.PartyId == relation.PartyId && r.RelationType == relation.RelationType && r.IsActive))
            throw new InvalidOperationException($"Active {relation.RelationType} relation with party {relation.PartyId} already exists");

        PartyRelations.Add(relation);
    }

    public void RemovePartyRelation(PartyId partyId, UserPartyRelationType relationType)
    {
        var relation = PartyRelations.FirstOrDefault(r => r.PartyId == partyId && r.RelationType == relationType);
        if (relation != null)
        {
            relation.Deactivate();
        }
    }

    public void SetPreferences(UserPreferences preferences)
    {
        Preferences = preferences;
    }

    public List<UserPartyRelation> GetActivePartyRelations()
    {
        return PartyRelations.Where(r => r.IsActive && r.IsValidAt(DateTime.UtcNow)).ToList();
    }

    /// <summary>
    /// User-party relationship value object
    /// </summary>
    public class UserPartyRelation
    {
        public PartyId PartyId { get; private set; }
        public UserPartyRelationType RelationType { get; private set; }
        public DateTime ValidFrom { get; private set; }
        public DateTime? ValidTo { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime Created { get; private set; }

        private UserPartyRelation() { }

        public UserPartyRelation(
            PartyId partyId,
            UserPartyRelationType relationType,
            DateTime? validFrom = null,
            DateTime? validTo = null)
        {
            PartyId = partyId;
            RelationType = relationType;
            ValidFrom = validFrom ?? DateTime.UtcNow;
            ValidTo = validTo;
            IsActive = true;
            Created = DateTime.UtcNow;
        }

        public void UpdateValidityPeriod(DateTime validFrom, DateTime? validTo = null)
        {
            if (validTo.HasValue && validTo <= validFrom)
                throw new ArgumentException("ValidTo must be after ValidFrom");

            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public bool IsValidAt(DateTime checkDate)
        {
            return IsActive && 
                   ValidFrom <= checkDate && 
                   (!ValidTo.HasValue || ValidTo.Value >= checkDate);
        }
    }

    /// <summary>
    /// User preferences value object
    /// </summary>
    public class UserPreferences
    {
        public string? TimeZone { get; private set; }
        public string? DateFormat { get; private set; }
        public string? NumberFormat { get; private set; }
        public bool EmailNotifications { get; private set; }
        public bool SmsNotifications { get; private set; }
        public Dictionary<string, object> CustomSettings { get; private set; }

        private UserPreferences()
        {
            CustomSettings = new Dictionary<string, object>();
        }

        public UserPreferences(
            string? timeZone = null,
            string? dateFormat = null,
            string? numberFormat = null,
            bool emailNotifications = true,
            bool smsNotifications = false,
            Dictionary<string, object>? customSettings = null)
        {
            TimeZone = timeZone;
            DateFormat = dateFormat;
            NumberFormat = numberFormat;
            EmailNotifications = emailNotifications;
            SmsNotifications = smsNotifications;
            CustomSettings = customSettings ?? new Dictionary<string, object>();
        }

        public void UpdateNotificationSettings(bool emailNotifications, bool smsNotifications)
        {
            EmailNotifications = emailNotifications;
            SmsNotifications = smsNotifications;
        }

        public void UpdateLocalizationSettings(string? timeZone, string? dateFormat, string? numberFormat)
        {
            TimeZone = timeZone;
            DateFormat = dateFormat;
            NumberFormat = numberFormat;
        }

        public void SetCustomSetting(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            CustomSettings[key] = value;
        }

        public void RemoveCustomSetting(string key)
        {
            CustomSettings.Remove(key);
        }

        public T? GetCustomSetting<T>(string key)
        {
            if (CustomSettings.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return default;
        }
    }
}

#endregion

#region Role Models

/// <summary>
/// Role definition model
/// </summary>
public class RoleModel
{
    public string RoleCode { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public RoleType RoleType { get; private set; }
    public bool IsDelegable { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public bool IsActive { get; private set; }
    public List<RoleRight> Rights { get; private set; }
    public List<string> RequiredRoles { get; private set; }
    public DateTime Created { get; private set; }
    public DateTime? Updated { get; private set; }

    private RoleModel()
    {
        Rights = new List<RoleRight>();
        RequiredRoles = new List<string>();
    }

    public RoleModel(
        string roleCode,
        string name,
        RoleType roleType,
        string? description = null,
        bool isDelegable = true,
        DateTime? validFrom = null,
        DateTime? validTo = null)
    {
        if (string.IsNullOrWhiteSpace(roleCode))
            throw new ArgumentException("Role code cannot be null or empty", nameof(roleCode));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        RoleCode = roleCode;
        Name = name;
        Description = description;
        RoleType = roleType;
        IsDelegable = isDelegable;
        ValidFrom = validFrom ?? DateTime.UtcNow;
        ValidTo = validTo;
        IsActive = true;
        Rights = new List<RoleRight>();
        RequiredRoles = new List<string>();
        Created = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string? description, bool isDelegable)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        Name = name;
        Description = description;
        IsDelegable = isDelegable;
        Updated = DateTime.UtcNow;
    }

    public void UpdateValidityPeriod(DateTime validFrom, DateTime? validTo = null)
    {
        if (validTo.HasValue && validTo <= validFrom)
            throw new ArgumentException("ValidTo must be after ValidFrom");

        ValidFrom = validFrom;
        ValidTo = validTo;
        Updated = DateTime.UtcNow;
    }

    public void AddRight(RoleRight right)
    {
        if (Rights.Any(r => r.Action == right.Action && r.Resource == right.Resource))
            throw new InvalidOperationException($"Right for action '{right.Action}' on resource '{right.Resource}' already exists");

        Rights.Add(right);
        Updated = DateTime.UtcNow;
    }

    public void RemoveRight(string action, string resource)
    {
        var right = Rights.FirstOrDefault(r => r.Action == action && r.Resource == resource);
        if (right != null)
        {
            Rights.Remove(right);
            Updated = DateTime.UtcNow;
        }
    }

    public void AddRequiredRole(string roleCode)
    {
        if (string.IsNullOrWhiteSpace(roleCode))
            throw new ArgumentException("Role code cannot be null or empty", nameof(roleCode));

        if (roleCode == RoleCode)
            throw new ArgumentException("Role cannot require itself");

        if (!RequiredRoles.Contains(roleCode))
        {
            RequiredRoles.Add(roleCode);
            Updated = DateTime.UtcNow;
        }
    }

    public void RemoveRequiredRole(string roleCode)
    {
        if (RequiredRoles.Remove(roleCode))
        {
            Updated = DateTime.UtcNow;
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        Updated = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        Updated = DateTime.UtcNow;
    }

    public bool IsValidAt(DateTime checkDate)
    {
        return IsActive && 
               ValidFrom <= checkDate && 
               (!ValidTo.HasValue || ValidTo.Value >= checkDate);
    }

    /// <summary>
    /// Role right value object
    /// </summary>
    public class RoleRight
    {
        public string Action { get; private set; }
        public string Resource { get; private set; }
        public List<AttributeMatch> Conditions { get; private set; }
        public bool IsMandatory { get; private set; }

        private RoleRight()
        {
            Conditions = new List<AttributeMatch>();
        }

        public RoleRight(
            string action,
            string resource,
            List<AttributeMatch>? conditions = null,
            bool isMandatory = false)
        {
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action cannot be null or empty", nameof(action));
            if (string.IsNullOrWhiteSpace(resource))
                throw new ArgumentException("Resource cannot be null or empty", nameof(resource));

            Action = action;
            Resource = resource;
            Conditions = conditions ?? new List<AttributeMatch>();
            IsMandatory = isMandatory;
        }

        public void AddCondition(AttributeMatch condition)
        {
            if (Conditions.Any(c => c.Id == condition.Id))
                throw new InvalidOperationException($"Condition with ID {condition.Id} already exists");

            Conditions.Add(condition);
        }

        public void RemoveCondition(string conditionId)
        {
            var condition = Conditions.FirstOrDefault(c => c.Id == conditionId);
            if (condition != null)
            {
                Conditions.Remove(condition);
            }
        }

        public void SetMandatory(bool isMandatory)
        {
            IsMandatory = isMandatory;
        }
    }
}

/// <summary>
/// Role assignment model
/// </summary>
public class RoleAssignmentModel
{
    public Guid Id { get; private set; }
    public PartyId PartyId { get; private set; }
    public UserId UserId { get; private set; }
    public string RoleCode { get; private set; }
    public DateTime AssignedDate { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public UserId AssignedByUserId { get; private set; }
    public bool IsActive { get; private set; }

    private RoleAssignmentModel() { }

    public RoleAssignmentModel(
        PartyId partyId,
        UserId userId,
        string roleCode,
        UserId assignedByUserId,
        DateTime? validFrom = null,
        DateTime? validTo = null)
    {
        if (string.IsNullOrWhiteSpace(roleCode))
            throw new ArgumentException("Role code cannot be null or empty", nameof(roleCode));

        if (validTo.HasValue && validFrom.HasValue && validTo <= validFrom)
            throw new ArgumentException("ValidTo must be after ValidFrom");

        Id = Guid.NewGuid();
        PartyId = partyId;
        UserId = userId;
        RoleCode = roleCode;
        AssignedByUserId = assignedByUserId;
        AssignedDate = DateTime.UtcNow;
        ValidFrom = validFrom;
        ValidTo = validTo;
        IsActive = true;
    }

    public void UpdateValidityPeriod(DateTime? validFrom, DateTime? validTo = null)
    {
        if (validTo.HasValue && validFrom.HasValue && validTo <= validFrom)
            throw new ArgumentException("ValidTo must be after ValidFrom");

        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public bool IsValidAt(DateTime checkDate)
    {
        return IsActive && 
               (!ValidFrom.HasValue || ValidFrom <= checkDate) && 
               (!ValidTo.HasValue || ValidTo >= checkDate);
    }
}

#endregion

#region Main Unit Models

/// <summary>
/// Main unit model for organizations
/// </summary>
public class MainUnitModel
{
    public OrganizationNumber OrganizationNumber { get; private set; }
    public string Name { get; private set; }
    public string? BusinessAddress { get; private set; }
    public string? PostalAddress { get; private set; }
    public string? IndustryCode { get; private set; }
    public string? IndustryDescription { get; private set; }
    public int? EmployeeCount { get; private set; }
    public DateTime? FoundedDate { get; private set; }
    public DateTime RegistrationDate { get; private set; }
    public bool IsActive { get; private set; }
    public List<SubUnit> SubUnits { get; private set; }
    public CompanyDetails? CompanyDetails { get; private set; }

    private MainUnitModel()
    {
        SubUnits = new List<SubUnit>();
    }

    public MainUnitModel(
        OrganizationNumber organizationNumber,
        string name,
        DateTime? registrationDate = null,
        string? businessAddress = null,
        string? postalAddress = null,
        string? industryCode = null,
        string? industryDescription = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        OrganizationNumber = organizationNumber;
        Name = name;
        RegistrationDate = registrationDate ?? DateTime.UtcNow;
        BusinessAddress = businessAddress;
        PostalAddress = postalAddress;
        IndustryCode = industryCode;
        IndustryDescription = industryDescription;
        IsActive = true;
        SubUnits = new List<SubUnit>();
    }

    public void UpdateBasicInfo(string name, string? businessAddress, string? postalAddress)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        Name = name;
        BusinessAddress = businessAddress;
        PostalAddress = postalAddress;
    }

    public void UpdateIndustryInfo(string? industryCode, string? industryDescription)
    {
        IndustryCode = industryCode;
        IndustryDescription = industryDescription;
    }

    public void UpdateEmployeeCount(int? employeeCount)
    {
        if (employeeCount.HasValue && employeeCount < 0)
            throw new ArgumentException("Employee count cannot be negative", nameof(employeeCount));

        EmployeeCount = employeeCount;
    }

    public void UpdateFoundedDate(DateTime? foundedDate)
    {
        if (foundedDate.HasValue && foundedDate > DateTime.UtcNow)
            throw new ArgumentException("Founded date cannot be in the future", nameof(foundedDate));

        FoundedDate = foundedDate;
    }

    public void AddSubUnit(SubUnit subUnit)
    {
        if (SubUnits.Any(s => s.OrganizationNumber == subUnit.OrganizationNumber))
            throw new InvalidOperationException($"Sub unit with organization number {subUnit.OrganizationNumber} already exists");

        SubUnits.Add(subUnit);
    }

    public void RemoveSubUnit(OrganizationNumber organizationNumber)
    {
        var subUnit = SubUnits.FirstOrDefault(s => s.OrganizationNumber == organizationNumber);
        if (subUnit != null)
        {
            subUnit.Deactivate();
        }
    }

    public void SetCompanyDetails(CompanyDetails details)
    {
        CompanyDetails = details;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public List<SubUnit> GetActiveSubUnits()
    {
        return SubUnits.Where(s => s.IsActive).ToList();
    }

    /// <summary>
    /// Sub-unit value object
    /// </summary>
    public class SubUnit
    {
        public OrganizationNumber OrganizationNumber { get; private set; }
        public string Name { get; private set; }
        public string? BusinessAddress { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime RegistrationDate { get; private set; }

        private SubUnit() { }

        public SubUnit(
            OrganizationNumber organizationNumber,
            string name,
            string? businessAddress = null,
            DateTime? registrationDate = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));

            OrganizationNumber = organizationNumber;
            Name = name;
            BusinessAddress = businessAddress;
            RegistrationDate = registrationDate ?? DateTime.UtcNow;
            IsActive = true;
        }

        public void UpdateInfo(string name, string? businessAddress)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));

            Name = name;
            BusinessAddress = businessAddress;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }
    }

    /// <summary>
    /// Company details value object
    /// </summary>
    public class CompanyDetails
    {
        public string? CompanyForm { get; private set; }
        public string? ShareCapital { get; private set; }
        public string? Currency { get; private set; }
        public DateTime? LastAccountsDate { get; private set; }
        public List<string> BusinessCodes { get; private set; }
        public Dictionary<string, string> RegisterDetails { get; private set; }

        private CompanyDetails()
        {
            BusinessCodes = new List<string>();
            RegisterDetails = new Dictionary<string, string>();
        }

        public CompanyDetails(
            string? companyForm = null,
            string? shareCapital = null,
            string? currency = null,
            DateTime? lastAccountsDate = null,
            List<string>? businessCodes = null,
            Dictionary<string, string>? registerDetails = null)
        {
            CompanyForm = companyForm;
            ShareCapital = shareCapital;
            Currency = currency;
            LastAccountsDate = lastAccountsDate;
            BusinessCodes = businessCodes ?? new List<string>();
            RegisterDetails = registerDetails ?? new Dictionary<string, string>();
        }

        public void UpdateFinancialInfo(string? shareCapital, string? currency, DateTime? lastAccountsDate)
        {
            ShareCapital = shareCapital;
            Currency = currency;
            LastAccountsDate = lastAccountsDate;
        }

        public void AddBusinessCode(string businessCode)
        {
            if (string.IsNullOrWhiteSpace(businessCode))
                throw new ArgumentException("Business code cannot be null or empty", nameof(businessCode));

            if (!BusinessCodes.Contains(businessCode))
            {
                BusinessCodes.Add(businessCode);
            }
        }

        public void RemoveBusinessCode(string businessCode)
        {
            BusinessCodes.Remove(businessCode);
        }

        public void SetRegisterDetail(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));

            RegisterDetails[key] = value;
        }

        public void RemoveRegisterDetail(string key)
        {
            RegisterDetails.Remove(key);
        }

        public string? GetRegisterDetail(string key)
        {
            return RegisterDetails.TryGetValue(key, out var value) ? value : null;
        }
    }
}

#endregion

#region Enums

public enum ContactType
{
    Email,
    Phone,
    Mobile,
    Fax,
    Website
}

public enum AddressType
{
    Business,
    Postal,
    Visiting,
    Home
}

public enum RelationshipType
{
    Parent,
    Child,
    Subsidiary,
    Branch,
    Representative,
    Owner
}

public enum UserPartyRelationType
{
    Employee,
    Representative,
    Owner,
    Authorized,
    Contact
}

public enum RoleType
{
    System,
    Business,
    Delegation,
    AccessList
}

#endregion

#region Exceptions

public class RegisterException : Exception
{
    public string? ErrorCode { get; }

    public RegisterException(string message, string? errorCode = null) : base(message)
    {
        ErrorCode = errorCode;
    }

    public RegisterException(string message, Exception innerException, string? errorCode = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

public class PartyNotFoundException : RegisterException
{
    public PartyId? PartyId { get; }
    public string? OrganizationNumber { get; }
    public string? PersonIdentifier { get; }

    public PartyNotFoundException(PartyId partyId) 
        : base($"Party with ID {partyId} not found", "PARTY_NOT_FOUND")
    {
        PartyId = partyId;
    }

    public PartyNotFoundException(string identifier, bool isOrganization = true) 
        : base($"{(isOrganization ? "Organization" : "Person")} with identifier {identifier} not found", "PARTY_NOT_FOUND")
    {
        if (isOrganization)
            OrganizationNumber = identifier;
        else
            PersonIdentifier = identifier;
    }
}

#endregion