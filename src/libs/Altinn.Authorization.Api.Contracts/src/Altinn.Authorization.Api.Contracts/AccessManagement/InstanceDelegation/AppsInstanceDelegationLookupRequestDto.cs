using System.ComponentModel.DataAnnotations;
using Altinn.Authorization.Api.Contracts.AccessManagement.Common;
using Altinn.Authorization.Api.Contracts.AccessManagement.Consent;
using Altinn.Urn.Json;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.InstanceDelegation;

/// <summary>
/// Request model for performing delegation of access to an app instance from Apps
/// </summary>
public class AppsInstanceDelegationLookupRequestDto
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
    /// Gets or sets the urn identifying the resource of the instance
    /// </summary>
    [Required]
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the urn identifying the instance id
    /// </summary>
    [Required]
    public string Instance { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of delegation to distinguish between parallel signing and other instance delegations
    /// </summary>
    [Required]
    public InstanceDelegationModeExternal InstanceDelegationMode { get; set; }
}
