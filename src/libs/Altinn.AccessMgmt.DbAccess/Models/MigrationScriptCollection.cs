﻿namespace Altinn.AccessMgmt.DbAccess.Models;

/// <summary>
/// Holds migration scripts for a type
/// </summary>
public class MigrationScriptCollection
{
    /// <summary>
    /// Type of the migration script collection
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// Migration scripts
    /// </summary>
    public OrderedDictionary<string, string> Scripts { get; set; }

    /// <summary>
    /// Dependencies for the migration script collection
    /// </summary>
    public Dictionary<Type, int> Dependencies { get; set; }

    /// <summary>
    /// Default constructor for MigrationScriptCollection
    /// </summary>
    /// <param name="type">Type</param>
    public MigrationScriptCollection(Type type)
    {
        Type = type;
        Scripts = new OrderedDictionary<string, string>();
        Dependencies = new Dictionary<Type, int>();
    }

    /// <summary>
    /// Add scripts to collection
    /// </summary>
    /// <param name="scripts">Migration keys and queries</param>
    public void AddScripts(OrderedDictionary<string, string> scripts)
    {
        foreach (var script in scripts)
        {
            Scripts.Add(script.Key, script.Value);
        }
    }

    /// <summary>
    /// Add script to collection
    /// </summary>
    /// <param name="keyValueSet">Migration key and query</param>
    public void AddScripts((string Key, string Query) keyValueSet)
    {
        Scripts.Add(keyValueSet.Key, keyValueSet.Query);
    }

    /// <summary>
    /// Add script to collection
    /// </summary>
    /// <param name="key">Migration key</param>
    /// <param name="query">Script</param>
    public void AddScripts(string key, string query)
    {
        Scripts.Add(key, query);
    }

    /// <summary>
    /// Add dependency to collection
    /// </summary>
    /// <param name="type">Type</param>
    public void AddDependency(Type type)
    {
        if (type.Equals(Type))
        {
            return;
        }

        if (!Dependencies.ContainsKey(type))
        {
            Dependencies.Add(type, 0);
        }

        Dependencies[type]++;
    }
}
