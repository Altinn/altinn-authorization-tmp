using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class TagGroupDbConverter : IDbBasicConverter<TagGroup>
{
    /// <summary>
    /// TagGroupDbConverter Constructor
    /// </summary>
    public TagGroupDbConverter() { }

    /// <inheritdoc/>
    public List<TagGroup> ConvertBasic(IDataReader reader)
    {
        var result = new List<TagGroup>();
        while (reader.Read())
        {
            result.Add(new TagGroup()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public TagGroup? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new TagGroup()
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
