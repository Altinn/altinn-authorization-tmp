namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// XACML JSON object for status.
/// </summary>
public class AuthorizationXacmlStatusDto
{
    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string StatusMessage { get; set; }

    /// <summary>
    /// Gets or sets list of status details.
    /// </summary>
    public List<string> StatusDetails { get; set; }

    /// <summary>
    /// Gets or sets the status code.
    /// </summary>
    public AuthorizationXacmlStatusCodeDto StatusCode { get; set; }
}
