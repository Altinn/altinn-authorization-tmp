using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class EntityDbConverter : IDbExtendedConverter<Entity, ExtEntity>
{
    private readonly IDbExtendedConverter<EntityType, ExtEntityType> entityTypeConverter;
    private readonly IDbExtendedConverter<EntityVariant, ExtEntityVariant> entityVariantConverter;

    /// <summary>
    /// EntityDbConverter Constructor
    /// </summary>
    /// <param name="entityTypeConverter">Converter EntityType</param>
    /// <param name="entityVariantConverter">Converter EntityVariant</param>
    public EntityDbConverter(
        IDbExtendedConverter<EntityType, ExtEntityType> entityTypeConverter,
        IDbExtendedConverter<EntityVariant, ExtEntityVariant> entityVariantConverter
        )
    {
        this.entityTypeConverter = entityTypeConverter;
        this.entityVariantConverter = entityVariantConverter;
    }

    /// <inheritdoc/>
    public List<ExtEntity> ConvertExtended(IDataReader reader)
    {
        var result = new List<ExtEntity>();
        while (reader.Read())
        {
            result.Add(new ExtEntity()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                RefId = (string)reader["refid"],
                TypeId = (Guid)reader["typeid"],
                VariantId = (Guid)reader["variantid"],
                Type = entityTypeConverter.ConvertSingleBasic(reader, "type_") ?? new EntityType(),
                Variant = entityVariantConverter.ConvertSingleBasic(reader, "variant_") ?? new EntityVariant()
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public List<Entity> ConvertBasic(IDataReader reader)
    {
        var result = new List<Entity>();
        while (reader.Read())
        {
            result.Add(new Entity()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                RefId = (string)reader["refid"],
                TypeId = (Guid)reader["typeid"],
                VariantId = (Guid)reader["variantid"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public Entity? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new Entity()
            {
                Id = (Guid)reader[$"{prefix}id"],
                Name = (string)reader[$"{prefix}name"],
                RefId = (string)reader[$"{prefix}refid"],
                TypeId = (Guid)reader[$"{prefix}typeid"],
                VariantId = (Guid)reader[$"{prefix}variantid"]
            };
        }
        catch
        {
            return null;
        }
    }
}
