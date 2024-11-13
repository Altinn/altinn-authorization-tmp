using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class PackageTagDbConverter : IDbCrossConverter<Package, PackageTag, Tag>
{
    private readonly IDbExtendedConverter<Package, ExtPackage> packageConverter;
    private readonly IDbExtendedConverter<Tag, ExtTag> tagConverter;

    /// <summary>
    /// PackageTagDbConverter Constructor
    /// </summary>
    public PackageTagDbConverter(
        IDbExtendedConverter<Package, ExtPackage> packageConverter,
        IDbExtendedConverter<Tag, ExtTag> tagConverter
        )
    {
        this.packageConverter = packageConverter;
        this.tagConverter = tagConverter;
    }

    /// <inheritdoc/>
    public List<PackageTag> ConvertBasic(IDataReader reader)
    {
        var result = new List<PackageTag>();
        while (reader.Read())
        {
            result.Add(new PackageTag()
            {
                Id = (Guid)reader["id"],
                PackageId = (Guid)reader["packageid"],
                TagId = (Guid)reader["tagid"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public PackageTag? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new PackageTag()
            {
                Id = (Guid)reader[$"{prefix}id"],
                PackageId = (Guid)reader[$"{prefix}packageid"],
                TagId = (Guid)reader[$"{prefix}tagid"],
            };
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public List<Package> ConvertA(IDataReader reader)
    {
        return packageConverter.ConvertBasic(reader);
    }

    /// <inheritdoc/>
    public List<Tag> ConvertB(IDataReader reader)
    {
        return tagConverter.ConvertBasic(reader);
    }
}