using System.Collections.Immutable;

namespace Altinn.Authorization.Scopes;

/// <summary>
/// Requirement for authorization policies used for validating a client scope.
/// <see href="https://docs.asp.net/en/latest/security/authorization/policies.html"/> for details about authorization
/// in asp.net core.
/// </summary>
internal sealed class AnyOfScopeAuthorizationRequirement
    : IAnyOfScopeAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnyOfScopeAuthorizationRequirement"/> class and 
    /// pupulates the Scope property with the given scope.
    /// </summary>
    /// <param name="scope">The scope for this requirement</param>
    public AnyOfScopeAuthorizationRequirement(string scope)
        : this([scope])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnyOfScopeAuthorizationRequirement"/> class with the given scopes.
    /// </summary>
    /// <param name="scopes">The scope for this requirement</param>
    public AnyOfScopeAuthorizationRequirement(scoped ReadOnlySpan<string> scopes)
    {
        AnyOfScopes = ScopeSearchValues.Create(scopes);
    }

    /// <summary>
    /// Gets or sets the scope defined for the policy using this requirement
    /// </summary>
    public ScopeSearchValues AnyOfScopes { get; }
}
