using System.Collections.Concurrent;
using System.Reflection;
using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Models;

namespace Altinn.AccessMgmt.DbAccess.Helpers;

// TODO: Convert to singleton?

/// <summary>
/// Provides a centralized store for database definitions (<see cref="DbDefinition"/>) keyed by the CLR type.
/// This class allows registering, retrieving, and automatically loading definitions via reflection.
/// </summary>
public static class DefinitionStore
{
    /// <summary>
    /// The internal store for database definitions, keyed by type.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, DbDefinition> Store = new();

    /// <summary>
    /// Configures and registers a database definition for type <typeparamref name="T"/> using a configuration action.
    /// </summary>
    /// <typeparam name="T">The entity type for which the definition is to be registered.</typeparam>
    /// <param name="configure">An action that configures a <see cref="DefinitionBuilder{T}"/>.</param>
    public static void Define<T>(Action<DefinitionBuilder<T>> configure)
    {
        var builder = new DefinitionBuilder<T>();
        configure(builder);
        Store.AddOrUpdate(
            typeof(T),
            _ => builder.Build(), // Add
            (_, __) => builder.Build() // Update
        );
    }

    /// <summary>
    /// Registers a database definition for type <typeparamref name="T"/> using an existing <see cref="DefinitionBuilder{T}"/>.
    /// </summary>
    /// <typeparam name="T">The entity type for which the definition is to be registered.</typeparam>
    /// <param name="builder">The builder containing the definition configuration.</param>
    public static void Define<T>(DefinitionBuilder<T> builder)
    {
        Store.AddOrUpdate(
            typeof(T),
            _ => builder.Build(), // Add
            (_, __) => builder.Build() // Update
        );
    }

    /// <summary>
    /// Registers a database definition for type <typeparamref name="T"/> using an existing <see cref="DbDefinition"/>.
    /// </summary>
    /// <typeparam name="T">The entity type for which the definition is to be registered.</typeparam>
    /// <param name="dbDefinition">The database definition to register.</param>
    public static void Define<T>(DbDefinition dbDefinition)
    {
        Store.AddOrUpdate(
            typeof(T),
            _ => dbDefinition, // Add
            (_, __) => dbDefinition // Update
        );
    }

    /// <summary>
    /// Retrieves the registered database definition for type <typeparamref name="T"/>.
    /// If no definition is registered, a new one is built using the default configuration.
    /// </summary>
    /// <typeparam name="T">The entity type whose definition is requested.</typeparam>
    /// <returns>The corresponding <see cref="DbDefinition"/>.</returns>
    public static DbDefinition Definition<T>()
    {
        return Store.GetOrAdd(typeof(T), _ => new DefinitionBuilder<T>().Build());
    }

    public static DbDefinition? TryGetBaseDefinition<T>()
    {
        return Store.Where(t => t.Value.ModelType == typeof(T)).FirstOrDefault().Value;
    }

    /// <summary>
    /// Attempts to retrieve the registered database definition for the specified type.
    /// </summary>
    /// <param name="type">The entity type for which the definition is requested.</param>
    /// <returns>
    /// The corresponding <see cref="DbDefinition"/> if found; otherwise, <c>null</c>.
    /// </returns>
    public static DbDefinition? TryGetDefinition(Type type)
    {
        return Store.ContainsKey(type) ? Store[type] : null;
    }

    /// <summary>
    /// Attempts to retrieve the registered database definition for the specified type.
    /// </summary>
    /// <typeparam name="T">The entity type whose definition is requested.</typeparam>
    /// <returns>
    /// The corresponding <see cref="DbDefinition"/> if found; otherwise, <c>null</c>.
    /// </returns>
    public static DbDefinition? TryGetDefinition<T>()
    {
        return Store.ContainsKey(typeof(T)) ? Store[typeof(T)] : null;
    }

    /// <summary>
    /// Scans assemblies for types implementing <see cref="IDbDefinition"/> and registers their definitions.
    /// If a <paramref name="definitionNamespace"/> is provided, only definitions from the specified assembly are loaded.
    /// Otherwise, it scans all assemblies whose name starts with the executing assembly's name.
    /// </summary>
    /// <param name="definitionNamespace">
    /// The assembly name to load definitions from. If empty, the current executing assembly's related assemblies are scanned.
    /// </param>
    public static void RegisterAllDefinitions(string definitionNamespace = "")
    {
        List<IDbDefinition>? definitions;
        if (string.IsNullOrEmpty(definitionNamespace))
        {
            var executingAssemblyName = Assembly.GetExecutingAssembly().GetName().Name!;

            definitions = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name!.StartsWith(executingAssemblyName))
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IDbDefinition).IsAssignableFrom(t)
                    && !t.IsInterface
                    && !t.IsAbstract
                // Uncomment and adjust the following line if namespace filtering is needed:
                // && t.Namespace?.StartsWith("MyProject.Definitions") == true
                )
                .Select(t => (IDbDefinition)Activator.CreateInstance(t)!)
                .ToList();
        }
        else
        {
            var targetAssembly = Assembly.Load(definitionNamespace);
            definitions = targetAssembly.GetTypes()
                .Where(t => typeof(IDbDefinition).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(t => (IDbDefinition)Activator.CreateInstance(t)!)
                .ToList();
        }

        foreach (var definition in definitions)
        {
            definition.Define();
        }
    }
}
