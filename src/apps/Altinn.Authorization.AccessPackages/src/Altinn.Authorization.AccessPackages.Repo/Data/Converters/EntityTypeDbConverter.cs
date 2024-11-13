using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class EntityTypeDbConverter : IDbExtendedConverter<EntityType, ExtEntityType>
{
    private readonly IDbBasicConverter<Provider> providerConverter;

    /// <summary>
    /// EntityTypeDbConverter Constructor
    /// </summary>
    /// <param name="providerConverter">Provider converter</param>
    public EntityTypeDbConverter(IDbBasicConverter<Provider> providerConverter)
    {
        this.providerConverter = providerConverter;
    }

    /// <inheritdoc/>
    public List<ExtEntityType> ConvertExtended(IDataReader reader)
    {
        var result = new List<ExtEntityType>();
        while (reader.Read())
        {
            result.Add(new ExtEntityType()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                ProviderId = (Guid)reader["providerid"],
                Provider = providerConverter.ConvertSingleBasic(reader, "provider_") ?? new Provider()
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public List<EntityType> ConvertBasic(IDataReader reader)
    {
        var result = new List<EntityType>();
        while (reader.Read())
        {
            result.Add(new EntityType()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                ProviderId = (Guid)reader["providerid"]
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public EntityType? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new EntityType()
            {
                Id = (Guid)reader[$"{prefix}id"],
                Name = (string)reader[$"{prefix}name"],
                ProviderId = (Guid)reader[$"{prefix}providerid"]
            };
        }
        catch
        {
            return null;
        }
    }
}