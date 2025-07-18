using Altinn.Authorization.Shared;

namespace Altinn.AccessManagement.Core.Models;

#region Delegation Models

/// <summary>
/// Core delegation domain model with business logic
/// </summary>
public class DelegationModel
{
    public DelegationId Id { get; private set; }
    public PartyId OfferedBy { get; private set; }
    public string OfferedByName { get; private set; }
    public string? OfferedByOrganizationNumber { get; private set; }
    public PartyId CoveredBy { get; private set; }
    public string CoveredByName { get; private set; }
    public string? CoveredByOrganizationNumber { get; private set; }
    public ResourceId ResourceId { get; private set; }
    public string ResourceType { get; private set; }
    public DateTime Created { get; private set; }
    public DateTime? Updated { get; private set; }
    public UserId PerformedBy { get; private set; }
    public DelegationStatus Status { get; private set; }
    public List<DelegationRight> Rights { get; private set; }
    public List<ResourceReference> ResourceReferences { get; private set; }
    public CompetentAuthority? Authority { get; private set; }

    private DelegationModel() 
    { 
        Rights = new List<DelegationRight>();
        ResourceReferences = new List<ResourceReference>();
    } // EF Constructor

    public DelegationModel(
        DelegationId id,
        PartyId offeredBy,
        string offeredByName,
        PartyId coveredBy,
        string coveredByName,
        ResourceId resourceId,
        string resourceType,
        UserId performedBy,
        List<DelegationRight> rights)
    {
        ValidateConstructorParameters(offeredBy, offeredByName, coveredBy, coveredByName, resourceId, resourceType, rights);

        Id = id;
        OfferedBy = offeredBy;
        OfferedByName = offeredByName;
        CoveredBy = coveredBy;
        CoveredByName = coveredByName;
        ResourceId = resourceId;
        ResourceType = resourceType;
        PerformedBy = performedBy;
        Rights = rights;
        ResourceReferences = new List<ResourceReference>();
        Status = DelegationStatus.Active;
        Created = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a delegation model from persistence data
    /// </summary>
    public static DelegationModel FromPersistence(
        DelegationId id,
        PartyId offeredBy,
        string offeredByName,
        string? offeredByOrganizationNumber,
        PartyId coveredBy,
        string coveredByName,
        string? coveredByOrganizationNumber,
        ResourceId resourceId,
        string resourceType,
        DateTime created,
        DateTime? updated,
        UserId performedBy,
        DelegationStatus status,
        List<DelegationRight> rights,
        List<ResourceReference> resourceReferences,
        CompetentAuthority? competentAuthority = null)
    {
        return new DelegationModel
        {
            Id = id,
            OfferedBy = offeredBy,
            OfferedByName = offeredByName,
            OfferedByOrganizationNumber = offeredByOrganizationNumber,
            CoveredBy = coveredBy,
            CoveredByName = coveredByName,
            CoveredByOrganizationNumber = coveredByOrganizationNumber,
            ResourceId = resourceId,
            ResourceType = resourceType,
            Created = created,
            Updated = updated,
            PerformedBy = performedBy,
            Status = status,
            Rights = rights,
            ResourceReferences = resourceReferences,
            Authority = competentAuthority
        };
    }

    // Business Logic Methods
    public void Revoke(UserId performedBy, string? reason = null)
    {
        if (Status == DelegationStatus.Revoked)
            throw new InvalidOperationException("Delegation is already revoked");

        Status = DelegationStatus.Revoked;
        PerformedBy = performedBy;
        Updated = DateTime.UtcNow;
    }

    public DelegationModel UpdateRights(List<DelegationRight> newRights, UserId performedBy)
    {
        if (newRights == null || !newRights.Any())
            throw new ArgumentException("Rights cannot be empty", nameof(newRights));

        Rights = newRights;
        PerformedBy = performedBy;
        Updated = DateTime.UtcNow;
        return this;
    }

    public bool IsActiveForResource(ResourceId resourceId)
    {
        return Status == DelegationStatus.Active && ResourceId == resourceId;
    }

    public void AddResourceReference(ResourceReference reference)
    {
        if (ResourceReferences.Any(r => r.ReferenceType == reference.ReferenceType))
            throw new InvalidOperationException($"Reference type {reference.ReferenceType} already exists");

        ResourceReferences.Add(reference);
        Updated = DateTime.UtcNow;
    }

    public void SetPartyDetails(string offeredByName, string? offeredByOrganizationNumber,
        string coveredByName, string? coveredByOrganizationNumber)
    {
        OfferedByName = offeredByName;
        OfferedByOrganizationNumber = offeredByOrganizationNumber;
        CoveredByName = coveredByName;
        CoveredByOrganizationNumber = coveredByOrganizationNumber;
        Updated = DateTime.UtcNow;
    }

    public void SetResourceDetails(string resourceType)
    {
        ResourceType = resourceType;
        Updated = DateTime.UtcNow;
    }

    public void SetCompetentAuthority(CompetentAuthority competentAuthority)
    {
        Authority = competentAuthority;
        Updated = DateTime.UtcNow;
    }

    private static void ValidateConstructorParameters(
        PartyId offeredBy, string offeredByName, PartyId coveredBy, string coveredByName,
        ResourceId resourceId, string resourceType, List<DelegationRight> rights)
    {
        if (offeredBy == coveredBy)
            throw new DomainException("Cannot delegate to the same party");

        if (string.IsNullOrWhiteSpace(offeredByName))
            throw new DomainException("OfferedByName cannot be empty");

        if (string.IsNullOrWhiteSpace(coveredByName))
            throw new DomainException("CoveredByName cannot be empty");

        if (string.IsNullOrWhiteSpace(resourceType))
            throw new DomainException("ResourceType cannot be empty");

        if (rights == null || !rights.Any())
            throw new DomainException("Rights cannot be empty");
    }

    /// <summary>
    /// Delegation right value object
    /// </summary>
    public class DelegationRight : IEquatable<DelegationRight>
    {
        public string Action { get; private set; }
        public string Resource { get; private set; }
        public List<AttributeMatch> AttributeMatches { get; private set; }

        private DelegationRight() 
        { 
            AttributeMatches = new List<AttributeMatch>();
        } // EF Constructor

        public DelegationRight(string action, string resource, List<AttributeMatch>? attributeMatches = null)
        {
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action cannot be null or empty", nameof(action));
            if (string.IsNullOrWhiteSpace(resource))
                throw new ArgumentException("Resource cannot be null or empty", nameof(resource));

            Action = action;
            Resource = resource;
            AttributeMatches = attributeMatches ?? new List<AttributeMatch>();
        }

        public void AddAttributeMatch(AttributeMatch attributeMatch)
        {
            if (AttributeMatches.Any(am => am.Id == attributeMatch.Id))
                throw new InvalidOperationException($"Attribute match with ID {attributeMatch.Id} already exists");

            AttributeMatches.Add(attributeMatch);
        }

        public bool Equals(DelegationRight? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Action == other.Action && Resource == other.Resource;
        }

        public override bool Equals(object? obj) => Equals(obj as DelegationRight);
        public override int GetHashCode() => HashCode.Combine(Action, Resource);
    }

    /// <summary>
    /// Resource reference value object
    /// </summary>
    public class ResourceReference : IEquatable<ResourceReference>
    {
        public string ReferenceType { get; private set; }
        public string Reference { get; private set; }
        public string? ReferenceSource { get; private set; }

        private ResourceReference() { } // EF Constructor

        public ResourceReference(string referenceType, string reference, string? referenceSource = null)
        {
            if (string.IsNullOrWhiteSpace(referenceType))
                throw new ArgumentException("ReferenceType cannot be null or empty", nameof(referenceType));
            if (string.IsNullOrWhiteSpace(reference))
                throw new ArgumentException("Reference cannot be null or empty", nameof(reference));

            ReferenceType = referenceType;
            Reference = reference;
            ReferenceSource = referenceSource;
        }

        public bool Equals(ResourceReference? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return ReferenceType == other.ReferenceType && Reference == other.Reference;
        }

        public override bool Equals(object? obj) => Equals(obj as ResourceReference);
        public override int GetHashCode() => HashCode.Combine(ReferenceType, Reference);
    }

    /// <summary>
    /// Competent authority value object
    /// </summary>
    public class CompetentAuthority
    {
        public string? Orgcode { get; private set; }
        public string? Organization { get; private set; }
        public string? Name { get; private set; }

        private CompetentAuthority() { } // EF Constructor

        public CompetentAuthority(string? orgcode, string? organization, string? name)
        {
            Orgcode = orgcode;
            Organization = organization;
            Name = name;
        }
    }
}

/// <summary>
/// Delegation change event model
/// </summary>
public class DelegationChangeModel
{
    public Guid Id { get; private set; }
    public DelegationId DelegationId { get; private set; }
    public DelegationChangeType ChangeType { get; private set; }
    public UserId PerformedBy { get; private set; }
    public DateTime Created { get; private set; }
    public string? ChangeDetails { get; private set; }

    private DelegationChangeModel() { } // EF Constructor

    public DelegationChangeModel(
        DelegationId delegationId,
        DelegationChangeType changeType,
        UserId performedBy,
        string? changeDetails = null)
    {
        Id = Guid.NewGuid();
        DelegationId = delegationId;
        ChangeType = changeType;
        PerformedBy = performedBy;
        ChangeDetails = changeDetails;
        Created = DateTime.UtcNow;
    }
}

#endregion

#region Authorized Party Models

/// <summary>
/// Authorized party domain model
/// </summary>
public class AuthorizedPartyModel
{
    public PartyId PartyId { get; private set; }
    public string Name { get; private set; }
    public string? OrganizationNumber { get; private set; }
    public string? PersonIdentifier { get; private set; }
    public PartyType PartyType { get; private set; }
    public List<Right> Rights { get; private set; }
    public List<Resource> Resources { get; private set; }
    public List<DelegationModel> Delegations { get; private set; }

    private AuthorizedPartyModel() 
    { 
        Rights = new List<Right>();
        Resources = new List<Resource>();
        Delegations = new List<DelegationModel>();
    } // EF Constructor

    public AuthorizedPartyModel(
        PartyId partyId, 
        string name, 
        PartyType partyType,
        string? organizationNumber = null,
        string? personIdentifier = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        PartyId = partyId;
        Name = name;
        PartyType = partyType;
        OrganizationNumber = organizationNumber;
        PersonIdentifier = personIdentifier;
        Rights = new List<Right>();
        Resources = new List<Resource>();
        Delegations = new List<DelegationModel>();
    }

    public void AddRight(Right right)
    {
        if (Rights.Any(r => r.Equals(right)))
            return; // Already exists

        Rights.Add(right);
    }

    public void AddResource(Resource resource)
    {
        if (Resources.Any(r => r.ResourceId == resource.ResourceId))
            return; // Already exists

        Resources.Add(resource);
    }

    public void AddDelegation(DelegationModel delegation)
    {
        if (Delegations.Any(d => d.Id == delegation.Id))
            return; // Already exists

        Delegations.Add(delegation);
    }

    /// <summary>
    /// Right value object
    /// </summary>
    public class Right : IEquatable<Right>
    {
        public string Action { get; private set; }
        public string Resource { get; private set; }
        public RightSourceType Source { get; private set; }
        public List<AttributeMatch> AttributeMatches { get; private set; }
        public DateTime? ValidFrom { get; private set; }
        public DateTime? ValidTo { get; private set; }

        private Right() 
        { 
            AttributeMatches = new List<AttributeMatch>();
        } // EF Constructor

        public Right(
            string action, 
            string resource, 
            RightSourceType source,
            List<AttributeMatch>? attributeMatches = null,
            DateTime? validFrom = null,
            DateTime? validTo = null)
        {
            if (string.IsNullOrWhiteSpace(action))
                throw new ArgumentException("Action cannot be null or empty", nameof(action));
            if (string.IsNullOrWhiteSpace(resource))
                throw new ArgumentException("Resource cannot be null or empty", nameof(resource));

            Action = action;
            Resource = resource;
            Source = source;
            AttributeMatches = attributeMatches ?? new List<AttributeMatch>();
            ValidFrom = validFrom;
            ValidTo = validTo;
        }

        public bool IsValid(DateTime? atTime = null)
        {
            var checkTime = atTime ?? DateTime.UtcNow;
            return (ValidFrom == null || ValidFrom <= checkTime) &&
                   (ValidTo == null || ValidTo >= checkTime);
        }

        public bool Equals(Right? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Action == other.Action && Resource == other.Resource && Source == other.Source;
        }

        public override bool Equals(object? obj) => Equals(obj as Right);
        public override int GetHashCode() => HashCode.Combine(Action, Resource, Source);
    }

    /// <summary>
    /// Resource value object
    /// </summary>
    public class Resource
    {
        public ResourceId ResourceId { get; private set; }
        public string ResourceType { get; private set; }
        public string Title { get; private set; }
        public string? Description { get; private set; }
        public List<string> AvailableActions { get; private set; }

        private Resource() 
        { 
            AvailableActions = new List<string>();
        } // EF Constructor

        public Resource(
            ResourceId resourceId, 
            string resourceType, 
            string title,
            string? description = null,
            List<string>? availableActions = null)
        {
            if (string.IsNullOrWhiteSpace(resourceType))
                throw new ArgumentException("ResourceType cannot be null or empty", nameof(resourceType));
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be null or empty", nameof(title));

            ResourceId = resourceId;
            ResourceType = resourceType;
            Title = title;
            Description = description;
            AvailableActions = availableActions ?? new List<string>();
        }
    }
}

#endregion

#region Common Value Objects

// AttributeMatch is now provided by Altinn.Authorization.Shared

#endregion

#region Enums

public enum DelegationStatus
{
    Active,
    Revoked,
    Expired
}

public enum DelegationType
{
    Normal,
    SystemUser,
    Maskinporten
}

// DelegationChangeType is now provided by Altinn.Authorization.Shared

public enum PartyType
{
    Person,
    Organization,
    SubUnit
}

public enum RightSourceType
{
    Role,
    Delegation,
    AccessList,
    SystemUser,
    Maskinporten
}

// AttributeMatchType is now provided by Altinn.Authorization.Shared

public enum InstanceDelegationMode
{
    Normal,
    ParallelSigning,
    CompleteInstance
}

#endregion

#region Exceptions

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

#endregion