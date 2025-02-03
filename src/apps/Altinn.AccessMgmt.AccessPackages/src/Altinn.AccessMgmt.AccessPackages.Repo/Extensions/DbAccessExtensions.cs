using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;
using Altinn.AccessMgmt.AccessPackages.Repo.Ingest;
using Altinn.AccessMgmt.AccessPackages.Repo.Migrate;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Models;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.DbAccess.Data.Services.Mssql;
using Altinn.AccessMgmt.DbAccess.Data.Services.Postgres;
using Altinn.AccessMgmt.DbAccess.Migrate.Contracts;
using Altinn.AccessMgmt.DbAccess.Migrate.Models;
using Altinn.AccessMgmt.DbAccess.Migrate.Services;
using Altinn.AccessMgmt.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Extensions;

/// <summary>
/// DbAccess DI Extensions
/// </summary>
public static class DbAccessExtensions
{
    /// <summary>
    /// AddDatabaseDefinitions
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <param name="configureOptions">DbObjDefConfig</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddDatabaseDefinitions(this IHostApplicationBuilder builder, Action<DbObjDefConfig>? configureOptions = null)
    {
        builder.Services.Configure<DbObjDefConfig>(config =>
        {
            builder.Configuration.GetSection("DbObjDefConfig").Bind(config);
            configureOptions?.Invoke(config);
        });

        builder.Services.AddSingleton<DatabaseDefinitions>();

        return builder;
    }

    /// <summary>
    /// UseDatabaseDefinitions
    /// </summary>
    /// <param name="services">IServiceProvider</param>
    /// <returns></returns>
    public static IServiceProvider UseDatabaseDefinitions(this IServiceProvider services)
    {
        var definitions = services.GetRequiredService<DatabaseDefinitions>();
        definitions.SetDatabaseDefinitions();
        return services;
    }

    /// <summary>
    /// Adds DbAccess Migrations
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <param name="configureOptions">DbMigrationConfig</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddDbAccessMigrations(this IHostApplicationBuilder builder, Action<DbMigrationConfig>? configureOptions = null)
    {
        builder.Services.Configure<DbMigrationConfig>(config =>
        {
            builder.Configuration.GetSection("DbMigration").Bind(config);
            configureOptions?.Invoke(config);
        });

        var config = new DbMigrationConfig(config =>
        {
            builder.Configuration.GetSection("DbMigration").Bind(config);
            configureOptions?.Invoke(config);
        });

        if (config.UseSqlServer)
        {
            builder.Services.AddSingleton<IDbMigrationFactory, SqlMigrationFactory>();
        }
        else
        {
            builder.Services.AddSingleton<IDbMigrationFactory, PostgresMigrationFactory>();
        }

        builder.Services.AddSingleton<IDatabaseMigration, DatabaseMigration>();
        return builder;
    }

    /// <summary>
    /// Uses DbAccess Migrations
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <returns></returns>
    public async static Task<IServiceProvider> UseDbAccessMigrations(this IServiceProvider services)
    {
        var dbMigration = services.GetRequiredService<IDatabaseMigration>();
        await dbMigration.Init();
        return services;
    }

    /// <summary>
    /// Adds DbAccess Ingests
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <param name="configureOptions">JsonIngestConfig</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddJsonIngests(this IHostApplicationBuilder builder, Action<JsonIngestConfig>? configureOptions = null)
    {
        builder.Services.Configure<JsonIngestConfig>(config =>
        {
            builder.Configuration.GetSection("JsonIngest").Bind(config);
            config.BasePath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Ingest/JsonData/");
            configureOptions?.Invoke(config);
        });

        builder.Services.AddMetrics();
        builder.Services.AddSingleton<JsonIngestMeters>();
        builder.Services.AddSingleton<JsonIngestFactory>();
        return builder;
    }

    /// <summary>
    /// Uses DbAccess Ingests
    /// </summary>
    /// <param name="services">IServiceProvider</param>
    /// <returns></returns>
    public async static Task<IServiceProvider> UseJsonIngests(this IServiceProvider services)
    {
        var dbIngest = services.GetRequiredService<JsonIngestFactory>();
        await dbIngest.IngestAll();
        return services;
    }

    /// <summary>
    /// Adds DbAccess Data
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <param name="configureOptions">DbAccessDataConfig</param>
    /// <param name="telemetryOptions">TelemetryConfig</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddDbAccessData(this IHostApplicationBuilder builder, Action<DbAccessDataConfig>? configureOptions = null, Action<TelemetryConfig>? telemetryOptions = null)
    {
        builder.Services.Configure<DbAccessDataConfig>(config =>
        {
            builder.Configuration.GetSection("DataService").Bind(config);
            configureOptions?.Invoke(config);
        });

        var config = new DbAccessDataConfig(config =>
        {
            builder.Configuration.GetSection("DataService").Bind(config);
            configureOptions?.Invoke(config);
        });

        builder.Services.AddSingleton<DbConverter>();

        builder.AddDbAccessDataTelemetry();
        builder.AddDbAccessRepoTelemetry();

        if (config.UseSqlServer)
        {
            RegisterSqlDataRepo(builder.Services);
        }
        else
        {
            RegisterPostgresDataRepo(builder.Services);
        }

        #region Register Services
        
        builder.Services.AddSingleton<IPackageResourceService, PackageResourceDataService>();
        builder.Services.AddSingleton<IResourceService, ResourceDataService>();
        builder.Services.AddSingleton<IResourceGroupService, ResourceGroupDataService>();
        builder.Services.AddSingleton<IResourceTypeService, ResourceTypeDataService>();
        builder.Services.AddSingleton<IElementTypeService, ElementTypeDataService>();
        builder.Services.AddSingleton<IElementService, ElementDataService>();
        builder.Services.AddSingleton<IComponentService, ComponentDataService>();
        builder.Services.AddSingleton<IPolicyService, PolicyDataService>();
        builder.Services.AddSingleton<IPolicyComponentService, PolicyComponentDataService>();
        builder.Services.AddSingleton<IAreaService, AreaDataService>();
        builder.Services.AddSingleton<IAreaGroupService, AreaGroupDataService>();
        builder.Services.AddSingleton<IWorkerConfigService, WorkerConfigDataService>();
        builder.Services.AddSingleton<IEntityTypeService, EntityTypeDataService>();
        builder.Services.AddSingleton<IEntityVariantService, EntityVariantDataService>();
        builder.Services.AddSingleton<IPackageService, PackageDataService>();
        builder.Services.AddSingleton<IProviderService, ProviderDataService>();
        builder.Services.AddSingleton<IRoleService, RoleDataService>();
        builder.Services.AddSingleton<IRolePackageService, RolePackageDataService>();
        builder.Services.AddSingleton<IRoleResourceService, RoleResourceDataService>();
        builder.Services.AddSingleton<ITagGroupService, TagGroupDataService>();
        builder.Services.AddSingleton<IPackageTagService, PackageTagDataService>();
        builder.Services.AddSingleton<ITagService, TagDataService>();
        builder.Services.AddSingleton<IEntityService, EntityDataService>();
        builder.Services.AddSingleton<IEntityLookupService, EntityLookupDataService>();
        builder.Services.AddSingleton<IEntityVariantRoleService, EntityVariantRoleDataService>();
        builder.Services.AddSingleton<IRoleMapService, RoleMapDataService>();
        builder.Services.AddSingleton<IAssignmentService, AssignmentDataService>();
        builder.Services.AddSingleton<IAssignmentPackageService, AssignmentPackageDataService>();
        builder.Services.AddSingleton<IAssignmentResourceService, AssignmentResourceDataService>();
        builder.Services.AddSingleton<IGroupService, GroupDataService>();
        builder.Services.AddSingleton<IGroupMemberService, GroupMemberDataService>();
        builder.Services.AddSingleton<IGroupAdminService, GroupAdminDataService>();
        builder.Services.AddSingleton<IGroupDelegationService, GroupDelegationDataService>();
        builder.Services.AddSingleton<IPackageDelegationService, PackageDelegationDataService>();
        builder.Services.AddSingleton<IDelegationService, DelegationDataService>();
        builder.Services.AddSingleton<IDelegationResourceService, DelegationResourceDataService>();
        builder.Services.AddSingleton<IDelegationPackageResourceService, DelegationPackageResourceDataService>();
        #endregion

        return builder;
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
        services.AddSingleton<IDbExtendedRepo<PackageDelegation, ExtPackageDelegation>, PostgresExtendedRepo<PackageDelegation, ExtPackageDelegation>>();
        services.AddSingleton<IDbExtendedRepo<PackageResource, ExtPackageResource>, PostgresExtendedRepo<PackageResource, ExtPackageResource>>();
        services.AddSingleton<IDbCrossRepo<Package, PackageTag, Tag>, PostgresCrossRepo<Package, PackageTag, Tag>>();
        services.AddSingleton<IDbBasicRepo<Provider>, PostgresBasicRepo<Provider>>();
        services.AddSingleton<IDbExtendedRepo<Resource, ExtResource>, PostgresExtendedRepo<Resource, ExtResource>>();
        services.AddSingleton<IDbExtendedRepo<ResourceGroup, ExtResourceGroup>, PostgresExtendedRepo<ResourceGroup, ExtResourceGroup>>();
        services.AddSingleton<IDbBasicRepo<ResourceType>, PostgresBasicRepo<ResourceType>>();
        services.AddSingleton<IDbBasicRepo<ElementType>, PostgresBasicRepo<ElementType>>();
        services.AddSingleton<IDbExtendedRepo<Component, ExtComponent>, PostgresExtendedRepo<Component, ExtComponent>>();
        services.AddSingleton<IDbExtendedRepo<Element, ExtElement>, PostgresExtendedRepo<Element, ExtElement>>();
        services.AddSingleton<IDbExtendedRepo<Policy, ExtPolicy>, PostgresExtendedRepo<Policy, ExtPolicy>>();
        services.AddSingleton<IDbCrossRepo<Policy, PolicyComponent, Component>, PostgresCrossRepo<Policy, PolicyComponent, Component>>();
        services.AddSingleton<IDbExtendedRepo<Role, ExtRole>, PostgresExtendedRepo<Role, ExtRole>>();
        services.AddSingleton<IDbExtendedRepo<RoleMap, ExtRoleMap>, PostgresExtendedRepo<RoleMap, ExtRoleMap>>();
        services.AddSingleton<IDbExtendedRepo<RolePackage, ExtRolePackage>, PostgresExtendedRepo<RolePackage, ExtRolePackage>>();
        services.AddSingleton<IDbCrossRepo<Role, RoleResource, Resource>, PostgresCrossRepo<Role, RoleResource, Resource>>();
        services.AddSingleton<IDbExtendedRepo<Tag, ExtTag>, PostgresExtendedRepo<Tag, ExtTag>>();
        services.AddSingleton<IDbBasicRepo<TagGroup>, PostgresBasicRepo<TagGroup>>();
        services.AddSingleton<IDbExtendedRepo<Delegation, ExtDelegation>, PostgresExtendedRepo<Delegation, ExtDelegation>>();
        services.AddSingleton<IDbCrossRepo<Delegation, DelegationResource, Resource>, PostgresCrossRepo<Delegation, DelegationResource, Resource>>();
        services.AddSingleton<IDbCrossRepo<Delegation, DelegationPackageResource, PackageResource>, PostgresCrossRepo<Delegation, DelegationPackageResource, PackageResource>>();
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
        services.AddSingleton<IDbExtendedRepo<PackageDelegation, ExtPackageDelegation>, SqlExtendedRepo<PackageDelegation, ExtPackageDelegation>>();
        services.AddSingleton<IDbExtendedRepo<PackageResource, ExtPackageResource>, SqlExtendedRepo<PackageResource, ExtPackageResource>>();
        services.AddSingleton<IDbCrossRepo<Package, PackageTag, Tag>, SqlCrossRepo<Package, PackageTag, Tag>>();
        services.AddSingleton<IDbBasicRepo<Provider>, SqlBasicRepo<Provider>>();
        services.AddSingleton<IDbExtendedRepo<Resource, ExtResource>, SqlExtendedRepo<Resource, ExtResource>>();
        services.AddSingleton<IDbExtendedRepo<ResourceGroup, ExtResourceGroup>, SqlExtendedRepo<ResourceGroup, ExtResourceGroup>>();
        services.AddSingleton<IDbBasicRepo<ResourceType>, SqlBasicRepo<ResourceType>>();
        services.AddSingleton<IDbBasicRepo<ElementType>, SqlBasicRepo<ElementType>>();
        services.AddSingleton<IDbExtendedRepo<Component, ExtComponent>, SqlExtendedRepo<Component, ExtComponent>>();
        services.AddSingleton<IDbExtendedRepo<Element, ExtElement>, SqlExtendedRepo<Element, ExtElement>>();
        services.AddSingleton<IDbExtendedRepo<Policy, ExtPolicy>, SqlExtendedRepo<Policy, ExtPolicy>>();
        services.AddSingleton<IDbCrossRepo<Policy, PolicyComponent, Component>, SqlCrossRepo<Policy, PolicyComponent, Component>>();
        services.AddSingleton<IDbExtendedRepo<Role, ExtRole>, SqlExtendedRepo<Role, ExtRole>>();
        services.AddSingleton<IDbExtendedRepo<RoleMap, ExtRoleMap>, SqlExtendedRepo<RoleMap, ExtRoleMap>>();
        services.AddSingleton<IDbExtendedRepo<RolePackage, ExtRolePackage>, SqlExtendedRepo<RolePackage, ExtRolePackage>>();
        services.AddSingleton<IDbCrossRepo<Role, RoleResource, Resource>, SqlCrossRepo<Role, RoleResource, Resource>>();
        services.AddSingleton<IDbExtendedRepo<Tag, ExtTag>, SqlExtendedRepo<Tag, ExtTag>>();
        services.AddSingleton<IDbBasicRepo<TagGroup>, SqlBasicRepo<TagGroup>>();
        services.AddSingleton<IDbExtendedRepo<Delegation, ExtDelegation>, SqlExtendedRepo<Delegation, ExtDelegation>>();
        services.AddSingleton<IDbCrossRepo<Delegation, DelegationResource, Resource>, SqlCrossRepo<Delegation, DelegationResource, Resource>>();
        services.AddSingleton<IDbCrossRepo<Delegation, DelegationPackageResource, PackageResource>, SqlCrossRepo<Delegation, DelegationPackageResource, PackageResource>>();
    }
}
