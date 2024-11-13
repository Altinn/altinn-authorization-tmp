using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class AreaGroupDbConverter : IDbBasicConverter<AreaGroup>
{
    /// <inheritdoc/>
    public List<AreaGroup> ConvertBasic(IDataReader reader)
    {
        var result = new List<AreaGroup>();
        while (reader.Read())
        {
            result.Add(new AreaGroup()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                Description = (string)reader["description"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public AreaGroup? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new AreaGroup()
            {
                Id = (Guid)reader[$"{prefix}id"],
                Name = (string)reader[$"{prefix}name"],
                Description = (string)reader[$"{prefix}description"],
            };
        }
        catch
        {
            return null;
        }
    }
}