using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class ResourceDbConverter : IDbExtendedConverter<Resource, ExtResource>
{
    private readonly IDbExtendedConverter<ResourceGroup, ExtResourceGroup> resourceGroupConverter;
    private readonly IDbBasicConverter<ResourceType> resourceTypeConverter;
    private readonly IDbBasicConverter<Provider> providerConverter;

    /// <summary>
    /// ResourceDbConverter Constructor
    /// </summary>
    public ResourceDbConverter(
        IDbExtendedConverter<ResourceGroup, ExtResourceGroup> resourceGroupConverter,
        IDbBasicConverter<ResourceType> resourceTypeConverter,
        IDbBasicConverter<Provider> providerConverter
    )
    {
        this.resourceGroupConverter = resourceGroupConverter;
        this.resourceTypeConverter = resourceTypeConverter;
        this.providerConverter = providerConverter;
    }

    /// <inheritdoc/>
    public List<ExtResource> ConvertExtended(IDataReader reader)
    {
        var result = new List<ExtResource>();
        while (reader.Read())
        {
            result.Add(new ExtResource()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                GroupId = (Guid)reader["groupid"],
                TypeId = (Guid)reader["typeid"],
                ProviderId = (Guid)reader["providerid"],
                Group = resourceGroupConverter.ConvertSingleBasic(reader, "group_") ?? new ResourceGroup(),
                Type = resourceTypeConverter.ConvertSingleBasic(reader, "type_") ?? new ResourceType(),
                Provider = providerConverter.ConvertSingleBasic(reader, "provider_") ?? new Provider()
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public List<Resource> ConvertBasic(IDataReader reader)
    {
        var result = new List<Resource>();
        while (reader.Read())
        {
            result.Add(new Resource()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                GroupId = (Guid)reader["groupid"],
                TypeId = (Guid)reader["typeid"],
                ProviderId = (Guid)reader["providerid"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public Resource? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new Resource()
            {
                Id = (Guid)reader[$"{prefix}id"],
                Name = (string)reader[$"{prefix}name"],
                GroupId = (Guid)reader[$"{prefix}groupid"],
                TypeId = (Guid)reader[$"{prefix}typeid"],
                ProviderId = (Guid)reader[$"{prefix}providerid"],
            };
        }
        catch
        {
            return null;
        }
    }
}
