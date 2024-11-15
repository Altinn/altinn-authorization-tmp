using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Altinn.Authorization.AccessPackages.Models;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Extensions;

/// <summary>
/// Database Definitions
/// </summary>
public class DatabaseDefinitions
{
    private DbObjDefConfig Config { get; set; }

    /// <summary>
    /// Database Definitions
    /// </summary>
    /// <param name="options">DbObjDefConfig</param>
    public DatabaseDefinitions(IOptions<DbObjDefConfig> options)
    {
        Config = options.Value;
    }

    /// <summary>
    /// Use Database Definitions
    /// </summary>
    public void SetDatabaseDefinitions()
    {
        DbDefinitions.Add<Area>(Config);
        DbDefinitions.Add<AreaGroup>(Config);

        DbDefinitions.Add<Entity>(Config);
        DbDefinitions.Add<EntityType>(Config);
        DbDefinitions.Add<EntityVariant>(Config);
        DbDefinitions.Add<EntityVariantRole>(Config);

        DbDefinitions.Add<Package>(Config);
        DbDefinitions.Add<PackageDelegation>(Config);
        DbDefinitions.Add<PackageResource>(Config);
        DbDefinitions.Add<PackageTag>(Config);

        DbDefinitions.Add<Provider>(Config);

        DbDefinitions.Add<Resource>(Config);
        DbDefinitions.Add<ResourceGroup>(Config);
        DbDefinitions.Add<ResourceType>(Config);

        DbDefinitions.Add<Role>(Config);
        DbDefinitions.Add<RoleAssignment>(Config);
        DbDefinitions.Add<RoleMap>(Config);
        DbDefinitions.Add<RolePackage>(Config);

        DbDefinitions.Add<Tag>(Config);
        DbDefinitions.Add<TagGroup>(Config);
    }
}
