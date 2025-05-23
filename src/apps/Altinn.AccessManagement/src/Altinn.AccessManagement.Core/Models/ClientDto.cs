using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Model representing a connected client party, meaning a party which has been authorized for one or more accesses, either directly or through role(s), access packages, resources or resource instances.
/// Model can be used both to represent a connection received from another party or a connection provided to another party.
/// </summary>
public class ClientDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientDto"/> class.
    /// </summary>
    public ClientDto()
    {
    }

    /// <summary>
    /// Gets or sets the party
    /// </summary>
    public ClientParty Party { get; set; }

    /// <summary>
    /// Gets or sets a collection of all rolecodes for roles from either Enhetsregisteret or Altinn 2 which the authorized subject has been authorized for on behalf of this party
    /// </summary>
    public List<string> AuthorizedRoles { get; set; } = [];

    /// <summary>
    /// Gets or sets a collection of all Authorized Packages 
    /// </summary>
    public List<ClientRoleAccessPackages> AuthorizedAccessPackages { get; set; } = [];

    /// <summary>
    /// Composite Key instances
    /// </summary>
    public class ClientParty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientParty"/> class.
        /// </summary>
        public ClientParty()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientParty"/> class based on a <see cref="Party"/> class.
        /// </summary>
        /// <param name="party">Party model from registry</param>
        /// <param name="includeSubunits">Whether model should also build list of subunits if any exists</param>
        public ClientParty(Party party, bool includeSubunits = true)
        {
            if (party.PartyTypeName != Platform.Register.Enums.PartyType.Organisation)
            {
                throw new ArgumentException("Party type must be organisation", nameof(party));
            }

            PartyUuid = party.PartyUuid.Value;
            Name = party.Name;
            Type = party.PartyTypeName.ToString();
            OrganizationNumber = party.OrgNumber;
            UnitType = party.UnitType;
            IsDeleted = party.IsDeleted;
            Subunits = includeSubunits ? party.ChildParties?.Select(subunit => new ClientParty(subunit)).ToList() ?? [] : [];
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
        public List<ClientParty> Subunits { get; set; } = [];
    }

    /// <summary>
    /// Composite Key instances
    /// </summary>
    public class ClientRoleAccessPackages
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
}
