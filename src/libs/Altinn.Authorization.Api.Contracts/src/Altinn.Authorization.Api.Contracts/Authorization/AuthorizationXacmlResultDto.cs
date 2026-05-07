namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// A single XACML JSON authorization result.
/// See: https://docs.oasis-open.org/xacml/xacml-json-http/v1.1/os/xacml-json-http-v1.1-os.html#_Toc5116225
/// </summary>
public class AuthorizationXacmlResultDto
{
    /// <summary>
    /// Gets or sets the XACML Decision (e.g. "Permit", "Deny", "NotApplicable", "Indeterminate").
    /// </summary>
    public string Decision { get; set; }

    /// <summary>
    /// Gets or sets the status.
    /// </summary>
    public AuthorizationXacmlStatusDto Status { get; set; }

    /// <summary>
    /// Gets or sets any obligations of the result.
    /// </summary>
    public List<AuthorizationXacmlObligationOrAdviceDto> Obligations { get; set; }

    /// <summary>
    /// Gets or sets XACML Advice.
    /// </summary>
    public List<AuthorizationXacmlObligationOrAdviceDto> AssociateAdvice { get; set; }

    /// <summary>
    /// Gets or sets category attributes returned with the result.
    /// </summary>
    public List<AuthorizationXacmlCategoryDto> Category { get; set; }

    /// <summary>
    /// Gets or sets policy identifier list related to the result.
    /// </summary>
    public AuthorizationXacmlPolicyIdentifierListDto PolicyIdentifierList { get; set; }
}
