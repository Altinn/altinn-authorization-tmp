using Altinn.Authorization.Scopes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class AltinnAuthorizationScopesServiceCollectionExtensions
{
    /// <summary>
    /// Registers Altinn scopes-based authorization handlers with the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to which the authorization handler will be added. Cannot be null.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddAltinnScopesAuthorizationHandlers(this IServiceCollection services)
    {
        services.AddAnyOfScopeAuthorizationHandler();

        return services;
    }

    /// <summary>
    /// Registers the AnyOfScopeAuthorizationHandler for scope-based authorization in the application's dependency
    /// injection container.
    /// </summary>
    /// <param name="services">The service collection to which the authorization handler will be added. Cannot be null.</param>
    /// <returns><paramref name="services"/>.</returns>
    public static IServiceCollection AddAnyOfScopeAuthorizationHandler(this IServiceCollection services)
    {
        services.TryAddSingleton<IAuthorizationHandler, AnyOfScopeAuthorizationHandler>();

        return services;
    }
}
