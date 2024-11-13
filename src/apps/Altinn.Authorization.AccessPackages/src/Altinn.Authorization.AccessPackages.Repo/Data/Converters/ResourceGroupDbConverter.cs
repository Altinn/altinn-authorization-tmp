using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class ResourceGroupDbConverter : IDbExtendedConverter<ResourceGroup, ExtResourceGroup>
{
    private readonly IDbBasicConverter<Provider> providerConverter;

    /// <summary>
    /// ResourceGroupDbConverter Constructor
    /// </summary>
    public ResourceGroupDbConverter(IDbBasicConverter<Provider> providerConverter)
    {
        this.providerConverter = providerConverter;
    }

    /// <inheritdoc/>
    public List<ExtResourceGroup> ConvertExtended(IDataReader reader)
    {
        var result = new List<ExtResourceGroup>();
        while (reader.Read())
        {
            result.Add(new ExtResourceGroup()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                ProviderId = (Guid)reader["providerid"],
                Provider = providerConverter.ConvertSingleBasic(reader, "provider_") ?? new Provider(),
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public List<ResourceGroup> ConvertBasic(IDataReader reader)
    {
        var result = new List<ResourceGroup>();
        while (reader.Read())
        {
            result.Add(new ResourceGroup()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                ProviderId = (Guid)reader["providerid"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public ResourceGroup? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new ResourceGroup()
            {
                Id = (Guid)reader[$"{prefix}id"],
                Name = (string)reader[$"{prefix}name"],
                ProviderId = (Guid)reader[$"{prefix}providerid"],
            };
        }
        catch
        {
            return null;
        }
    }
}