namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// JSON object for request references in a multi-request.
/// </summary>
public class AuthorizationXacmlRequestReferenceDto
{
    /// <summary>
    /// Gets or sets the reference Ids.
    /// </summary>
    public List<string> ReferenceId { get; set; }
}
