using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;
using Altinn.AccessMgmt.AccessPackages.Repo.Ingest;
using Altinn.AccessMgmt.AccessPackages.Repo.Migrate;
using Altinn.AccessMgmt.AccessPackages.Repo.Mock;
using Altinn.AccessMgmt.DbAccess;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Models;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.DbAccess.Data.Services.Mssql;
using Altinn.AccessMgmt.DbAccess.Data.Services.Postgres;
using Altinn.AccessMgmt.DbAccess.Migrate.Contracts;
using Altinn.AccessMgmt.DbAccess.Migrate.Models;
using Altinn.AccessMgmt.DbAccess.Migrate.Services;
using Altinn.AccessMgmt.Models;
using Altinn.Authorization.Host.Lease;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Extensions;

public static class DbAccessHostExtensions
{
    public static IHostApplicationBuilder ConfigureDb(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<DbAccessConfig>(builder.Configuration.GetRequiredSection("DbAccessConfig"));

        builder.AddAltinnLease(opt =>
        {
            opt.Type = AltinnLeaseType.InMemory;
            //opt.Type = AltinnLeaseType.AzureStorageAccount;
            //opt.StorageAccount.Endpoint = new Uri("https://standreastest.blob.core.windows.net/");
        });

        return builder;
    }

    public static IHostApplicationBuilder AddDb(this IHostApplicationBuilder builder)
    {
        var config = builder.Configuration.Get<DbAccessConfig>();

        builder.Services.AddSingleton<DatabaseDefinitions>();
        builder.Services.AddSingleton<DatabaseMigration>();
        builder.Services.AddSingleton<DbConverter>();
        
        builder.Services.AddSingleton<JsonIngestFactory>();
        builder.Services.AddSingleton<Mockups>();

        /*
        TODO: Implement Enum for Postgres/Mssql switch. See AltinnLease.ConfigureAltinnLease
        */

        builder.Services.AddSingleton<IDbMigrationFactory, PostgresMigrationFactory>();
        RegisterPostgresDataRepo(builder.Services);

        //// builder.Services.AddSingleton<IDbMigrationFactory, SqlMigrationFactory>();
        //// RegisterSqlDataRepo(builder.Services);

        RegisterDbServices(builder.Services);

        return builder;
    }

    public static async Task<IHost> UseDb(this IHost host)
    {
        var definitions = host.Services.GetRequiredService<DatabaseDefinitions>();
        definitions.SetDatabaseDefinitions();

        var dbMigration = host.Services.GetRequiredService<DatabaseMigration>();
        await dbMigration.Init();

        var dbIngest = host.Services.GetRequiredService<JsonIngestFactory>();
        await dbIngest.IngestAll();

        var mockService = host.Services.GetService<Mockups>();
        if (mockService != null)
        {
            await mockService.Run();
            
        }

        return host;
    }

    private static void RegisterDbServices(IServiceCollection services)
    {
        #region Register Services
        services.AddSingleton<IPackageResourceService, PackageResourceDataService>();
        services.AddSingleton<IResourceService, ResourceDataService>();
        services.AddSingleton<IResourceGroupService, ResourceGroupDataService>();
        services.AddSingleton<IResourceTypeService, ResourceTypeDataService>();
        services.AddSingleton<IElementTypeService, ElementTypeDataService>();
        services.AddSingleton<IElementService, ElementDataService>();
        services.AddSingleton<IPolicyService, PolicyDataService>();
        services.AddSingleton<IPolicyElementService, PolicyElementDataService>();
        services.AddSingleton<IAreaService, AreaDataService>();
        services.AddSingleton<IAreaGroupService, AreaGroupDataService>();
        services.AddSingleton<IWorkerConfigService, WorkerConfigDataService>();
        services.AddSingleton<IEntityTypeService, EntityTypeDataService>();
        services.AddSingleton<IEntityVariantService, EntityVariantDataService>();
        services.AddSingleton<IPackageService, PackageDataService>();
        services.AddSingleton<IProviderService, ProviderDataService>();
        services.AddSingleton<IRoleService, RoleDataService>();
        services.AddSingleton<IRolePackageService, RolePackageDataService>();
        services.AddSingleton<IRoleResourceService, RoleResourceDataService>();
        services.AddSingleton<ITagGroupService, TagGroupDataService>();
        services.AddSingleton<IPackageTagService, PackageTagDataService>();
        services.AddSingleton<ITagService, TagDataService>();
        services.AddSingleton<IEntityService, EntityDataService>();
        services.AddSingleton<IEntityLookupService, EntityLookupDataService>();
        services.AddSingleton<IEntityVariantRoleService, EntityVariantRoleDataService>();
        services.AddSingleton<IRoleMapService, RoleMapDataService>();
        services.AddSingleton<IAssignmentService, AssignmentDataService>();
        services.AddSingleton<IAssignmentPackageService, AssignmentPackageDataService>();
        services.AddSingleton<IAssignmentResourceService, AssignmentResourceDataService>();
        services.AddSingleton<IGroupService, GroupDataService>();
        services.AddSingleton<IGroupMemberService, GroupMemberDataService>();
        services.AddSingleton<IGroupAdminService, GroupAdminDataService>();
        services.AddSingleton<IGroupDelegationService, GroupDelegationDataService>();
        services.AddSingleton<IDelegationService, DelegationDataService>();
        #endregion
    }

    private static void RegisterPostgresDataRepo(IServiceCollection services)
    {
        services.AddSingleton<IDbBasicRepo<WorkerConfig>, PostgresBasicRepo<WorkerConfig>>();
        services.AddSingleton<IDbExtendedRepo<Area, ExtArea>, PostgresExtendedRepo<Area, ExtArea>>();
        services.AddSingleton<IDbExtendedRepo<AreaGroup, ExtAreaGroup>, PostgresExtendedRepo<AreaGroup, ExtAreaGroup>>();
        services.AddSingleton<IDbExtendedRepo<Assignment, ExtAssignment>, PostgresExtendedRepo<Assignment, ExtAssignment>>();
        services.AddSingleton<IDbExtendedRepo<AssignmentPackage, ExtAssignmentPackage>, PostgresExtendedRepo<AssignmentPackage, ExtAssignmentPackage>>();
        services.AddSingleton<IDbExtendedRepo<AssignmentResource, ExtAssignmentResource>, PostgresExtendedRepo<AssignmentResource, ExtAssignmentResource>>();
        services.AddSingleton<IDbExtendedRepo<Entity, ExtEntity>, PostgresExtendedRepo<Entity, ExtEntity>>();
        services.AddSingleton<IDbExtendedRepo<EntityLookup, ExtEntityLookup>, PostgresExtendedRepo<EntityLookup, ExtEntityLookup>>();
        services.AddSingleton<IDbExtendedRepo<EntityType, ExtEntityType>, PostgresExtendedRepo<EntityType, ExtEntityType>>();
        services.AddSingleton<IDbExtendedRepo<EntityVariant, ExtEntityVariant>, PostgresExtendedRepo<EntityVariant, ExtEntityVariant>>();
        services.AddSingleton<IDbCrossRepo<EntityVariant, EntityVariantRole, Role>, PostgresCrossRepo<EntityVariant, EntityVariantRole, Role>>();
        services.AddSingleton<IDbExtendedRepo<EntityGroup, ExtEntityGroup>, PostgresExtendedRepo<EntityGroup, ExtEntityGroup>>();
        services.AddSingleton<IDbExtendedRepo<GroupAdmin, ExtGroupAdmin>, PostgresExtendedRepo<GroupAdmin, ExtGroupAdmin>>();
        services.AddSingleton<IDbExtendedRepo<GroupMember, ExtGroupMember>, PostgresExtendedRepo<GroupMember, ExtGroupMember>>();
        services.AddSingleton<IDbExtendedRepo<GroupDelegation, ExtGroupDelegation>, PostgresExtendedRepo<GroupDelegation, ExtGroupDelegation>>();
        services.AddSingleton<IDbExtendedRepo<Package, ExtPackage>, PostgresExtendedRepo<Package, ExtPackage>>();
        services.AddSingleton<IDbExtendedRepo<PackageResource, ExtPackageResource>, PostgresExtendedRepo<PackageResource, ExtPackageResource>>();
        services.AddSingleton<IDbCrossRepo<Package, PackageTag, Tag>, PostgresCrossRepo<Package, PackageTag, Tag>>();
        services.AddSingleton<IDbBasicRepo<Provider>, PostgresBasicRepo<Provider>>();
        services.AddSingleton<IDbExtendedRepo<Resource, ExtResource>, PostgresExtendedRepo<Resource, ExtResource>>();
        services.AddSingleton<IDbExtendedRepo<ResourceGroup, ExtResourceGroup>, PostgresExtendedRepo<ResourceGroup, ExtResourceGroup>>();
        services.AddSingleton<IDbBasicRepo<ResourceType>, PostgresBasicRepo<ResourceType>>();
        services.AddSingleton<IDbBasicRepo<ElementType>, PostgresBasicRepo<ElementType>>();
        services.AddSingleton<IDbExtendedRepo<Element, ExtElement>, PostgresExtendedRepo<Element, ExtElement>>();
        services.AddSingleton<IDbExtendedRepo<Policy, ExtPolicy>, PostgresExtendedRepo<Policy, ExtPolicy>>();
        services.AddSingleton<IDbCrossRepo<Policy, PolicyElement, Element>, PostgresCrossRepo<Policy, PolicyElement, Element>>();
        services.AddSingleton<IDbExtendedRepo<Role, ExtRole>, PostgresExtendedRepo<Role, ExtRole>>();
        services.AddSingleton<IDbExtendedRepo<RoleMap, ExtRoleMap>, PostgresExtendedRepo<RoleMap, ExtRoleMap>>();
        services.AddSingleton<IDbExtendedRepo<RolePackage, ExtRolePackage>, PostgresExtendedRepo<RolePackage, ExtRolePackage>>();
        services.AddSingleton<IDbCrossRepo<Role, RoleResource, Resource>, PostgresCrossRepo<Role, RoleResource, Resource>>();
        services.AddSingleton<IDbExtendedRepo<Tag, ExtTag>, PostgresExtendedRepo<Tag, ExtTag>>();
        services.AddSingleton<IDbBasicRepo<TagGroup>, PostgresBasicRepo<TagGroup>>();
        services.AddSingleton<IDbExtendedRepo<Delegation, ExtDelegation>, PostgresExtendedRepo<Delegation, ExtDelegation>>();
    }

    private static void RegisterSqlDataRepo(IServiceCollection services)
    {
        services.AddSingleton<IDbBasicRepo<WorkerConfig>, SqlBasicRepo<WorkerConfig>>();
        services.AddSingleton<IDbExtendedRepo<Area, ExtArea>, SqlExtendedRepo<Area, ExtArea>>();
        services.AddSingleton<IDbExtendedRepo<AreaGroup, ExtAreaGroup>, SqlExtendedRepo<AreaGroup, ExtAreaGroup>>();
        services.AddSingleton<IDbExtendedRepo<Assignment, ExtAssignment>, SqlExtendedRepo<Assignment, ExtAssignment>>();
        services.AddSingleton<IDbExtendedRepo<AssignmentPackage, ExtAssignmentPackage>, SqlExtendedRepo<AssignmentPackage, ExtAssignmentPackage>>();
        services.AddSingleton<IDbExtendedRepo<AssignmentResource, ExtAssignmentResource>, SqlExtendedRepo<AssignmentResource, ExtAssignmentResource>>();
        services.AddSingleton<IDbExtendedRepo<Entity, ExtEntity>, SqlExtendedRepo<Entity, ExtEntity>>();
        services.AddSingleton<IDbExtendedRepo<EntityLookup, ExtEntityLookup>, SqlExtendedRepo<EntityLookup, ExtEntityLookup>>();
        services.AddSingleton<IDbExtendedRepo<EntityType, ExtEntityType>, SqlExtendedRepo<EntityType, ExtEntityType>>();
        services.AddSingleton<IDbExtendedRepo<EntityVariant, ExtEntityVariant>, SqlExtendedRepo<EntityVariant, ExtEntityVariant>>();
        services.AddSingleton<IDbCrossRepo<EntityVariant, EntityVariantRole, Role>, SqlCrossRepo<EntityVariant, EntityVariantRole, Role>>();
        services.AddSingleton<IDbExtendedRepo<EntityGroup, ExtEntityGroup>, SqlExtendedRepo<EntityGroup, ExtEntityGroup>>();
        services.AddSingleton<IDbExtendedRepo<GroupAdmin, ExtGroupAdmin>, SqlExtendedRepo<GroupAdmin, ExtGroupAdmin>>();
        services.AddSingleton<IDbExtendedRepo<GroupMember, ExtGroupMember>, SqlExtendedRepo<GroupMember, ExtGroupMember>>();
        services.AddSingleton<IDbExtendedRepo<GroupDelegation, ExtGroupDelegation>, SqlExtendedRepo<GroupDelegation, ExtGroupDelegation>>();
        services.AddSingleton<IDbExtendedRepo<Package, ExtPackage>, SqlExtendedRepo<Package, ExtPackage>>();
        services.AddSingleton<IDbExtendedRepo<PackageResource, ExtPackageResource>, SqlExtendedRepo<PackageResource, ExtPackageResource>>();
        services.AddSingleton<IDbCrossRepo<Package, PackageTag, Tag>, SqlCrossRepo<Package, PackageTag, Tag>>();
        services.AddSingleton<IDbBasicRepo<Provider>, SqlBasicRepo<Provider>>();
        services.AddSingleton<IDbExtendedRepo<Resource, ExtResource>, SqlExtendedRepo<Resource, ExtResource>>();
        services.AddSingleton<IDbExtendedRepo<ResourceGroup, ExtResourceGroup>, SqlExtendedRepo<ResourceGroup, ExtResourceGroup>>();
        services.AddSingleton<IDbBasicRepo<ResourceType>, SqlBasicRepo<ResourceType>>();
        services.AddSingleton<IDbBasicRepo<ElementType>, SqlBasicRepo<ElementType>>();
        services.AddSingleton<IDbExtendedRepo<Element, ExtElement>, SqlExtendedRepo<Element, ExtElement>>();
        services.AddSingleton<IDbExtendedRepo<Policy, ExtPolicy>, SqlExtendedRepo<Policy, ExtPolicy>>();
        services.AddSingleton<IDbCrossRepo<Policy, PolicyElement, Element>, SqlCrossRepo<Policy, PolicyElement, Element>>();
        services.AddSingleton<IDbExtendedRepo<Role, ExtRole>, SqlExtendedRepo<Role, ExtRole>>();
        services.AddSingleton<IDbExtendedRepo<RoleMap, ExtRoleMap>, SqlExtendedRepo<RoleMap, ExtRoleMap>>();
        services.AddSingleton<IDbExtendedRepo<RolePackage, ExtRolePackage>, SqlExtendedRepo<RolePackage, ExtRolePackage>>();
        services.AddSingleton<IDbCrossRepo<Role, RoleResource, Resource>, SqlCrossRepo<Role, RoleResource, Resource>>();
        services.AddSingleton<IDbExtendedRepo<Tag, ExtTag>, SqlExtendedRepo<Tag, ExtTag>>();
        services.AddSingleton<IDbBasicRepo<TagGroup>, SqlBasicRepo<TagGroup>>();
        services.AddSingleton<IDbExtendedRepo<Delegation, ExtDelegation>, SqlExtendedRepo<Delegation, ExtDelegation>>();
    }
}
