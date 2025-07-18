using System.ComponentModel.DataAnnotations;
using Altinn.Authorization.Api.Contracts.AccessManagement.Common;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.Rights;

/// <summary>
/// Response model for the result of a delegation of one or more rights to a recipient.
/// </summary>
public class RightsDelegationResponseExternal
{
    /// <summary>
    /// Gets or sets a set of Attribute Id and Attribute Value identfying the single person or entity receiving the delegation
    /// </summary>
    [Required]
    public List<AttributeMatchExternal> To { get; set; } = [];

    /// <summary>
    /// Gets or sets a list of rights delegation results with status wheter the delegation was successfully delegated to the recipient.
    /// </summary>
    [Required]
    public List<RightDelegationResultExternal> RightDelegationResults { get; set; } = [];
}
