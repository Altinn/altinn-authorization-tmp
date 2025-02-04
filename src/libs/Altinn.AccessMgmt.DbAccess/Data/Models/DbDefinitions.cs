namespace Altinn.AccessMgmt.DbAccess.Data.Models;

/// <summary>
/// Static resource holding Database Definitions
/// </summary>
public static class DbDefinitions
{
    private static Dictionary<Type, ObjectDefinition> DbObjects { get; set; } = new Dictionary<Type, ObjectDefinition>();

    /// <summary>
    /// Add a definition
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="config">DbObjDefConfig</param>
    /// <param name="useTranslation">UseTranslation (default: false)</param>
    /// <param name="useHistory">UseHistory (default: false)</param>
    public static void Add<T>(DbAccessConfig config, bool useTranslation = false, bool useHistory = false)
    {
        if (!DbObjects.ContainsKey(typeof(T)))
        {
            DbObjects.Add(typeof(T), new ObjectDefinition(typeof(T), useTranslation, useHistory));
        }
    }

    /// <summary>
    /// Add a definition
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <typeparam name="TExtended">ExtendedType</typeparam>
    /// <param name="config">DbObjDefConfig</param>
    /// <param name="useTranslation">UseTranslation (default: false)</param>
    /// <param name="useHistory">UseHistory (default: false)</param>
    public static void Add<T, TExtended>(DbAccessConfig config, bool useTranslation = false, bool useHistory = false)
    {
        if (!DbObjects.ContainsKey(typeof(T)))
        {
            DbObjects.Add(typeof(T), new ObjectDefinition(typeof(T), typeof(TExtended), useTranslation, useHistory));
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
    /// <typeparam name="TExtended">Type</typeparam>
    /// <returns></returns>
    public static ObjectDefinition? GetExtended<TExtended>()
    {
        return GetExtended(type: typeof(TExtended));
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

    /// <summary>
    /// Gets a definition
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns></returns>
    public static ObjectDefinition? GetExtended(Type type)
    {
        return DbObjects.Count(t => t.Value.ExtendedType == type) > 0 ? DbObjects.First(t => t.Value.ExtendedType == type).Value : null;
    }

    /// <summary>
    /// Gets a definition
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns></returns>
    public static ObjectDefinition? GetNullable(Type? type)
    {
        if (type == null)
        {
            return null;
        }

        return DbObjects.ContainsKey(type) ? DbObjects.First(t => t.Key == type).Value : null;
    }
}
