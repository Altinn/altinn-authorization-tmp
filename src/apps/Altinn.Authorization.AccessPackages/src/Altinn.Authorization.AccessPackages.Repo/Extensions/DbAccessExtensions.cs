using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services.Mssql;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services.Postgres;
using Altinn.Authorization.AccessPackages.DbAccess.Migrate.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Migrate.Models;
using Altinn.Authorization.AccessPackages.DbAccess.Migrate.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Altinn.Authorization.AccessPackages.Repo.Data.Services;
using Altinn.Authorization.AccessPackages.Repo.Ingest;
using Altinn.Authorization.AccessPackages.Repo.Migrate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altinn.Authorization.AccessPackages.Repo.Extensions;

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
        var ss = services.GetRequiredService<JsonIngestMeters>();
        ss.Test.Record(6);
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
        builder.Services.AddSingleton<IAreaService, AreaDataService>();
        builder.Services.AddSingleton<IAreaGroupService, AreaGroupDataService>();
        builder.Services.AddSingleton<IEntityTypeService, EntityTypeDataService>();
        builder.Services.AddSingleton<IEntityVariantService, EntityVariantDataService>();
        builder.Services.AddSingleton<IPackageService, PackageDataService>();
        builder.Services.AddSingleton<IProviderService, ProviderDataService>();
        builder.Services.AddSingleton<IRoleService, RoleDataService>();
        builder.Services.AddSingleton<IRolePackageService, RolePackageDataService>();
        builder.Services.AddSingleton<ITagGroupService, TagGroupDataService>();
        builder.Services.AddSingleton<IPackageTagService, PackageTagDataService>();
        builder.Services.AddSingleton<ITagService, TagDataService>();
        builder.Services.AddSingleton<IEntityService, EntityDataService>();
        builder.Services.AddSingleton<IRoleAssignmentService, RoleAssignmentDataService>();
        builder.Services.AddSingleton<IEntityVariantRoleService, EntityVariantRoleDataService>();
        builder.Services.AddSingleton<IRoleMapService, RoleMapDataService>();
        #endregion

        return builder;
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
