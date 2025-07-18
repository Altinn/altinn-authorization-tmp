using System.ComponentModel.DataAnnotations;
using Altinn.Authorization.Api.Contracts.AccessManagement.Consent;
using Altinn.Authorization.Api.Contracts.AccessManagement.Rights;
using Altinn.Urn.Json;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.InstanceDelegation;

/// <summary>
/// Request model for performing delegation of access to an app instance from Apps
/// </summary>
public class AppsInstanceDelegationRequestDto
{
    /// <summary>
    /// Gets or sets the urn identifying the party to delegate from
    /// </summary>
    [Required]
    public required UrnJsonTypeValue<ConsentPartyUrn> From { get; set; }

    /// <summary>
    /// Gets or sets the urn identifying the party to be delegated to
    /// </summary>
    [Required]
    public required UrnJsonTypeValue<ConsentPartyUrn> To { get; set; }

    /// <summary>
    /// Gets or sets the rights to delegate
    /// </summary>
    [Required]
    public required IEnumerable<RightDto> Rights { get; set; }
}
