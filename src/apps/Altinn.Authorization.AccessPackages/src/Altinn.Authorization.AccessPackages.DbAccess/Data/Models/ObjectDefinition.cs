using System.Reflection;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

/// <summary>
/// Database Object Definition
/// </summary>
public class ObjectDefinition
{
    /// <summary>
    /// Base object definition
    /// </summary>
    public DbObject BaseDbObject { get; set; }

    /// <summary>
    /// Translation object definition
    /// </summary>
    public DbObject TranslationDbObject { get; set; }

    /// <summary>
    /// History object definition
    /// </summary>
    public DbObject HistoryDbObject { get; set; }

    /// <summary>
    /// Use translation
    /// </summary>
    public bool UseTranslation { get; set; } = false;

    /// <summary>
    /// Use history
    /// </summary>
    public bool UseHistory { get; set; } = false;

    /// <summary>
    /// Properties
    /// </summary>
    public Dictionary<string, PropertyInfo> Properties { get; set; }

    /// <summary>
    /// New Database Object Definition
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="config">DbObjDefConfig</param>
    public ObjectDefinition(Type type, DbObjDefConfig config)
    {
        var name = type.Name;
        Properties = new Dictionary<string, PropertyInfo>();
        foreach (var property in type.GetProperties())
        {
            Properties.Add(property.Name, property);
        }

        BaseDbObject = new DbObject(type, name, config.BaseSchema, name);
        TranslationDbObject = new DbObject(type, name, config.TranslationSchema, "Translation" + name);
        HistoryDbObject = new DbObject(type, name, config.HistorySchema, "History" + name);

        if (config.TranslateObjects != null && config.TranslateObjects.Contains(name))
        {
            UseTranslation = true;
        }

        if (config.HistoryObjects != null && config.HistoryObjects.Contains(name))
        {
            UseHistory = true;
        }
    }
}
