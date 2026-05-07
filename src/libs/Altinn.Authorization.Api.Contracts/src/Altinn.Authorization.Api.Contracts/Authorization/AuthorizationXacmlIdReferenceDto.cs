namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// A JSON object for policy references.
/// </summary>
public class AuthorizationXacmlIdReferenceDto
{
    /// <summary>
    /// Gets or sets a string containing a XACML policy or policy set URI.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public string Version { get; set; }
}
