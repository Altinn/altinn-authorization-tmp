using System.ComponentModel.DataAnnotations;
using Altinn.Authorization.Api.Contracts.AccessManagement.Consent;
using Altinn.Authorization.Api.Contracts.AccessManagement.Rights;
using Altinn.Urn.Json;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.InstanceDelegation;

/// <summary>
/// Request model for performing revoke of access to a resource from Apps
/// </summary>
public class AppsInstanceRevokeResponseDto
{
    /// <summary>
    /// Gets or sets the urn identifying the party to delegate from
    /// </summary>
    [Required]
    public UrnJsonTypeValue<ConsentPartyUrn> From { get; set; }

    /// <summary>
    /// Gets or sets the urn identifying the party to be delegated to
    /// </summary>
    [Required]
    public UrnJsonTypeValue<ConsentPartyUrn> To { get; set; }

    /// <summary>
    /// Gets or sets a value identifying the resource of the instance
    /// </summary>
    [Required]
    public string ResourceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value identifying the instance id
    /// </summary>
    [Required]
    public string InstanceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rights to delegate
    /// </summary>
    [Required]
    public IEnumerable<RightRevokeResultDto> Rights { get; set; } = [];
}
