using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

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

    private static readonly ConcurrentDictionary<Type, Dictionary<string, (PropertyInfo Property, Type? ElementType)>> PropertyCache = new();

    private Dictionary<string, (PropertyInfo Property, Type? ElementType)> CreatePropertyCacheWithPrefix(Type type, string prefix)
    {
        var properties = new Dictionary<string, (PropertyInfo, Type?)>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            string propertyKey = (prefix + property.Name.ToLower());

            // Sjekk om typen er en generisk List<T> eller IEnumerable<T>
            Type? elementType = GetListOrEnumerableElementType(property);

            properties[propertyKey] = (property, elementType);

            // Hvis det er et komplekst objekt og ikke en liste/enumerable, cache egenskaper med prefiks
            if (elementType == null && property.PropertyType.IsClass && property.PropertyType != typeof(string))
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

    private List<(PropertyInfo Property, string Prefix, Type? ElementType)> GetPropertiesWithPrefix<T>()
    {
        return GetPropertiesWithPrefix(typeof(T));
    }

    private List<(PropertyInfo Property, string Prefix, Type? ElementType)> GetPropertiesWithPrefix(Type type)
    {
        return PropertyCache.GetOrAdd(type, type => CreatePropertyCacheWithPrefix(type, string.Empty))
                            .Select(kv =>
                            {
                                string prefix = kv.Key.Contains("_") ? kv.Key.Substring(0, kv.Key.LastIndexOf('_') + 1) : string.Empty;
                                return (kv.Value.Property, prefix, kv.Value.ElementType);
                            })
                            .ToList();
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

    private Type? GetListOrEnumerableElementType(PropertyInfo property)
    {
        var propertyType = property.PropertyType;

        // Sjekk om det er en generisk List<T>
        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
        {
            return propertyType.GetGenericArguments()[0]; // Returner T i List<T>
        }

        // Sjekk om det er en generisk IEnumerable<T>
        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return propertyType.GetGenericArguments()[0]; // Returner T i IEnumerable<T>
        }

        return null;
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

    /// <summary>
    /// ConvertToObjects
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="reader">IDataReader</param>
    /// <returns></returns>
    public List<T> ConvertToObjectOlds<T>(IDataReader reader) 
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
                object value = reader.IsDBNull(i) ? null : reader.GetValue(i);

                foreach (var (property, prefix, elementType) in properties)
                {
                    if (columnName == (prefix + property.Name.ToLower()))
                    {
                        object currentObject = instance;
                        Type currentType = typeof(T);

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
                                        parentProperty.Property.SetValue(currentObject, null);
                                        break;
                                    }

                                    var subObject = parentProperty.Property.GetValue(currentObject);
                                    if (subObject == null)
                                    {
                                        subObject = Activator.CreateInstance(parentProperty.Property.PropertyType);
                                        parentProperty.Property.SetValue(currentObject, subObject);
                                    }

                                    currentObject = subObject;
                                    currentType = parentProperty.Property.PropertyType;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        // Håndter List<T> eller IEnumerable<T> egenskaper
                        if (elementType != null)
                        {
                            var valueData = JsonSerializer.Deserialize(value?.ToString() ?? "[]", property.PropertyType);
                            property.SetValue(currentObject, valueData);
                        }
                        else
                        {
                            // Bruk SetPropertyValue for andre typer
                            SetPropertyValue(property, currentObject, value);
                        }

                        break; // Gå til neste kolonne når verdien er satt
                    }
                }
            }

            result.Add(instance);
        }

        return result;
    }

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
                object value = reader.IsDBNull(i) ? null : reader.GetValue(i);

                foreach (var (property, prefix, elementType) in properties)
                {
                    if (columnName == (prefix + property.Name.ToLower()))
                    {
                        // Sjekk om vi skal markere et sub-objekt som null
                        if (!string.IsNullOrEmpty(prefix) && property.Name.ToLower() == "id" && value == null)
                        {
                            subObjectNullCheck[prefix] = true;
                            break;
                        }

                        object currentObject = instance;
                        Type currentType = typeof(T);

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
                                        parentProperty.Property.SetValue(currentObject, null);
                                        break;
                                    }

                                    var subObject = parentProperty.Property.GetValue(currentObject);
                                    if (subObject == null)
                                    {
                                        subObject = Activator.CreateInstance(parentProperty.Property.PropertyType);
                                        parentProperty.Property.SetValue(currentObject, subObject);
                                    }

                                    currentObject = subObject;
                                    currentType = parentProperty.Property.PropertyType;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        // Håndter List<T> eller IEnumerable<T> egenskaper
                        if (elementType != null)
                        {
                            var valueData = JsonSerializer.Deserialize(value?.ToString() ?? "[]", property.PropertyType);
                            property.SetValue(currentObject, valueData);
                        }
                        else
                        {
                            // Bruk SetPropertyValue for andre typer
                            SetPropertyValue(property, currentObject, value);
                        }

                        break; // Gå til neste kolonne når verdien er satt
                    }
                }
            }

            result.Add(instance);
        }

        return result;
    }


}
