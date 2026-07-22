using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.V2;

/// <summary>
/// Model representing a connected client party with the resources delegated per role.
/// </summary>
public class ClientResourcesDto
{
    /// <summary>
    /// Gets or sets the party
    /// </summary>
    [JsonPropertyName("client")]
    public CompactEntityDto Client { get; set; }

    /// <summary>
    /// Gets or sets a collection of all access information for the client
    /// </summary>
    [JsonPropertyName("access")]
    public List<RoleAccess> Access { get; set; } = [];

    /// <summary>
    /// Access given through a single role
    /// </summary>
    public class RoleAccess
    {
        /// <summary>
        /// Role
        /// </summary>
        [JsonPropertyName("role")]
        public CompactRoleDto Role { get; set; }

        /// <summary>
        /// Resources
        /// </summary>
        [JsonPropertyName("resources")]
        public List<CompactResourceDto> Resources { get; set; } = [];
    }
}
