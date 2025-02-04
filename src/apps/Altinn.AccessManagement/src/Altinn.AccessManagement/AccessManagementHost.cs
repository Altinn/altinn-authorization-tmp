using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Extensions;
using Altinn.AccessManagement.Core.Services.Implementation;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Health;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.AccessManagement.Integration.Extensions;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.AccessManagement.Persistence.Extensions;
using Altinn.AccessMgmt.AccessPackages.Repo;
using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;
using Altinn.AccessMgmt.AccessPackages.Repo.Extensions;
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
using Altinn.Authorization.AccessManagement;
using Altinn.Authorization.Host;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Register.Extensions;
using Altinn.Authorization.ServiceDefaults;
using Altinn.Common.AccessToken;
using Altinn.Common.AccessToken.Configuration;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.Authentication.Configuration;
using Altinn.Common.Authentication.Models;
using Altinn.Common.PEP.Authorization;
using Altinn.Common.PEP.Clients;
using Altinn.Common.PEP.Implementation;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Swashbuckle.AspNetCore.Filters;

namespace Altinn.AccessManagement;

/// <summary>
/// Configures the register host.
/// </summary>
internal static class AccessManagementHost
{
    /// <summary>
    /// Configures the register host.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public static WebApplication Create(string[] args)
    {
        var builder = AltinnHost.CreateWebApplicationBuilder("access-management", args);

        builder.AddDatabaseDefinitions();
        builder.AddDbAccessData();
        builder.AddDbAccessMigrations();
        builder.AddJsonIngests();


        builder.Services.AddAutoMapper(typeof(Program));
        builder.Services.AddControllers();
        builder.Services.AddFeatureManagement();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks()
            .AddCheck<HealthCheck>("authorization_admin_health_check");

        builder.ConfigureAppsettings();
        builder.ConfigurePostgreSqlConfiguration();
        builder.ConfigureAltinnPackages();
        builder.ConfigureInternals();
        builder.ConfigureOpenAPI();
        builder.ConfigureAuthorization();

        builder.ConfigureHostedServices();
        return builder.Build();
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

    public static IHostApplicationBuilder AddDatabaseDefinitions(this WebApplicationBuilder builder, Action<DbObjDefConfig>? configureOptions = null)
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
    /// Adds DbAccess Migrations
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <param name="configureOptions">DbMigrationConfig</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddDbAccessMigrations(this WebApplicationBuilder builder, Action<DbMigrationConfig>? configureOptions = null)
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
    /// Adds DbAccess Ingests
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <param name="configureOptions">JsonIngestConfig</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddJsonIngests(this WebApplicationBuilder builder, Action<JsonIngestConfig>? configureOptions = null)
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
    /// Adds DbAccess Data
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <param name="configureOptions">DbAccessDataConfig</param>
    /// <param name="telemetryOptions">TelemetryConfig</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddDbAccessData(this WebApplicationBuilder builder, Action<DbAccessDataConfig>? configureOptions = null)
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

    private static WebApplicationBuilder ConfigureHostedServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<RegisterHostedService>();
        builder.AddAppSettingDefaults();
        builder.AddAltinnLease(cgf =>
        {
            //// cgf.Type = AltinnLeaseType.InMemory;
            cgf.Type = AltinnLeaseType.AzureStorageAccount;
            cgf.StorageAccount.Endpoint = new Uri("https://standreastest.blob.core.windows.net/");
        });

        builder.AddAltinnRegister(opts =>
        {
            opts.Endpoint = new Uri("http://localhost:5020");
        });

        return builder;
    }

    private static WebApplicationBuilder ConfigureAltinnPackages(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransient<IPublicSigningKeyProvider, PublicSigningKeyProvider>();
        builder.Services.AddSingleton<IPDP, PDPAppSI>();
        builder.Services.AddHttpClient<AuthorizationApiClient>();
        return builder;
    }

    private static void ConfigureInternals(this WebApplicationBuilder builder)
    {
        builder.AddAccessManagementCore();
        builder.AddAccessManagementIntegration();
        builder.AddAccessManagementPersistence();
    }

    private static void ConfigureOpenAPI(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            options.OperationFilter<SecurityRequirementsOperationFilter>();

            var originalIdSelector = options.SchemaGeneratorOptions.SchemaIdSelector;
            options.SchemaGeneratorOptions.SchemaIdSelector = (Type t) =>
            {
                if (!t.IsNested)
                {
                    return originalIdSelector(t);
                }

                var chain = new List<string>();
                do
                {
                    chain.Add(originalIdSelector(t));
                    t = t.DeclaringType;
                }
                while (t != null);
                chain.Reverse();
                return string.Join(".", chain);
            };
        });

        builder.Services.AddUrnSwaggerSupport();
    }

    private static void ConfigureAppsettings(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;

        builder.Services.Configure<GeneralSettings>(config.GetSection("GeneralSettings"));
        builder.Services.Configure<PlatformSettings>(config.GetSection("PlatformSettings"));
        builder.Services.Configure<Altinn.Common.PEP.Configuration.PlatformSettings>(config.GetSection("PlatformSettings"));
        builder.Services.Configure<CacheConfig>(config.GetSection("CacheConfig"));
        builder.Services.Configure<PostgreSQLSettings>(config.GetSection("PostgreSQLSettings"));
        builder.Services.Configure<AzureStorageConfiguration>(config.GetSection("AzureStorageConfiguration"));
        builder.Services.Configure<SblBridgeSettings>(config.GetSection("SblBridgeSettings"));
        builder.Services.Configure<KeyVaultSettings>(config.GetSection("kvSetting"));
        builder.Services.Configure<OidcProviderSettings>(config.GetSection("OidcProviders"));
        builder.Services.Configure<UserProfileLookupSettings>(config.GetSection("UserProfileLookupSettings"));
        builder.Services.Configure<AppsInstanceDelegationSettings>(config.GetSection("AppsInstanceDelegationSettings"));
    }

    private static void ConfigureAuthorization(this WebApplicationBuilder builder)
    {
        PlatformSettings platformSettings = builder.Configuration.GetSection("PlatformSettings").Get<PlatformSettings>();
        OidcProviderSettings oidcProviders = builder.Configuration.GetSection("OidcProviders").Get<OidcProviderSettings>();

        if (oidcProviders.TryGetValue("altinn", out OidcProvider altinnOidcProvder))
        {
            builder.Services.AddAuthentication(JwtCookieDefaults.AuthenticationScheme)
            .AddJwtCookie(JwtCookieDefaults.AuthenticationScheme, options =>
            {
                options.JwtCookieName = platformSettings.JwtCookieName;
                options.MetadataAddress = altinnOidcProvder.Issuer;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                if (builder.Environment.IsDevelopment())
                {
                    options.RequireHttpsMetadata = false;
                }
            });
        }

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(AuthzConstants.PLATFORM_ACCESS_AUTHORIZATION, policy => policy.Requirements.Add(new AccessTokenRequirement()))
            .AddPolicy(AuthzConstants.ALTINNII_AUTHORIZATION, policy => policy.Requirements.Add(new ClaimAccessRequirement("urn:altinn:app", "sbl.authorization")))
            .AddPolicy(AuthzConstants.INTERNAL_AUTHORIZATION, policy => policy.Requirements.Add(new ClaimAccessRequirement("urn:altinn:app", "internal.authorization")))
            .AddPolicy(AuthzConstants.POLICY_MASKINPORTEN_DELEGATION_READ, policy => policy.Requirements.Add(new ResourceAccessRequirement("read", "altinn_maskinporten_scope_delegation")))
            .AddPolicy(AuthzConstants.POLICY_MASKINPORTEN_DELEGATION_WRITE, policy => policy.Requirements.Add(new ResourceAccessRequirement("write", "altinn_maskinporten_scope_delegation")))
            .AddPolicy(AuthzConstants.POLICY_MASKINPORTEN_DELEGATIONS_PROXY, policy => policy.Requirements.Add(new ScopeAccessRequirement(["altinn:maskinporten/delegations", "altinn:maskinporten/delegations.admin"])))
            .AddPolicy(AuthzConstants.POLICY_ACCESS_MANAGEMENT_READ, policy => policy.Requirements.Add(new ResourceAccessRequirement("read", "altinn_access_management")))
            .AddPolicy(AuthzConstants.POLICY_ACCESS_MANAGEMENT_WRITE, policy => policy.Requirements.Add(new ResourceAccessRequirement("write", "altinn_access_management")))
            .AddPolicy(AuthzConstants.POLICY_RESOURCEOWNER_AUTHORIZEDPARTIES, policy => policy.Requirements.Add(new ScopeAccessRequirement([AuthzConstants.SCOPE_AUTHORIZEDPARTIES_RESOURCEOWNER, AuthzConstants.SCOPE_AUTHORIZEDPARTIES_ADMIN])));

        builder.Services.AddScoped<IAuthorizationHandler, AccessTokenHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, ClaimAccessHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, ResourceAccessHandler>();
        builder.Services.AddScoped<IAuthorizationHandler, ScopeAccessHandler>();
    }

    private static void ConfigurePostgreSqlConfiguration(this WebApplicationBuilder builder)
    {
        var runMigrations = builder.Configuration.GetValue<bool>("PostgreSQLSettings:EnableDBConnection");
        var adminConnectionStringFmt = builder.Configuration.GetValue<string>("PostgreSQLSettings:AdminConnectionString");
        var adminConnectionStringPwd = builder.Configuration.GetValue<string>("PostgreSQLSettings:AuthorizationDbAdminPwd");
        var connectionStringFmt = builder.Configuration.GetValue<string>("PostgreSQLSettings:ConnectionString");
        var connectionStringPwd = builder.Configuration.GetValue<string>("PostgreSQLSettings:AuthorizationDbPwd");

        var adminConnectionString = new NpgsqlConnectionStringBuilder(string.Format(adminConnectionStringFmt, adminConnectionStringPwd));
        var connectionString = new NpgsqlConnectionStringBuilder(string.Format(connectionStringFmt, connectionStringPwd))
        {
            MaxAutoPrepare = 50,
            AutoPrepareMinUsages = 2,
        };

        var serviceDescriptor = builder.Services.GetAltinnServiceDescriptor();
        var existing = builder.Configuration.GetValue<string>($"ConnectionStrings:{serviceDescriptor.Name}_db");

        if (!string.IsNullOrEmpty(existing))
        {
            return;
        }

        builder.Configuration.AddInMemoryCollection([
            KeyValuePair.Create($"ConnectionStrings:{serviceDescriptor.Name}_db", connectionString.ToString()),
                KeyValuePair.Create($"ConnectionStrings:{serviceDescriptor.Name}_db_migrate", adminConnectionString.ToString()),
                KeyValuePair.Create($"Altinn:Npgsql:{serviceDescriptor.Name}:Migrate:Enabled", runMigrations ? "true" : "false"),
            ]);
    }
}
