using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class RoleMapDbConverter : IDbExtendedConverter<RoleMap, ExtRoleMap>
{
    private readonly IDbExtendedConverter<Role, ExtRole> roleConverter;

    /// <summary>
    /// RoleMapDbConverter Constructor
    /// </summary>
    public RoleMapDbConverter(IDbExtendedConverter<Role, ExtRole> roleConverter)
    {
        this.roleConverter = roleConverter;
    }

    /// <inheritdoc/>
    public List<ExtRoleMap> ConvertExtended(IDataReader reader)
    {
        var result = new List<ExtRoleMap>();
        while (reader.Read())
        {
            result.Add(new ExtRoleMap()
            {
                Id = (Guid)reader["id"],
                HasRoleId = (Guid)reader["hasroleid"],
                GetRoleId = (Guid)reader["getroleid"],
                HasRole = roleConverter.ConvertSingleBasic(reader, "hasrole_") ?? new Role(),
                GetRole = roleConverter.ConvertSingleBasic(reader, "getrole_") ?? new Role()
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public List<RoleMap> ConvertBasic(IDataReader reader)
    {
        var result = new List<RoleMap>();
        while (reader.Read())
        {
            result.Add(new RoleMap()
            {
                Id = (Guid)reader["id"],
                HasRoleId = (Guid)reader["hasroleid"],
                GetRoleId = (Guid)reader["getroleid"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public RoleMap? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new RoleMap()
            {
                Id = (Guid)reader[$"{prefix}id"],
                HasRoleId = (Guid)reader[$"{prefix}hasroleid"],
                GetRoleId = (Guid)reader[$"{prefix}getroleid"],
            };
        }
        catch
        {
            return null;
        }
    }
}