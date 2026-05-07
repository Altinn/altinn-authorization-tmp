namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// Representation of the XACML request in JSON.
/// See: https://docs.oasis-open.org/xacml/xacml-json-http/v1.1/os/xacml-json-http-v1.1-os.html#_Toc5116207
/// </summary>
public class AuthorizationXacmlRequestDto
{
    /// <summary>
    /// Gets or sets a value indicating whether the PolicyIdList should be returned. Optional. Default false.
    /// </summary>
    public bool ReturnPolicyIdList { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether it is a combined decision.
    /// </summary>
    public bool CombinedDecision { get; set; }

    /// <summary>
    /// Gets or sets the xpath version.
    /// </summary>
    public string XPathVersion { get; set; }

    /// <summary>
    /// Gets or sets the generic category attributes.
    /// </summary>
    public List<AuthorizationXacmlCategoryDto> Category { get; set; }

    /// <summary>
    /// Gets or sets the resource attributes.
    /// </summary>
    public List<AuthorizationXacmlCategoryDto> Resource { get; set; }

    /// <summary>
    /// Gets or sets the action attributes.
    /// </summary>
    public List<AuthorizationXacmlCategoryDto> Action { get; set; }

    /// <summary>
    /// Gets or sets the subject attributes.
    /// </summary>
    public List<AuthorizationXacmlCategoryDto> AccessSubject { get; set; }

    /// <summary>
    /// Gets or sets the recipient subject.
    /// </summary>
    public List<AuthorizationXacmlCategoryDto> RecipientSubject { get; set; }

    /// <summary>
    /// Gets or sets the intermediary subjects attributes.
    /// </summary>
    public List<AuthorizationXacmlCategoryDto> IntermediarySubject { get; set; }

    /// <summary>
    /// Gets or sets attributes about requesting machine.
    /// </summary>
    public List<AuthorizationXacmlCategoryDto> RequestingMachine { get; set; }

    /// <summary>
    /// Gets or sets references to multiple requests.
    /// </summary>
    public AuthorizationXacmlMultiRequestsDto MultiRequests { get; set; }
}
