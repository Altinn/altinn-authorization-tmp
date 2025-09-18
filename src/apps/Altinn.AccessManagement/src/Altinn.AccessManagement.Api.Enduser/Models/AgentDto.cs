namespace Altinn.AccessManagement.Core.Models;

/// <summary>
/// Model representing a connected client party, meaning a party which has been authorized for one or more accesses, either directly or through role(s), access packages, resources or resource instances.
/// Model can be used both to represent a connection received from another party or a connection provided to another party.
/// </summary>
public class AgentDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientDto"/> class.
    /// </summary>
    public AgentDto()
    {
    }

    /// <summary>
    /// Gets or sets the party
    /// </summary>
    public AgentParty Party { get; set; }

    /// <summary>
    /// Composite Key instances
    /// </summary>
    public class AgentParty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AgentParty"/> class.
        /// </summary>
        public AgentParty()
        {
        }

        /// <summary>
        /// Gets or sets the universally unique identifier of the party
        /// </summary>
        public Guid Id { get; set; }

        public string PersonIdentifier { get; set;  }

        /// <summary>
        /// Gets or sets the name of the party
        /// </summary>
        public string Name { get; set; }

        public List<ClientDto> Client { get; set; }
    }    
}
