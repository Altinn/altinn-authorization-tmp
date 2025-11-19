using System.Collections;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace Altinn.Authorization.Scopes;

/// <summary>
/// Defines an authorization requirement that is satisfied when the user possesses any one of the specified scopes.
/// </summary>
public interface IAnyOfScopeAuthorizationRequirement
    : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the collection of scopes, any one of which is sufficient to satisfy the authorization requirement.
    /// </summary>
    ScopeSearchValues AnyOfScopes { get; }
}
