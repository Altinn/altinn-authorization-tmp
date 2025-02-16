using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Altinn.AccessMgmt.DbAccess.Helpers;

/// <summary>
/// Provides extension methods for registering database-related services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the database access services to the specified <see cref="IServiceCollection"/>,
    /// scanning the given assembly for implementations and definitions required for database operations.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the database services will be added.</param>
    /// <param name="assemblyName">
    /// The name of the assembly to scan for database service definitions and implementations.
    /// This may include repositories, converters, and other database-related components.
    /// </param>
    /// <returns>
    /// The updated <see cref="IServiceCollection"/> instance.
    /// </returns>
    public static IServiceCollection AddDbServices(this IServiceCollection services, string assemblyName)
    {
        var targetAssembly = Assembly.Load(assemblyName);

        var serviceTypes = targetAssembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract)
            .Where(t => t.GetInterfaces().Any(i => i.Name.EndsWith("Service"))); // Henter alle som slutter på "Service"

        foreach (var serviceType in serviceTypes)
        {
            var interfaceType = serviceType.GetInterfaces().FirstOrDefault(i => i.Name == $"I{serviceType.Name}");
            if (interfaceType != null)
            {
                Console.WriteLine($"Registering {serviceType.Name} as {interfaceType.Name}");
                services.AddScoped(interfaceType, serviceType);
            }
        }

        return services;
    }
}
