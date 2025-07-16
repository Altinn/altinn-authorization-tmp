using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Altinn.Authorization.Api.Contracts.AccessManagement.Common;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.Delegation;

/// <summary>
/// Response model describing the delegation status for a given single right, whether the authenticated user is able to delegate the right or not on behalf of the from part.
/// </summary>
public class ResourceRightDelegationCheckResultDto
{
    /// <summary>
    /// Gets or sets the right key
    /// </summary>
    [Required]
    public string RightKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of resource matches which uniquely identifies the resource this right applies to.
    /// </summary>
    [Required]
    public IEnumerable<string> Resource { get; set; } = [];

    /// <summary>
    /// Gets or sets the action identifier for the action this right applies to
    /// </summary>
    [Required]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the right is delegable or not
    /// </summary>
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DelegableStatusExternal Status { get; set; }

    /// <summary>
    /// Gets or sets a list of details describing why or why not the right is valid in the current user and reportee party context
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IEnumerable<DetailExternal> Details { get; set; } = [];
}
