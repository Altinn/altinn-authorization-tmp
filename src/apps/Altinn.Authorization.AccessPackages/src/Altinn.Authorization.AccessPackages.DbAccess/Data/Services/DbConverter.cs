using System.Collections.Concurrent;
using System.Data;
using System.Reflection;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Services;

/// <summary>
/// DbConverter
/// </summary>
public sealed class DbConverter
{
    private static readonly Lazy<DbConverter> _instance = new(() => new DbConverter());

    /// <summary>
    /// Instance
    /// </summary>
    public static DbConverter Instance => _instance.Value;

    /// <summary>
    /// DbConverter
    /// </summary>
    public DbConverter()
    {
    }

    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> PropertyCache = new();

    private List<(PropertyInfo Property, string Prefix)> GetPropertiesWithPrefix<T>()
    {
        return GetPropertiesWithPrefix(typeof(T));
    }

    private List<(PropertyInfo Property, string Prefix)> GetPropertiesWithPrefix(Type type)
    {
        return PropertyCache.GetOrAdd(type, type => CreatePropertyCacheWithPrefix(type, string.Empty))
                            .Select(kv => (kv.Value, kv.Key.Contains("_") ? kv.Key.Substring(0, kv.Key.LastIndexOf('_') + 1) : string.Empty))
                            .ToList();
    }

    private Dictionary<string, PropertyInfo> CreatePropertyCacheWithPrefix(Type type, string prefix)
    {
        var properties = new Dictionary<string, PropertyInfo>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            string propertyKey = prefix + property.Name.ToLower();

            properties[propertyKey] = property;

            // Hvis det er et komplekst objekt, cache dets egenskaper med prefiks
            if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var subProperties = CreatePropertyCacheWithPrefix(property.PropertyType, propertyKey + "_");
                foreach (var subProperty in subProperties)
                {
                    properties[subProperty.Key] = subProperty.Value;
                }
            }
        }

        return properties;
    }

    /// <summary>
    /// PreloadCache
    /// </summary>
    /// <param name="types">Types to preload</param>
    public void PreloadCache(params Type[] types)
    {
        foreach (var type in types)
        {
            if (!PropertyCache.ContainsKey(type))
            {
                GetPropertiesWithPrefix(type);
            }
        }
    }

    /// <summary>
    /// ConvertToObjects
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="reader">IDataReader</param>
    /// <returns></returns>
    public List<T> ConvertToObjects<T>(IDataReader reader) 
        where T : new()
    {
        var properties = GetPropertiesWithPrefix<T>();
        var result = new List<T>();

        while (reader.Read())
        {
            var instance = new T();
            var subObjectNullCheck = new Dictionary<string, bool>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i).ToLower();
                object? value = reader.IsDBNull(i) ? null : reader.GetValue(i);

                foreach (var (property, prefix) in properties)
                {
                    if (columnName == (prefix + property.Name.ToLower()))
                    {
                        object? currentObject = instance;
                        Type currentType = typeof(T);

                        // Sjekk om vi har en identifikator for et sub-objekt
                        if (!string.IsNullOrEmpty(prefix) && property.Name.ToLower() == "id" && value == null)
                        {
                            // Marker at dette sub-objektet skal være null
                            subObjectNullCheck[prefix] = true;
                            break;
                        }

                        // Naviger til riktig sub-objekt hvis det er nødvendig
                        if (!string.IsNullOrEmpty(prefix))
                        {
                            var parts = prefix.Split('_', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var part in parts)
                            {
                                if (PropertyCache[currentType].TryGetValue(part, out var parentProperty))
                                {
                                    // Sjekk om hele sub-objektet skal være null
                                    if (subObjectNullCheck.TryGetValue(prefix, out bool isNull) && isNull)
                                    {
                                        parentProperty.SetValue(currentObject, null);
                                        break;
                                    }

                                    var subObject = parentProperty.GetValue(currentObject);
                                    if (subObject == null)
                                    {
                                        subObject = Activator.CreateInstance(parentProperty.PropertyType);
                                        parentProperty.SetValue(currentObject, subObject);
                                    }

                                    currentObject = subObject;
                                    currentType = parentProperty.PropertyType;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        // Bruk SetPropertyValue for å sette verdien
                        SetPropertyValue(property, currentObject, value);
                        break; // Gå til neste kolonne når verdien er satt
                    }
                }
            }

            result.Add(instance);
        }

        return result;
    }
    
    private void SetPropertyValue(PropertyInfo property, object? target, object? value)
    {
        if (value != null)
        {
            if (property.PropertyType == typeof(Guid))
            {
                // Sett Guid direkte eller parse fra string
                property.SetValue(target, value is Guid ? value : Guid.Parse(value.ToString()));
            }
            else if (property.PropertyType == typeof(Guid?))
            {
                // Håndter nullable Guid og sett til null hvis strengen er tom eller Guid er Guid.Empty
                if (value is Guid guidValue)
                {
                    property.SetValue(target, guidValue == Guid.Empty ? null : (Guid?)guidValue);
                }
                else if (string.IsNullOrWhiteSpace(value.ToString()))
                {
                    property.SetValue(target, null);
                }
                else
                {
                    property.SetValue(target, (Guid?)Guid.Parse(value.ToString()));
                }
            }
            else
            {
                // Generell konvertering for andre typer
                property.SetValue(target, Convert.ChangeType(value, property.PropertyType));
            }
        }
    }
}
