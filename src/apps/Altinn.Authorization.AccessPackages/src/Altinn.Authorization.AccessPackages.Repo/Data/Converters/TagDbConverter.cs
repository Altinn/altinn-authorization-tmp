using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class TagDbConverter : IDbExtendedConverter<Tag, ExtTag>
{
    private readonly IDbBasicConverter<TagGroup> tagGroupConverter;

    /// <summary>
    /// TagDbConverter Constructor
    /// </summary>
    public TagDbConverter(IDbBasicConverter<TagGroup> tagGroupConverter)
    {
        this.tagGroupConverter = tagGroupConverter;
    }

    /// <inheritdoc/>
    public List<ExtTag> ConvertExtended(IDataReader reader)
    {
        var result = new List<ExtTag>();
        while (reader.Read())
        {
            result.Add(new ExtTag()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                GroupId = DbUtil.ConvertDbValue<Guid>(reader["groupid"]),
                ParentId = DbUtil.ConvertDbValue<Guid>(reader["parentid"]),
                Group = tagGroupConverter.ConvertSingleBasic(reader, "group_") ?? null,
                Parent = ConvertSingleBasic(reader, "parent_") ?? null
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public List<Tag> ConvertBasic(IDataReader reader)
    {
        var result = new List<Tag>();
        while (reader.Read())
        {
            result.Add(new Tag()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                GroupId = DbUtil.ConvertDbValue<Guid>(reader["groupid"]),
                ParentId = DbUtil.ConvertDbValue<Guid>(reader["parentid"]),
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public Tag? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new Tag()
            {
                Id = (Guid)reader[$"{prefix}id"],
                Name = (string)reader[$"{prefix}name"],
                GroupId = DbUtil.ConvertDbValue<Guid>(reader[$"{prefix}groupid"]),
                ParentId = DbUtil.ConvertDbValue<Guid>(reader[$"{prefix}parentid"]),
            };
        }
        catch
        {
            return null;
        }
    }
}