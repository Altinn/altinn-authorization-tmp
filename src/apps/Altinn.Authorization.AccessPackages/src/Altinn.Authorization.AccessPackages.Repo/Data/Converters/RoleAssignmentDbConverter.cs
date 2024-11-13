using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class RoleAssignmentDbConverter : IDbExtendedConverter<RoleAssignment, ExtRoleAssignment>
{
    private readonly IDbExtendedConverter<Entity, ExtEntity> entityConverter;
    private readonly IDbExtendedConverter<Role, ExtRole> roleConverter;

    /// <summary>
    /// RoleAssignmentDbConverter Constructor
    /// </summary>
    public RoleAssignmentDbConverter(
        IDbExtendedConverter<Entity, ExtEntity> entityConverter,
        IDbExtendedConverter<Role, ExtRole> roleConverter
        )
    {
        this.entityConverter = entityConverter;
        this.roleConverter = roleConverter;
    }

    /// <inheritdoc/>
    public List<ExtRoleAssignment> ConvertExtended(IDataReader reader)
    {
        var result = new List<ExtRoleAssignment>();
        while (reader.Read())
        {
            result.Add(new ExtRoleAssignment()
            {
                Id = (Guid)reader["id"],
                ForId = (Guid)reader["forid"],
                RoleId = (Guid)reader["roleid"],
                ToId = (Guid)reader["toid"],
                For = entityConverter.ConvertSingleBasic(reader, "for_") ?? new Entity(),
                Role = roleConverter.ConvertSingleBasic(reader, "role_") ?? new Role(),
                To = entityConverter.ConvertSingleBasic(reader, "to_") ?? new Entity()
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public List<RoleAssignment> ConvertBasic(IDataReader reader)
    {
        var result = new List<RoleAssignment>();
        while (reader.Read())
        {
            result.Add(new RoleAssignment()
            {
                Id = (Guid)reader["id"],
                ForId = (Guid)reader["forid"],
                RoleId = (Guid)reader["roleid"],
                ToId = (Guid)reader["toid"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public RoleAssignment? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new RoleAssignment()
            {
                Id = (Guid)reader[$"{prefix}id"],
                ForId = (Guid)reader[$"{prefix}forid"],
                RoleId = (Guid)reader[$"{prefix}roleid"],
                ToId = (Guid)reader[$"{prefix}toid"],
            };
        }
        catch
        {
            return null;
        }
    }
}