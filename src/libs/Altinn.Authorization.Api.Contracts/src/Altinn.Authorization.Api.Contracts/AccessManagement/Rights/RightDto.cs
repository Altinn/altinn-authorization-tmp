using System.Text.Json;
using Altinn.Authorization.Api.Contracts.AccessManagement.Common;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.Rights;

/// <summary>
/// This model describes a single right
/// </summary>
public class RightDto
{
    /// <summary>
    /// Gets or sets the list of resource matches which uniquely identifies the resource this right applies to.
    /// </summary>
    public IEnumerable<AttributeMatchExternal> Resource { get; set; } = [];

    /// <summary>
    /// Gets or sets the set of Attribute Id and Attribute Value for a specific action, to identify the action this right applies to
    /// </summary>
    public string Action { get; set; } = string.Empty;
}
