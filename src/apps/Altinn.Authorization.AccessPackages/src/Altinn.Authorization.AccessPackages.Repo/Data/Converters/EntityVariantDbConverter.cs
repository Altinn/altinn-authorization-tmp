using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class EntityVariantDbConverter : IDbExtendedConverter<EntityVariant, ExtEntityVariant>
{
    private readonly IDbExtendedConverter<EntityType, ExtEntityType> entityTypeConverter;

    /// <summary>
    /// EntityVariantDbConverter Constructor
    /// </summary>
    /// <param name="entityTypeConverter">Provider converter</param>
    public EntityVariantDbConverter(IDbExtendedConverter<EntityType, ExtEntityType> entityTypeConverter)
    {
        this.entityTypeConverter = entityTypeConverter;
    }

    /// <inheritdoc/>
    public List<ExtEntityVariant> ConvertExtended(IDataReader reader)
    {
        var result = new List<ExtEntityVariant>();
        while (reader.Read())
        {
            result.Add(new ExtEntityVariant()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                Description = (string)reader["description"],
                TypeId = (Guid)reader["typeid"],
                Type = entityTypeConverter.ConvertSingleBasic(reader, "type_") ?? new EntityType()
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public List<EntityVariant> ConvertBasic(IDataReader reader)
    {
        var result = new List<EntityVariant>();
        while (reader.Read())
        {
            result.Add(new EntityVariant()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                Description = (string)reader["description"],
                TypeId = (Guid)reader["typeid"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public EntityVariant? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new EntityVariant()
            {
                Id = (Guid)reader[$"{prefix}id"],
                Name = (string)reader[$"{prefix}name"],
                Description = (string)reader[$"{prefix}description"],
                TypeId = (Guid)reader[$"{prefix}typeid"],
            };
        }
        catch
        {
            return null;
        }
    }
}