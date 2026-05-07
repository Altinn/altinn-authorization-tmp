namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// A JSON object that defines references to multiple requests.
/// </summary>
public class AuthorizationXacmlMultiRequestsDto
{
    /// <summary>
    /// Gets or sets the request references.
    /// </summary>
    public List<AuthorizationXacmlRequestReferenceDto> RequestReference { get; set; }
}
