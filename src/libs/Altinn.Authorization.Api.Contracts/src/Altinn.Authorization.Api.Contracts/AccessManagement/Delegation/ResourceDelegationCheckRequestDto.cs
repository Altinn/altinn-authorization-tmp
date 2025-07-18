using System.ComponentModel.DataAnnotations;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.Delegation;

/// <summary>
/// Request model for a list of all delegable rights for a specific resource.
/// </summary>
public class ResourceDelegationCheckRequestDto
{
    /// <summary>
    /// Gets or sets the resource id for identifying the resource of the rights to be checked
    /// </summary>
    [Required]
    public string ResourceId { get; set; } = string.Empty;
}
