using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Model representing a connected party, meaning a party which has been authorized for one or more accesses, either directly or through role(s), access packages, resources or resource instances.
/// Model can be used both to represent a connection received from another party or a connection provided to another party.
/// </summary>
public class NewConnectionDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NewConnectionDto"/> class.
    /// </summary>
    public NewConnectionDto()
    {
    }

    /// <summary>
    /// Gets or sets the party
    /// </summary>
    public NewParty Party { get; set; }

    /// <summary>
    /// Gets or sets sub-connections through key roles
    /// </summary>
    public IEnumerable<NewConnectionDto> KeyRoleConnections { get; set; }

    /// <summary>
    /// Gets or sets sub-connections through client-connection
    /// </summary>
    public IEnumerable<NewConnectionDto> ClientConnections { get; set; }

    /// <summary>
    /// Gets or sets a collection of all rolecodes for roles from either Enhetsregisteret or Altinn 2 which the authorized subject has been authorized for on behalf of this party
    /// </summary>
    public List<string> AuthorizedRoles { get; set; } = [];

    /// <summary>
    /// Gets or sets a collection of all Authorized Packages 
    /// </summary>
    public List<AuthorizedAccessPackage> AuthorizedPackages { get; set; } = [];

    /// <summary>
    /// Gets or sets a collection of all resource identifier the authorized subject has some access to on behalf of this party
    /// </summary>
    public List<string> AuthorizedResources { get; set; } = [];

    /// <summary>
    /// Gets or sets a collection of all Authorized Instances 
    /// </summary>
    public List<AuthorizedResourceInstance> AuthorizedInstances { get; set; } = [];

    /// <summary>
    /// Enriches this authorized party and any subunits with a resource access
    /// </summary>
    /// <param name="resourceId">The resource ID to add to the authorized party (and any subunits) list of authorized resources</param>
    public void EnrichWithResourceAccess(string resourceId)
    {
        resourceId = MapAppIdToResourceId(resourceId);
        AuthorizedResources.Add(resourceId);
    }

    /// <summary>
    /// Enriches this authorized party with a resource instance access
    /// </summary>
    /// <param name="resourceId">The resource ID of the instance delegation to add to the authorized party</param>
    /// <param name="instanceId">The instance ID of the instance delegation to add to the authorized party</param>
    public void EnrichWithResourceInstanceAccess(string resourceId, string instanceId)
    {
        // Ensure that we dont't add duplicates
        if (AuthorizedInstances.Exists(instance => instance.InstanceId == instanceId && instance.ResourceId == resourceId))
        {
            return;
        }

        AuthorizedInstances.Add(new()
        {
            ResourceId = resourceId,
            InstanceId = instanceId
        });
    }

    private static string MapAppIdToResourceId(string altinnAppId)
    {
        string[] orgAppSplit = altinnAppId.Split('/');
        if (orgAppSplit.Length == 2)
        {
            return $"app_{orgAppSplit[0]}_{orgAppSplit[1]}";
        }

        return altinnAppId;
    }

    /// <summary>
    /// Composite Key instances
    /// </summary>
    public class NewParty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewParty"/> class.
        /// </summary>
        public NewParty()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewParty"/> class based on a <see cref="Party"/> class.
        /// </summary>
        /// <param name="party">Party model from registry</param>
        /// <param name="includeSubunits">Whether model should also build list of subunits if any exists</param>
        public NewParty(Party party, bool includeSubunits = true)
        {
            PartyId = party.PartyId;
            PartyUuid = party.PartyUuid.Value;
            Name = party.Name;
            Type = party.PartyTypeName.ToString();
            OrganizationNumber = party.OrgNumber;
            UnitType = party.UnitType;
            IsDeleted = party.IsDeleted;
            Subunits = includeSubunits ? party.ChildParties?.Select(subunit => new NewParty(subunit)).ToList() ?? [] : [];
            PersonId = party.SSN;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewParty"/> class based on a <see cref="SblAuthorizedParty"/> class.
        /// </summary>
        /// <param name="sblAuthorizedParty">Authorized Party model from Altinn 2 SBL Bridge</param>
        /// <param name="includeSubunits">Whether model should also build list of subunits if any exists</param>
        public NewParty(SblAuthorizedParty sblAuthorizedParty, bool includeSubunits = true)
        {
            PartyId = sblAuthorizedParty.PartyId;
            PartyUuid = sblAuthorizedParty.PartyUuid.Value;
            Name = sblAuthorizedParty.Name;
            Type = sblAuthorizedParty.PartyTypeName.ToString();
            OrganizationNumber = sblAuthorizedParty.OrgNumber;
            UnitType = sblAuthorizedParty.UnitType;
            IsDeleted = sblAuthorizedParty.IsDeleted;
            Subunits = includeSubunits ? sblAuthorizedParty.ChildParties?.Select(subunit => new NewParty(subunit)).ToList() ?? [] : [];
            PersonId = sblAuthorizedParty.SSN;
        }

        /// <summary>
        /// Gets or sets the universally unique identifier of the party
        /// </summary>
        public Guid PartyUuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the party
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the organization number if the party is an organization
        /// </summary>
        public string OrganizationNumber { get; set; }

        /// <summary>
        /// Gets the national identity number if the party is a person
        /// </summary>
        public string PersonId { get; set; }

        /// <summary>
        /// Gets or sets the party id
        /// </summary>
        public int PartyId { get; set; }

        /// <summary>
        /// Gets or sets the type of party
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the unit type if the party is an organization
        /// </summary>
        public string UnitType { get; set; }

        /// <summary>
        /// Gets or sets whether this party is marked as deleted in the Central Coordinating Register for Legal Entities
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets a set of subunits of this party, which the authorized subject also has some access to.
        /// </summary>
        public List<NewParty> Subunits { get; set; } = [];
    }

    /// <summary>
    /// Composite Key instances
    /// </summary>
    public class AuthorizedAccessPackage
    {
        /// <summary>
        /// Resource ID
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// Instance ID
        /// </summary>
        public string[] Packages { get; set; }
    }

    /// <summary>
    /// Composite Key instances
    /// </summary>
    public class AuthorizedResourceInstance
    {
        /// <summary>
        /// Resource ID
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Instance ID
        /// </summary>
        public string InstanceId { get; set; }
    }
}
