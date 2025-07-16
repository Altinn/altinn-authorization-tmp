using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.Delegation;

/// <summary>
/// Response model for the result of a delegation status check, for which rights a user is able to delegate between two parties.
/// </summary>
public class ResourceDelegationCheckResponseDto
{
    /// <summary>
    /// Gets or sets the party identifier the rights can be delegated from
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public required string From { get; set; }

    /// <summary>
    /// Gets or sets a list of right delegation status models
    /// </summary>
    public IEnumerable<ResourceRightDelegationCheckResultDto> ResourceRightDelegationCheckResults { get; set; } = [];
}
