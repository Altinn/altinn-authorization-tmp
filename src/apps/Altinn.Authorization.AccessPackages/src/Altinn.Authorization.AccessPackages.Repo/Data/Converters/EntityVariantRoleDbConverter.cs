using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;
using Npgsql.Internal;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class EntityVariantRoleDbConverter : IDbCrossConverter<EntityVariant, EntityVariantRole, Role>
{
    private readonly IDbExtendedConverter<EntityVariant, ExtEntityVariant> entityVariantConverter;
    private readonly IDbExtendedConverter<Role, ExtRole> roleConverter;

    /// <summary>
    /// EntityVariantRoleDbConverter Constructor
    /// </summary>
    public EntityVariantRoleDbConverter(
        IDbExtendedConverter<EntityVariant, ExtEntityVariant> entityVariantConverter,
        IDbExtendedConverter<Role, ExtRole> roleConverter)
    {
        this.entityVariantConverter = entityVariantConverter;
        this.roleConverter = roleConverter;
    }

    /// <inheritdoc/>
    public List<EntityVariantRole> ConvertBasic(IDataReader reader)
    {
        var result = new List<EntityVariantRole>();
        while (reader.Read())
        {
            result.Add(new EntityVariantRole()
            {
                Id = (Guid)reader["id"],
                RoleId = (Guid)reader["roleid"],
                VariantId = (Guid)reader["variantid"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public EntityVariantRole? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new EntityVariantRole()
            {
                Id = (Guid)reader[$"{prefix}id"],
                RoleId = (Guid)reader[$"{prefix}roleid"],
                VariantId = (Guid)reader[$"{prefix}variantid"],
            };
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public List<EntityVariant> ConvertA(IDataReader reader)
    {
        return entityVariantConverter.ConvertBasic(reader);
    }

    /// <inheritdoc/>
    public List<Role> ConvertB(IDataReader reader)
    {
        return roleConverter.ConvertBasic(reader);
    }
}