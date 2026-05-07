namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// JSON representation of a XACML obligation or advice.
/// </summary>
public class AuthorizationXacmlObligationOrAdviceDto
{
    /// <summary>
    /// Gets or sets a string containing a XACML obligation or advice URI.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets an array of attribute assignment objects.
    /// </summary>
    public List<AuthorizationXacmlAttributeAssignmentDto> AttributeAssignment { get; set; }
}
