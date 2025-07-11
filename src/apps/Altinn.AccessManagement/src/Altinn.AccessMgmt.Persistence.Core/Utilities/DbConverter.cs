﻿using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Text.Json;
using Altinn.AccessMgmt.Persistence.Core.Models;

namespace Altinn.AccessMgmt.Persistence.Core.Utilities;

/// <summary>
/// DbConverter
/// </summary>
public sealed class DbConverter : IDbConverter
{
    private static readonly Lazy<DbConverter> _instance = new(() => new DbConverter());

    /// <summary>
    /// Instance
    /// </summary>
    public static DbConverter Instance => _instance.Value;

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

    /// <inheritdoc />
    public QueryResponse<T> ConvertToResult<T>(IDataReader reader)
    where T : new()
    {
        var converted = ConvertToObjects<T>(reader, true);
        return new QueryResponse<T>()
        {
            Data = converted.Data,
            Page = converted.PageInfo
        };
    }

    private (List<T> Data, QueryPageInfo PageInfo) ConvertToObjects<T>(IDataReader reader, bool includePageColumns)
    where T : new()
    {
        var properties = GetPropertiesWithPrefix<T>();
        var result = new List<T>();

        var pageInfo = new QueryPageInfo();

        while (reader.Read())
        {
            var instance = new T();
            var subObjectNullCheck = new Dictionary<string, bool>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i).ToLower();
                object value = reader.IsDBNull(i) ? null : reader.GetValue(i);

                if (includePageColumns)
                {
                    if (columnName == "_rownumber")
                    {
                        if (pageInfo.FirstRowOnPage == 0)
                        {
                            pageInfo.FirstRowOnPage = (int)value;
                        }

                        pageInfo.LastRowOnPage = (int)value;
                    }

                    if (pageInfo.TotalSize == 0 && columnName == "_totalItemCount")
                    {
                        pageInfo.TotalSize = (int)value;
                    }

                    if (pageInfo.PageSize == 0 && columnName == "_pageSize")
                    {
                        pageInfo.PageSize = (int)value;
                    }

                    if (pageInfo.PageNumber == 0 && columnName == "_pageNumber")
                    {
                        pageInfo.PageNumber = (int)value;
                    }
                }

                foreach (var (property, prefix, elementType) in properties)
                {
                    if (columnName == prefix + property.Name.ToLower())
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
                            var valueData = JsonSerializer.Deserialize(value?.ToString() ?? "[]", property.PropertyType, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));
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

        return (result, pageInfo);
    }

    private static readonly ConcurrentDictionary<Type, Dictionary<string, (PropertyInfo Property, Type ElementType)>> PropertyCache = new();

    private Dictionary<string, (PropertyInfo Property, Type ElementType)> CreatePropertyCacheWithPrefix(Type type, string prefix, int level)
    {
        var properties = new Dictionary<string, (PropertyInfo, Type)>();

        if (level > 3)
        {
            return properties;
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            string propertyKey = prefix + property.Name.ToLower();

            // Check if type is generic List<T> or IEnumerable<T>
            Type elementType = GetListOrEnumerableElementType(property);

            properties[propertyKey] = (property, elementType);

            // If property is complex and not a list/enumerable, cache properties with prefix
            if (elementType == null && property.PropertyType.IsClass && property.PropertyType != typeof(string))
            {
                var subProperties = CreatePropertyCacheWithPrefix(property.PropertyType, propertyKey + "_", level + 1);
                foreach (var subProperty in subProperties)
                {
                    properties[subProperty.Key] = subProperty.Value;
                }
            }
        }

        return properties;
    }

    private List<(PropertyInfo Property, string Prefix, Type ElementType)> GetPropertiesWithPrefix<T>()
    {
        return GetPropertiesWithPrefix(typeof(T));
    }

    private List<(PropertyInfo Property, string Prefix, Type ElementType)> GetPropertiesWithPrefix(Type type)
    {
        return PropertyCache.GetOrAdd(type, type => CreatePropertyCacheWithPrefix(type, string.Empty, 1))
                            .Select(kv =>
                            {
                                string prefix = kv.Key.Contains('_') ? kv.Key.Substring(0, kv.Key.LastIndexOf('_') + 1) : string.Empty;
                                return (kv.Value.Property, prefix, kv.Value.ElementType);
                            })
                            .ToList();
    }

    private Type GetListOrEnumerableElementType(PropertyInfo property)
    {
        var propertyType = property.PropertyType;

        // Check if property is generic List<T>
        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(List<>))
        {
            return propertyType.GetGenericArguments()[0]; // Return T in List<T>
        }

        // Check if property is generic IEnumerable<T>
        if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            return propertyType.GetGenericArguments()[0]; // Return T in IEnumerable<T>
        }

        return null;
    }

    private void SetPropertyValue(PropertyInfo property, object target, object value)
    {
        if (value != null)
        {
            if (property.PropertyType == typeof(Guid))
            {
                value = value is Guid ? value : Guid.Parse(value.ToString());
            }
            else if (property.PropertyType == typeof(Guid?))
            {
                if (value is Guid guidValue)
                {
                    value = guidValue == Guid.Empty ? null : (Guid?)guidValue;
                }
                else if (string.IsNullOrWhiteSpace(value.ToString()))
                {
                    value = null;
                }
                else
                {
                    value = (Guid?)Guid.Parse(value.ToString());
                }
            }
            else if (property.PropertyType == typeof(DateTimeOffset))
            {
                value = string.IsNullOrWhiteSpace(value.ToString()) ? null : DateTimeOffset.Parse(value.ToString());
            }
            else if (property.PropertyType.Namespace.StartsWith("Altinn"))
            {
                value = JsonSerializer.Deserialize(value?.ToString() ?? "{}", property.PropertyType, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));
                property.SetValue(target, value);
            }
            else
            {
                value = Convert.ChangeType(value, property.PropertyType);
            }

            // Use the non-public setter if available
            var setter = property.GetSetMethod(true);
            if (setter != null)
            {
                setter.Invoke(target, new[] { value });
            }
        }
    }

    private void SetPropertyValueOld(PropertyInfo property, object target, object value)
    {
        if (value != null)
        {
            if (property.PropertyType == typeof(Guid))
            {
                // Set Guid directly og parse from string
                property.SetValue(target, value is Guid ? value : Guid.Parse(value.ToString()));
            }
            else if (property.PropertyType == typeof(Guid?))
            {
                // Handle nullable Guid and set null if string is empty or if Guid is Guid.Empty
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
            else if (property.PropertyType == typeof(DateTimeOffset))
            {
                if (string.IsNullOrWhiteSpace(value.ToString()))
                {
                    property.SetValue(target, null);
                }
                else
                {
                    property.SetValue(target, DateTimeOffset.Parse(value.ToString()));
                }
            }
            else
            {
                // Generic convert for other types
                property.SetValue(target, Convert.ChangeType(value, property.PropertyType));
            }
        }
    }
}
