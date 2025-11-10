using Microsoft.AspNetCore.Authorization;

namespace Altinn.Authorization.Scopes;

/// <summary>
/// Represents an authorization handler that can perform authorization based on scope
/// </summary>
internal sealed class AnyOfScopeAuthorizationHandler
    : AuthorizationHandler<IAnyOfScopeAuthorizationRequirement>
{
    /// <inheritdoc/>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IAnyOfScopeAuthorizationRequirement requirement)
    {
        foreach (var identity in context.User.Identities.Where(static i => string.Equals(i.AuthenticationType, "AuthenticationTypes.Federation")))
        {
            foreach (var claim in identity.Claims.Where(static c => string.Equals(c.Type, "urn:altinn:scope")))
            {
                if (requirement.AnyOfScopes.Check(claim.Value))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }
        }

        foreach (var claim in context.User.Claims.Where(static c => string.Equals(c.Type, "scope")))
        {
            if (requirement.AnyOfScopes.Check(claim.Value))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
