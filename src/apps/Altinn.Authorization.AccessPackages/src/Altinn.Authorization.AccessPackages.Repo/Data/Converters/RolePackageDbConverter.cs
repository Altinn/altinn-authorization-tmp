using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class RolePackageDbConverter : IDbExtendedConverter<RolePackage, ExtRolePackage>
{
    private readonly IDbExtendedConverter<Role, ExtRole> roleConverter;
    private readonly IDbExtendedConverter<Package, ExtPackage> packageConverter;
    private readonly IDbExtendedConverter<EntityVariant, ExtEntityVariant> entityVariantConverter;

    /// <summary>
    /// RolePackageDbConverter Constructor
    /// </summary>
    public RolePackageDbConverter(
        IDbExtendedConverter<Role, ExtRole> roleConverter,
        IDbExtendedConverter<Package, ExtPackage> packageConverter,
        IDbExtendedConverter<EntityVariant, ExtEntityVariant> entityVariantConverter
        )
    {
        this.roleConverter = roleConverter;
        this.packageConverter = packageConverter;
        this.entityVariantConverter = entityVariantConverter;
    }

    /// <inheritdoc/>
    public List<ExtRolePackage> ConvertExtended(IDataReader reader)
    {
        var result = new List<ExtRolePackage>();
        while (reader.Read())
        {
            result.Add(new ExtRolePackage()
            {
                Id = (Guid)reader["id"],
                RoleId = (Guid)reader["roleid"],
                PackageId = (Guid)reader["packageid"],
                EntityVariantId = DbUtil.ConvertDbValue<Guid>(reader["entityvariantid"]),
                IsActor = (bool)reader["isactor"],
                IsAdmin = (bool)reader["isadmin"],
                Role = roleConverter.ConvertSingleBasic(reader, "role_") ?? new Role(),
                Package = packageConverter.ConvertSingleBasic(reader, "package_") ?? new Package(),
                EntityVariant = entityVariantConverter.ConvertSingleBasic(reader, "entityvariant_")
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public List<RolePackage> ConvertBasic(IDataReader reader)
    {
        var result = new List<RolePackage>();
        while (reader.Read())
        {
            result.Add(new RolePackage()
            {
                Id = (Guid)reader["id"],
                RoleId = (Guid)reader["roleid"],
                PackageId = (Guid)reader["packageid"],
                EntityVariantId = DbUtil.ConvertDbValue<Guid>(reader["entityvariantid"]),
                IsActor = (bool)reader["isactor"],
                IsAdmin = (bool)reader["isadmin"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public RolePackage? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new RolePackage()
            {
                Id = (Guid)reader[$"{prefix}id"],
                RoleId = (Guid)reader[$"{prefix}roleid"],
                PackageId = (Guid)reader[$"{prefix}packageid"],
                EntityVariantId = DbUtil.ConvertDbValue<Guid>(reader[$"{prefix}entityvariantid"]),
                IsActor = (bool)reader[$"{prefix}isactor"],
                IsAdmin = (bool)reader[$"{prefix}isadmin"],
            };
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Utilities
/// </summary>
public static class DbUtil
{
    /// <summary>
    /// Convert nullable columns
    /// </summary>
    /// <typeparam name="T">ValueType</typeparam>
    /// <param name="obj">Value</param>
    /// <returns></returns>
    public static T? ConvertDbValue<T>(object obj)
    {
        if (obj == null || obj == DBNull.Value)
        {
            return default;
        }
        else
        {
            return (T)obj;
        }
    }
}
