namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// The Category object corresponds to the XML Attributes element.
/// See: http://docs.oasis-open.org/xacml/xacml-json-http/v1.1/csprd01/xacml-json-http-v1.1-csprd01.html
/// </summary>
public class AuthorizationXacmlCategoryDto
{
    /// <summary>
    /// Gets or sets CategoryId.
    /// Mandatory for a Category object in the "Category" member array; otherwise, optional.
    /// </summary>
    public string CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the Id of the category.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets optional XML content.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Gets or sets a list of attributes for a given attribute category.
    /// </summary>
    public List<AuthorizationXacmlAttributeDto> Attribute { get; set; }
}
