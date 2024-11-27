using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Altinn.Authorization.AccessPackages.Models;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Extensions;

/// <summary>
/// Database Cache
/// </summary>
public class DatabaseDefinitions
{
    private DbObjDefConfig Config { get; set; }

    /// <summary>
    /// Database Cache
    /// </summary>
    /// <param name="options">DbObjDefConfig</param>
    public DatabaseDefinitions(IOptions<DbObjDefConfig> options)
    {
        Config = options.Value;
    }

    /// <summary>
    /// Use Database Cache
    /// </summary>
    public void SetDatabaseDefinitions()
    {
        DbDefinitions.Add<Area, ExtArea>(Config);
        DbDefinitions.Add<AreaGroup>(Config);

        DbDefinitions.Add<Entity, ExtEntity>(Config);
        DbDefinitions.Add<EntityType, ExtEntityType>(Config);
        DbDefinitions.Add<EntityVariant, ExtEntityVariant>(Config);
        DbDefinitions.Add<EntityVariantRole, ExtEntityVariantRole>(Config);

        DbDefinitions.Add<Package, ExtPackage>(Config);
        DbDefinitions.Add<PackageDelegation, ExtPackageDelegation>(Config);
        DbDefinitions.Add<PackageResource, ExtPackageResource>(Config);
        DbDefinitions.Add<PackageTag, ExtPackageTag>(Config);

        DbDefinitions.Add<Provider>(Config);

        DbDefinitions.Add<Resource, ExtResource>(Config);
        DbDefinitions.Add<ResourceGroup, ExtResourceGroup>(Config);
        DbDefinitions.Add<ResourceType>(Config);

        DbDefinitions.Add<Role, ExtRole>(Config);
        DbDefinitions.Add<RoleAssignment, ExtRoleAssignment>(Config);
        DbDefinitions.Add<RoleMap, ExtRoleMap>(Config);
        DbDefinitions.Add<RolePackage, ExtRolePackage>(Config);

        DbDefinitions.Add<Tag, ExtTag>(Config);
        DbDefinitions.Add<TagGroup>(Config);

        //// TODO: IVAR
        DbDefinitions.Add<Relation, ExtRelation>(Config);
        DbDefinitions.Add<RelationAssignment, ExtRelationAssignment>(Config);
    }
}
