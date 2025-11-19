using Altinn.Authorization.Scopes;

namespace Microsoft.AspNetCore.Authorization;

/// <summary>
/// Provides extension methods for configuring Altinn-specific authorization policies using the
/// AuthorizationPolicyBuilder.
/// </summary>
public static class AltinnAuthorizationPolicyBuilderExtensions
{
#if NET9_0_OR_GREATER
    /// <summary>
    /// Adds a requirement to the authorization policy that at least one of the specified scopes must be present in the
    /// user's claims.
    /// </summary>
    /// <param name="builder">The <see cref="AuthorizationPolicyBuilder"/> to which the scope requirement will be added.</param>
    /// <param name="scopes">An array of scope names. At least one of these scopes must be present in the user's claims to satisfy the
    /// requirement. Cannot be null or empty.</param>
    /// <returns>The <see cref="AuthorizationPolicyBuilder"/> instance with the added scope requirement.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="scopes"/> is null or empty.</exception>
    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public static AuthorizationPolicyBuilder RequireAnyScopeOf(
        this AuthorizationPolicyBuilder builder,
        params System.Collections.Immutable.ImmutableArray<string> scopes)
    {
        if (scopes.IsDefaultOrEmpty)
        {
            throw new ArgumentException("Scopes cannot be null or empty.", nameof(scopes));
        }

        builder.Requirements.Add(new AnyOfScopeAuthorizationRequirement([.. scopes]));
        return builder;
    }
#endif

    /// <summary>
    /// Adds a requirement to the authorization policy that at least one of the specified scopes must be present in the
    /// user's claims.
    /// </summary>
    /// <param name="builder">The <see cref="AuthorizationPolicyBuilder"/> to which the scope requirement will be added.</param>
    /// <param name="scopes">An array of scope names. At least one of these scopes must be present in the user's claims to satisfy the
    /// requirement. Cannot be null or empty.</param>
    /// <returns>The <see cref="AuthorizationPolicyBuilder"/> instance with the added scope requirement.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="scopes"/> is null or empty.</exception>
    public static AuthorizationPolicyBuilder RequireAnyScopeOf(
        this AuthorizationPolicyBuilder builder,
        params string[] scopes)
    {
        if (scopes is null or { Length: 0 })
        {
            throw new ArgumentException("Scopes cannot be null or empty.", nameof(scopes));
        }

        builder.Requirements.Add(new AnyOfScopeAuthorizationRequirement([.. scopes]));
        return builder;
    }
}
