namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// JSON representation of a XACML attribute assignment.
/// </summary>
public class AuthorizationXacmlAttributeAssignmentDto
{
    /// <summary>
    /// Gets or sets a string containing a XACML attribute URI. Mandatory.
    /// </summary>
    public string AttributeId { get; set; }

    /// <summary>
    /// Gets or sets the value. Mandatory.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets a string containing a XACML category URI or the shorthand notation.
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Gets or sets the datatype of the attribute.
    /// </summary>
    public string DataType { get; set; }

    /// <summary>
    /// Gets or sets the issuer of the attribute. Optional.
    /// </summary>
    public string Issuer { get; set; }
}
