namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// XACML JSON object for status code.
/// </summary>
public class AuthorizationXacmlStatusCodeDto
{
    /// <summary>
    /// Gets or sets the status code value.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets a nested status code.
    /// </summary>
    public AuthorizationXacmlStatusCodeDto StatusCode { get; set; }
}
