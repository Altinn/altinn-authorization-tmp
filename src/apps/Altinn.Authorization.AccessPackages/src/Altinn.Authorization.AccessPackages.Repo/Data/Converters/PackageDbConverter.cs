using System.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Converters;

/// <inheritdoc/>
public class PackageDbConverter : IDbExtendedConverter<Package, ExtPackage>
{
    private readonly IDbBasicConverter<Provider> providerConverter;
    private readonly IDbExtendedConverter<Area, ExtArea> areaConverter;
    private readonly IDbExtendedConverter<EntityType, ExtEntityType> entityTypeConverter;

    /// <summary>
    /// PackageDbConverter Constructor
    /// </summary>
    public PackageDbConverter(
        IDbBasicConverter<Provider> providerConverter,
        IDbExtendedConverter<Area, ExtArea> areaConverter,
        IDbExtendedConverter<EntityType, ExtEntityType> entityTypeConverter
        )
    {
        this.providerConverter = providerConverter;
        this.areaConverter = areaConverter;
        this.entityTypeConverter = entityTypeConverter;
    }

    /// <inheritdoc/>
    public List<ExtPackage> ConvertExtended(IDataReader reader)
    {
        var result = new List<ExtPackage>();
        while (reader.Read())
        {
            result.Add(new ExtPackage()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                Description = (string)reader["description"],
                IsDelegable = (bool)reader["isdelegable"],
                ProviderId = (Guid)reader["providerid"],
                AreaId = (Guid)reader["areaid"],
                EntityTypeId = (Guid)reader["entitytypeid"],
                Provider = providerConverter.ConvertSingleBasic(reader, "provider_") ?? new Provider(),
                Area = areaConverter.ConvertSingleBasic(reader, "area_") ?? new Area(),
                EntityType = entityTypeConverter.ConvertSingleBasic(reader, "entitytype_") ?? new EntityType()
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public List<Package> ConvertBasic(IDataReader reader)
    {
        var result = new List<Package>();
        while (reader.Read())
        {
            result.Add(new Package()
            {
                Id = (Guid)reader["id"],
                Name = (string)reader["name"],
                Description = (string)reader["description"],
                IsDelegable = (bool)reader["isdelegable"],
                ProviderId = (Guid)reader["providerid"],
                AreaId = (Guid)reader["areaid"],
                EntityTypeId = (Guid)reader["entitytypeid"],
            });
        }

        return result;
    }

    /// <inheritdoc/>
    public Package? ConvertSingleBasic(IDataReader reader, string prefix)
    {
        try
        {
            return new Package()
            {
                Id = (Guid)reader[$"{prefix}id"],
                Name = (string)reader[$"{prefix}name"],
                Description = (string)reader[$"{prefix}description"],
                IsDelegable = (bool)reader[$"{prefix}isdelegable"],
                ProviderId = (Guid)reader[$"{prefix}providerid"],
                AreaId = (Guid)reader[$"{prefix}areaid"],
                EntityTypeId = (Guid)reader[$"{prefix}entitytypeid"],
            };
        }
        catch
        {
            return null;
        }
    }
}