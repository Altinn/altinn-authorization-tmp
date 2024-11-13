namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

/// <summary>
/// Static resource holding Database Definitions ()
/// </summary>
public static class DbDefinitions
{
    private static Dictionary<Type, ObjectDefinition> DbObjects { get; set; } = new Dictionary<Type, ObjectDefinition>();

    /// <summary>
    /// Add a definition
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="config">DbObjDefConfig</param>
    public static void Add<T>(DbObjDefConfig config)
    {
        if (!DbObjects.ContainsKey(typeof(T)))
        {
            DbObjects.Add(typeof(T), new ObjectDefinition(typeof(T), config));
        }
    }

    /// <summary>
    /// Gets a definition
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <returns></returns>
    public static ObjectDefinition? Get<T>()
    {
        return Get(type: typeof(T));
    }

    /// <summary>
    /// Gets a definition
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns></returns>
    public static ObjectDefinition? Get(Type type)
    {
        return DbObjects.ContainsKey(type) ? DbObjects.First(t => t.Key == type).Value : null;
    }
}
