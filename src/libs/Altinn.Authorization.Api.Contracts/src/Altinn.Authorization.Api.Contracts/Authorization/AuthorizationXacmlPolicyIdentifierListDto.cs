namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// A JSON object that references policies or policy sets.
/// </summary>
public class AuthorizationXacmlPolicyIdentifierListDto
{
    /// <summary>
    /// Gets or sets list of policy id references.
    /// </summary>
    public List<AuthorizationXacmlIdReferenceDto> PolicyIdReference { get; set; }

    /// <summary>
    /// Gets or sets list of policy set references.
    /// </summary>
    public List<AuthorizationXacmlIdReferenceDto> PolicySetIdReference { get; set; }
}
