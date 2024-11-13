using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class ResourceTypeDbConverter : IDbBasicConverter<ResourceType>
{
    /// <summary>
    /// ResourceTypeDbConverter Constructor
    /// </summary>
    public ResourceTypeDbConverter() { }

    /// <inheritdoc/>
    public List<ResourceType> ConvertBasic(IDataReader reader)
    {
        var result = new List<ResourceType>();
        while (reader.Read())
        {
            result.Add(new ResourceType()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public ResourceType? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new ResourceType()
            {
                Id = (Guid)reader[$"{prefix}id"],
                Name = (string)reader[$"{prefix}name"],
            };
        }
        catch
        {
            return null;
        }
    }
}