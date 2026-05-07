namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// The authorization response containing a list of XACML JSON results.
/// See: https://docs.oasis-open.org/xacml/xacml-json-http/v1.1/os/xacml-json-http-v1.1-os.html#_Toc5116225
/// </summary>
public class AuthorizationResponseDto
{
    /// <summary>
    /// Gets or sets a list of authorization results.
    /// </summary>
    public List<AuthorizationXacmlResultDto> Response { get; set; }
}
