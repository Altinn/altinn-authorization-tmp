using Altinn.Authorization.AccessPackages.DbAccess.Data;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services.Mssql;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services.Postgres;
using Altinn.Authorization.AccessPackages.DbAccess.Migrate.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Migrate.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Altinn.Authorization.AccessPackages.Repo.Data.Converters;
using Altinn.Authorization.AccessPackages.Repo.Data.Services;
using Altinn.Authorization.AccessPackages.Repo.Ingest;
using Altinn.Authorization.AccessPackages.Repo.Migrate;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Extensions;

/// <summary>
/// DbAccess DI Extensions
/// </summary>
public static class DbAccessExtensions
{
    /// <summary>
    /// Adds DbAccess Migrations
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="useSqlServer">Use Mssql or Postgres</param>
    /// <returns></returns>
    public static IServiceCollection AddDbAccessMigrations(this IServiceCollection services, bool useSqlServer = false)
    {
        if (useSqlServer)
        {
            services.AddSingleton<IDbMigrationFactory, SqlMigrationFactory>();
        }
        else
        {
            services.AddSingleton<IDbMigrationFactory, PostgresMigrationFactory>();
        }

        services.AddSingleton<IDatabaseMigration, DatabaseMigration>();
        return services;
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
    /// <param name="services">IServiceCollection</param>
    /// <returns></returns>
    public static IServiceCollection AddDbAccessIngests(this IServiceCollection services)
    {
        services.AddSingleton<IDatabaseIngest, JsonIngestFactory>();
        return services;
    }

    /// <summary>
    /// Uses DbAccess Ingests
    /// </summary>
    /// <param name="services">IServiceProvider</param>
    /// <returns></returns>
    public async static Task<IServiceProvider> UseDbAccessIngests(this IServiceProvider services)
    {
        var dbIngest = services.GetRequiredService<IDatabaseIngest>();
        await dbIngest.IngestAll();
        return services;
    }

    /// <summary>
    /// Adds DbAccess Data
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="useSqlServer">Use SqlServer or Postgres</param>
    /// <returns></returns>
    public static IServiceCollection AddDbAccessData(this IServiceCollection services, bool useSqlServer = false)
    {
        #region Register Converters
        services.AddSingleton<IDbExtendedConverter<PackageResource, ExtPackageResource>, PackageResourceDbConverter>();
        services.AddSingleton<IDbExtendedConverter<Resource, ExtResource>, ResourceDbConverter>();
        services.AddSingleton<IDbBasicConverter<ResourceType>, ResourceTypeDbConverter>();
        services.AddSingleton<IDbExtendedConverter<ResourceGroup, ExtResourceGroup>, ResourceGroupDbConverter>();
        services.AddSingleton<IDbExtendedConverter<Area, ExtArea>, AreaDbConverter>();
        services.AddSingleton<IDbBasicConverter<AreaGroup>, AreaGroupDbConverter>();
        services.AddSingleton<IDbBasicConverter<Provider>, ProviderDbConverter>();
        services.AddSingleton<IDbExtendedConverter<EntityType, ExtEntityType>, EntityTypeDbConverter>();
        services.AddSingleton<IDbExtendedConverter<EntityVariant, ExtEntityVariant>, EntityVariantDbConverter>();
        services.AddSingleton<IDbExtendedConverter<Package, ExtPackage>, PackageDbConverter>();
        services.AddSingleton<IDbExtendedConverter<Role, ExtRole>, RoleDbConverter>();
        services.AddSingleton<IDbExtendedConverter<RolePackage, ExtRolePackage>, RolePackageDbConverter>();
        services.AddSingleton<IDbBasicConverter<TagGroup>, TagGroupDbConverter>();
        services.AddSingleton<IDbCrossConverter<Package, PackageTag, Tag>, PackageTagDbConverter>();
        services.AddSingleton<IDbExtendedConverter<Tag, ExtTag>, TagDbConverter>();
        services.AddSingleton<IDbExtendedConverter<Entity, ExtEntity>, EntityDbConverter>();
        services.AddSingleton<IDbExtendedConverter<RoleAssignment, ExtRoleAssignment>, RoleAssignmentDbConverter>();
        services.AddSingleton<IDbCrossConverter<EntityVariant, EntityVariantRole, Role>, EntityVariantRoleDbConverter>();
        services.AddSingleton<IDbExtendedConverter<RoleMap, ExtRoleMap>, RoleMapDbConverter>();
        #endregion

        #region Register Data
        if (useSqlServer)
        {
            RegisterSqlDataRepo(services);
        }
        else
        {
            RegisterPostgresDataRepo(services);
        }
        #endregion

        #region Register Services
        services.AddSingleton<IPackageResourceService, PackageResourceDataService>();
        services.AddSingleton<IResourceService, ResourceDataService>();
        services.AddSingleton<IResourceGroupService, ResourceGroupDataService>();
        services.AddSingleton<IResourceTypeService, ResourceTypeDataService>();
        services.AddSingleton<IAreaService, AreaDataService>();
        services.AddSingleton<IAreaGroupService, AreaGroupDataService>();
        services.AddSingleton<IEntityTypeService, EntityTypeDataService>();
        services.AddSingleton<IEntityVariantService, EntityVariantDataService>();
        services.AddSingleton<IPackageService, PackageDataService>();
        services.AddSingleton<IProviderService, ProviderDataService>();
        services.AddSingleton<IRoleService, RoleDataService>();
        services.AddSingleton<IRolePackageService, RolePackageDataService>();
        services.AddSingleton<ITagGroupService, TagGroupDataService>();
        services.AddSingleton<IPackageTagService, PackageTagDataService>();
        services.AddSingleton<ITagService, TagDataService>();
        services.AddSingleton<IEntityService, EntityDataService>();
        services.AddSingleton<IRoleAssignmentService, RoleAssignmentDataService>();
        services.AddSingleton<IEntityVariantRoleService, EntityVariantRoleDataService>();
        services.AddSingleton<IRoleMapService, RoleMapDataService>();
        #endregion

        return services;
    }

    private static void RegisterPostgresDataRepo(IServiceCollection services)
    {
        services.AddSingleton<IDbExtendedRepo<PackageResource, ExtPackageResource>, PostgresExtendedRepo<PackageResource, ExtPackageResource>>();
        services.AddSingleton<IDbExtendedRepo<Resource, ExtResource>, PostgresExtendedRepo<Resource, ExtResource>>();
        services.AddSingleton<IDbExtendedRepo<ResourceGroup, ExtResourceGroup>, PostgresExtendedRepo<ResourceGroup, ExtResourceGroup>>();
        services.AddSingleton<IDbBasicRepo<ResourceType>, PostgresBasicRepo<ResourceType>>();
        services.AddSingleton<IDbExtendedRepo<Area, ExtArea>, PostgresExtendedRepo<Area, ExtArea>>();
        services.AddSingleton<IDbBasicRepo<AreaGroup>, PostgresBasicRepo<AreaGroup>>();
        services.AddSingleton<IDbBasicRepo<Provider>, PostgresBasicRepo<Provider>>();
        services.AddSingleton<IDbExtendedRepo<EntityType, ExtEntityType>, PostgresExtendedRepo<EntityType, ExtEntityType>>();
        services.AddSingleton<IDbExtendedRepo<EntityVariant, ExtEntityVariant>, PostgresExtendedRepo<EntityVariant, ExtEntityVariant>>();
        services.AddSingleton<IDbExtendedRepo<Package, ExtPackage>, PostgresExtendedRepo<Package, ExtPackage>>();
        services.AddSingleton<IDbExtendedRepo<Role, ExtRole>, PostgresExtendedRepo<Role, ExtRole>>();
        services.AddSingleton<IDbExtendedRepo<RolePackage, ExtRolePackage>, PostgresExtendedRepo<RolePackage, ExtRolePackage>>();
        services.AddSingleton<IDbBasicRepo<TagGroup>, PostgresBasicRepo<TagGroup>>();
        services.AddSingleton<IDbCrossRepo<Package, PackageTag, Tag>, PostgresCrossRepo<Package, PackageTag, Tag>>();
        services.AddSingleton<IDbExtendedRepo<Tag, ExtTag>, PostgresExtendedRepo<Tag, ExtTag>>();
        services.AddSingleton<IDbExtendedRepo<Entity, ExtEntity>, PostgresExtendedRepo<Entity, ExtEntity>>();
        services.AddSingleton<IDbExtendedRepo<RoleAssignment, ExtRoleAssignment>, PostgresExtendedRepo<RoleAssignment, ExtRoleAssignment>>();
        services.AddSingleton<IDbCrossRepo<EntityVariant, EntityVariantRole, Role>, PostgresCrossRepo<EntityVariant, EntityVariantRole, Role>>();
        services.AddSingleton<IDbExtendedRepo<RoleMap, ExtRoleMap>, PostgresExtendedRepo<RoleMap, ExtRoleMap>>();
    }

    private static void RegisterSqlDataRepo(IServiceCollection services)
    {
        services.AddSingleton<IDbExtendedRepo<PackageResource, ExtPackageResource>, SqlExtendedRepo<PackageResource, ExtPackageResource>>();
        services.AddSingleton<IDbExtendedRepo<Resource, ExtResource>, SqlExtendedRepo<Resource, ExtResource>>();
        services.AddSingleton<IDbExtendedRepo<ResourceGroup, ExtResourceGroup>, SqlExtendedRepo<ResourceGroup, ExtResourceGroup>>();
        services.AddSingleton<IDbBasicRepo<ResourceType>, SqlBasicRepo<ResourceType>>();
        services.AddSingleton<IDbExtendedRepo<Area, ExtArea>, SqlExtendedRepo<Area, ExtArea>>();
        services.AddSingleton<IDbBasicRepo<AreaGroup>, SqlBasicRepo<AreaGroup>>();
        services.AddSingleton<IDbBasicRepo<Provider>, SqlBasicRepo<Provider>>();
        services.AddSingleton<IDbExtendedRepo<EntityType, ExtEntityType>, SqlExtendedRepo<EntityType, ExtEntityType>>();
        services.AddSingleton<IDbExtendedRepo<EntityVariant, ExtEntityVariant>, SqlExtendedRepo<EntityVariant, ExtEntityVariant>>();
        services.AddSingleton<IDbExtendedRepo<Package, ExtPackage>, SqlExtendedRepo<Package, ExtPackage>>();
        services.AddSingleton<IDbExtendedRepo<Role, ExtRole>, SqlExtendedRepo<Role, ExtRole>>();
        services.AddSingleton<IDbExtendedRepo<RolePackage, ExtRolePackage>, SqlExtendedRepo<RolePackage, ExtRolePackage>>();
        services.AddSingleton<IDbBasicRepo<TagGroup>, SqlBasicRepo<TagGroup>>();
        services.AddSingleton<IDbCrossRepo<Package, PackageTag, Tag>, SqlCrossRepo<Package, PackageTag, Tag>>();
        services.AddSingleton<IDbExtendedRepo<Tag, ExtTag>, SqlExtendedRepo<Tag, ExtTag>>();
        services.AddSingleton<IDbExtendedRepo<Entity, ExtEntity>, SqlExtendedRepo<Entity, ExtEntity>>();
        services.AddSingleton<IDbExtendedRepo<RoleAssignment, ExtRoleAssignment>, SqlExtendedRepo<RoleAssignment, ExtRoleAssignment>>();
        services.AddSingleton<IDbCrossRepo<EntityVariant, EntityVariantRole, Role>, SqlCrossRepo<EntityVariant, EntityVariantRole, Role>>();
        services.AddSingleton<IDbExtendedRepo<RoleMap, ExtRoleMap>, SqlExtendedRepo<RoleMap, ExtRoleMap>>();
    }
}

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