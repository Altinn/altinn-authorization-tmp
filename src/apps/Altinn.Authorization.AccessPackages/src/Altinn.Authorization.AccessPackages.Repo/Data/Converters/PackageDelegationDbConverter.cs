using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class PackageDelegationDbConverter<T> : IDbExtendedConverter<PackageDelegation, ExtPackageDelegation>
{
    private readonly IDbBasicConverter<Entity> entityConverter;
    private readonly IDbBasicConverter<Package> packageConverter;

    /// <summary>
    /// PackageDelegationDbConverter Constructor
    /// </summary>
    public PackageDelegationDbConverter(
        IDbBasicConverter<Entity> entityConverter,
        IDbBasicConverter<Package> packageConverter
        )
    {
        this.entityConverter = entityConverter;
        this.packageConverter = packageConverter;
    }

    /// <inheritdoc/>
    public List<ExtPackageDelegation> ConvertExtended(IDataReader reader)
    {
        var result = new List<ExtPackageDelegation>();
        while (reader.Read())
        {
            result.Add(new ExtPackageDelegation()
            {
                Id = (Guid)reader["id"],
                ById = (Guid)reader["byid"],
                ForId = (Guid)reader["forid"],
                ToId = (Guid)reader["toid"],
                PackageId = (Guid)reader["packageid"],
                By = entityConverter.ConvertSingleBasic(reader, "by_") ?? new Entity(),
                For = entityConverter.ConvertSingleBasic(reader, "for_") ?? new Entity(),
                To = entityConverter.ConvertSingleBasic(reader, "to_") ?? new Entity(),
                Package = packageConverter.ConvertSingleBasic(reader, "package_") ?? new Package()
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public List<PackageDelegation> ConvertBasic(IDataReader reader)
    {
        var result = new List<PackageDelegation>();
        while (reader.Read())
        {
            result.Add(new PackageDelegation()
            {
                Id = (Guid)reader["id"],
                ById = (Guid)reader["byid"],
                ForId = (Guid)reader["forid"],
                ToId = (Guid)reader["toid"],
                PackageId = (Guid)reader["packageid"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public PackageDelegation? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new PackageDelegation()
            {
                Id = (Guid)reader[$"{prefix}id"],
                ById = (Guid)reader[$"{prefix}byid"],
                ForId = (Guid)reader[$"{prefix}forid"],
                ToId = (Guid)reader[$"{prefix}toid"],
                PackageId = (Guid)reader[$"{prefix}packageid"],
            };
        } 
        catch
        {
            return null;
        }
    }
}