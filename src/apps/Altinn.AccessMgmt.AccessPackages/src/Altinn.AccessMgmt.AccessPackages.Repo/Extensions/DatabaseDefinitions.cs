using Altinn.AccessMgmt.DbAccess.Data.Models;
using Altinn.AccessMgmt.Models;
using Microsoft.Extensions.Options;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Extensions;

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
        DbDefinitions.Add<WorkerConfig>(Config, useTranslation: false, useHistory: false);

        DbDefinitions.Add<Area, ExtArea>(Config, useTranslation: true, useHistory: true);
        DbDefinitions.Add<AreaGroup, ExtAreaGroup>(Config, useTranslation: true, useHistory: true);

        DbDefinitions.Add<Entity, ExtEntity>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<EntityLookup, ExtEntityLookup>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<EntityType, ExtEntityType>(Config, useTranslation: true, useHistory: true);
        DbDefinitions.Add<EntityVariant, ExtEntityVariant>(Config, useTranslation: true, useHistory: true);
        DbDefinitions.Add<EntityVariantRole, ExtEntityVariantRole>(Config, useTranslation: false, useHistory: true);

        DbDefinitions.Add<Package, ExtPackage>(Config, useTranslation: true, useHistory: true);
        DbDefinitions.Add<PackageResource, ExtPackageResource>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<PackageTag, ExtPackageTag>(Config, useTranslation: false, useHistory: true);

        DbDefinitions.Add<Provider>(Config, useTranslation: false, useHistory: true);

        DbDefinitions.Add<Resource, ExtResource>(Config, useTranslation: true, useHistory: true);
        DbDefinitions.Add<ResourceGroup, ExtResourceGroup>(Config, useTranslation: true, useHistory: true);
        DbDefinitions.Add<ResourceType>(Config, useTranslation: true, useHistory: true);
        DbDefinitions.Add<ElementType>(Config, useTranslation: true, useHistory: true);
        DbDefinitions.Add<Element>(Config, useTranslation: true, useHistory: true);
        DbDefinitions.Add<Component>(Config, useTranslation: true, useHistory: true);
        DbDefinitions.Add<Policy>(Config, useTranslation: true, useHistory: true);
        DbDefinitions.Add<PolicyComponent>(Config, useTranslation: false, useHistory: true);

        DbDefinitions.Add<Role, ExtRole>(Config, useTranslation: true, useHistory: true);
        DbDefinitions.Add<RoleMap, ExtRoleMap>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<RolePackage, ExtRolePackage>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<RoleResource, ExtRoleResource>(Config, useTranslation: false, useHistory: true);

        DbDefinitions.Add<Tag, ExtTag>(Config, useTranslation: true, useHistory: true);
        DbDefinitions.Add<TagGroup>(Config, useTranslation: true, useHistory: true);

        DbDefinitions.Add<Assignment, ExtAssignment>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<AssignmentPackage, ExtAssignmentPackage>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<AssignmentResource, ExtAssignmentResource>(Config, useTranslation: false, useHistory: true);

        DbDefinitions.Add<EntityGroup, ExtEntityGroup>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<GroupMember, ExtGroupMember>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<GroupAdmin, ExtGroupAdmin>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<GroupDelegation, ExtGroupDelegation>(Config, useTranslation: false, useHistory: true);

        DbDefinitions.Add<Delegation, ExtDelegation>(Config, useTranslation: false, useHistory: true);

        DbDefinitions.Add<DelegationAssignmentPackageResource, ExtDelegationAssignmentPackageResource>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<DelegationAssignmentResource, ExtDelegationAssignmentResource>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<DelegationAssignmentPackage, ExtDelegationAssignmentPackage>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<DelegationRolePackageResource, ExtDelegationRolePackageResource>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<DelegationRoleResource, ExtDelegationRoleResource>(Config, useTranslation: false, useHistory: true);
        DbDefinitions.Add<DelegationRolePackage, ExtDelegationRolePackage>(Config, useTranslation: false, useHistory: true);
    }
}
