using System.Data;
using System.Text.Json;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;
using Dapper;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class RoleDbConverter : IDbExtendedConverter<Role, ExtRole>
{
    private readonly IDbBasicConverter<Provider> providerConverter;
    private readonly IDbBasicConverter<EntityType> entityTypeConverter;

    /// <summary>
    /// RoleDbConverter Constructor
    /// </summary>
    public RoleDbConverter(
        IDbBasicConverter<Provider> providerConverter,
        IDbExtendedConverter<EntityType, ExtEntityType> entityTypeConverter)
    {
        this.providerConverter = providerConverter;
        this.entityTypeConverter = entityTypeConverter;
    }

    /// <inheritdoc/>
    public List<ExtRole> ConvertExtended(IDataReader reader)
    {
        var result = new List<ExtRole>();
        while (reader.Read())
        {
            result.Add(new ExtRole()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                Code = (string)reader["code"],
                Description = (string)reader["description"],
                Urn = (string)reader["urn"],
                EntityTypeId = (Guid)reader["entitytypeid"],
                ProviderId = (Guid)reader["providerid"],
                EntityType = entityTypeConverter.ConvertSingleBasic(reader, "entitytype_") ?? new EntityType(),
                Provider = providerConverter.ConvertSingleBasic(reader, "provider_") ?? new Provider()
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public List<Role> ConvertBasic(IDataReader reader)
    {
        var result = new List<Role>();
        while (reader.Read())
        {
            result.Add(new Role()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                Code = (string)reader["code"],
                Description = (string)reader["description"],
                Urn = (string)reader["urn"],
                EntityTypeId = (Guid)reader["entitytypeid"],
                ProviderId = (Guid)reader["providerid"],
            });
        }
        
        return result;
    }

    /// <inheritdoc/>
    public Role? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new Role()
            {
                Id = (Guid)reader[$"{prefix}id"],
                Name = (string)reader[$"{prefix}name"],
                Code = (string)reader[$"{prefix}code"],
                Description = (string)reader[$"{prefix}description"],
                Urn = (string)reader[$"{prefix}urn"],
                EntityTypeId = (Guid)reader[$"{prefix}entitytypeid"],
                ProviderId = (Guid)reader[$"{prefix}providerid"],
            };
        }
        catch
        {
            return null;
        }
    }
}