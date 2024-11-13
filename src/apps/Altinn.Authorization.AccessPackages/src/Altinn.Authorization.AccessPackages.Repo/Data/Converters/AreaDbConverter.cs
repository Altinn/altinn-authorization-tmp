using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class AreaDbConverter : IDbExtendedConverter<Area, ExtArea>
{
    private readonly IDbBasicConverter<AreaGroup> areaGroupConverter;

    /// <summary>
    /// AreaDbConverter Constructor
    /// </summary>
    public AreaDbConverter(IDbBasicConverter<AreaGroup> areaGroupConverter) 
    {
        this.areaGroupConverter = areaGroupConverter;
    }

    /// <inheritdoc/>
    public List<Area> ConvertBasic(IDataReader reader)
    {
        var result = new List<Area>();
        while (reader.Read())
        {
            result.Add(new Area()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                Description = (string)reader["description"],
                IconName = (string)reader["iconname"],
                GroupId = (Guid)reader["groupid"]
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public List<ExtArea> ConvertExtended(IDataReader reader)
    {
        var result = new List<ExtArea>();
        while (reader.Read())
        {
            result.Add(new ExtArea()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                Description = (string)reader["description"],
                IconName = (string)reader["iconname"],
                GroupId = (Guid)reader["groupid"],
                Group = areaGroupConverter.ConvertSingleBasic(reader, "group_") ?? new AreaGroup()
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public Area? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new Area()
            {
                Id = (Guid)reader[$"{prefix}id"],
                Name = (string)reader[$"{prefix}name"],
                Description = (string)reader[$"{prefix}description"],
                IconName = (string)reader[$"{prefix}iconname"],
                GroupId = (Guid)reader[$"{prefix}groupid"],
            };
        } 
        catch
        {
            return null;
        }
    }
}
