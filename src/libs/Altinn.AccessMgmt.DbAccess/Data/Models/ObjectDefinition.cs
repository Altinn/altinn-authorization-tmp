using System.Data;
using System.Reflection;

namespace Altinn.AccessMgmt.DbAccess.Data.Models;

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
    /// ExtendedType
    /// </summary>
    public Type? ExtendedType { get; set; }

    /// <summary>
    /// Extended properties
    /// </summary>
    public Dictionary<string, PropertyInfo> ExtendedProperties { get; set; }

    /// <summary>
    /// New Database Object Definition
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="config">DbObjDefConfig</param>
    /// <param name="useTranslation">UseTranslation (default: false)</param>
    /// <param name="useHistory">UseHistory (default: false)</param>
    public ObjectDefinition(Type type, DbObjDefConfig config, bool useTranslation = false, bool useHistory = false)
    {
        SetBasic(type, config, useTranslation, useHistory);
    }

    /// <summary>
    /// New Database Object Definition
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="extendedType">Extended type</param>
    /// <param name="config">DbObjDefConfig</param>
    /// <param name="useTranslation">UseTranslation (default: false)</param>
    /// <param name="useHistory">UseHistory (default: false)</param>
    public ObjectDefinition(Type type, Type extendedType, DbObjDefConfig config, bool useTranslation = false, bool useHistory = false)
    {
        SetBasic(type, config, useTranslation, useHistory);
        SetExtended(extendedType);
    }

    private void SetExtended(Type type)
    {
        ExtendedType = type;
        ExtendedProperties = new Dictionary<string, PropertyInfo>();
        foreach (var property in type.GetProperties())
        {
            if (!Properties.ContainsKey(property.Name))
            {
                ExtendedProperties.Add(property.Name, property);
            }
        }
    }

    private void SetBasic(Type type, DbObjDefConfig config, bool useTranslation, bool useHistory)
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
        UseTranslation = useTranslation;
        UseHistory = useHistory;
    }
}
