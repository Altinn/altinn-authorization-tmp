using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class PackageResourceDbConverter : IDbExtendedConverter<PackageResource, ExtPackageResource>
{
    private readonly IDbExtendedConverter<Package, ExtPackage> packageConverter;
    private readonly IDbExtendedConverter<Resource, ExtResource> resourceConverter;

    /// <summary>
    /// PackageResourceDbConverter Constructor
    /// </summary>
    public PackageResourceDbConverter(
        IDbExtendedConverter<Package, ExtPackage> packageConverter,
        IDbExtendedConverter<Resource, ExtResource> resourceConverter
    )
    {
        this.packageConverter = packageConverter;
        this.resourceConverter = resourceConverter;
    }

    /// <inheritdoc/>
    public List<ExtPackageResource> ConvertExtended(IDataReader reader)
    {
        var result = new List<ExtPackageResource>();
        while (reader.Read())
        {
            result.Add(new ExtPackageResource()
            {
                Id = (Guid)reader["id"],
                PackageId = (Guid)reader["packageid"],
                ResourceId = (Guid)reader["resourceid"],
                Read = (bool)reader["read"],
                Write = (bool)reader["write"],
                Sign = (bool)reader["sign"],
                Package = packageConverter.ConvertSingleBasic(reader, "package_") ?? new Package(),
                Resource = resourceConverter.ConvertSingleBasic(reader, "resource_") ?? new Resource()
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public List<PackageResource> ConvertBasic(IDataReader reader)
    {
        var result = new List<PackageResource>();
        while (reader.Read())
        {
            result.Add(new PackageResource()
            {
                Id = (Guid)reader["id"],
                PackageId = (Guid)reader["packageid"],
                ResourceId = (Guid)reader["resourceid"],
                Read = (bool)reader["read"],
                Write = (bool)reader["write"],
                Sign = (bool)reader["sign"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public PackageResource? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new PackageResource()
            {
                Id = (Guid)reader[$"{prefix}id"],
                PackageId = (Guid)reader[$"{prefix}packageid"],
                ResourceId = (Guid)reader[$"{prefix}resourceid"],
                Read = (bool)reader[$"{prefix}read"],
                Write = (bool)reader[$"{prefix}write"],
                Sign = (bool)reader[$"{prefix}sign"],
            };
        }
        catch
        {
            return null;
        }
    }
}