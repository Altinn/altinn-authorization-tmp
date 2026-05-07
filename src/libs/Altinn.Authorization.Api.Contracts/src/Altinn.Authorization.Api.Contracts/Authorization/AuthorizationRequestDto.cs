namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// Root object for the authorization request.
/// </summary>
public class AuthorizationRequestDto
{
    /// <summary>
    /// Gets or sets the request.
    /// </summary>
    public AuthorizationXacmlRequestDto Request { get; set; }
}
