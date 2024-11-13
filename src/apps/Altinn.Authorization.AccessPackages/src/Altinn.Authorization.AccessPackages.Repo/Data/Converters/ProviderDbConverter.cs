using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class ProviderDbConverter : IDbBasicConverter<Provider>
{
    /// <summary>
    /// ProviderDbConverter Constructor
    /// </summary>
    public ProviderDbConverter() { }

    /// <inheritdoc/>
    public List<Provider> ConvertBasic(IDataReader reader)
    {
        var result = new List<Provider>();
        while (reader.Read())
        {
            result.Add(new Provider()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public Provider? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new Provider()
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
