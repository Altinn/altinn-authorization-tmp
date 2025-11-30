using Altinn.ApiClients.Maskinporten.Extensions;
using Altinn.ApiClients.Maskinporten.Services;
using Altinn.Authorization.ServiceDefaults;
using Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql;
using Altinn.Authorization.Services.Implementation;
using Altinn.Common.AccessToken;
using Altinn.Common.AccessToken.Configuration;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Common.PEP.Authorization;
using Altinn.Platform.Authorization.Clients;
using Altinn.Platform.Authorization.Clients.Interfaces;
using Altinn.Platform.Authorization.Configuration;
using Altinn.Platform.Authorization.Constants;
using Altinn.Platform.Authorization.Extensions;
using Altinn.Platform.Authorization.Health;
using Altinn.Platform.Authorization.ModelBinding;
using Altinn.Platform.Authorization.Models;
using Altinn.Platform.Authorization.Repositories;
using Altinn.Platform.Authorization.Repositories.Interface;
using Altinn.Platform.Authorization.Services;
using Altinn.Platform.Authorization.Services.Implementation;
using Altinn.Platform.Authorization.Services.Interface;
using Altinn.Platform.Authorization.Services.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Altinn.Authorization;

internal static partial class AuthorizationHost
{
    /// <summary>
    /// Configures the register host.
    /// </summary>
    /// <param name="args">The command line arguments.</param>
    public static WebApplication Create(string[] args)
    {
        var builder = AltinnHost.CreateWebApplicationBuilder("authorization", args, opts => opts.ConfigureEnabledServices(services => services.DisableApplicationInsights()));
        var services = builder.Services;
        var config = builder.Configuration;
        var descriptor = builder.GetAltinnServiceDescriptor();

        if (!builder.Environment.IsDevelopment())
        {
            if (builder.Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey") is var key && !string.IsNullOrEmpty(key))
            {
                builder.Services.AddOpenTelemetry()
                    .UseAzureMonitor(m =>
                    {
                        m.ConnectionString = string.Format("InstrumentationKey={0}", key);
                    });
            }
        }

        services.AddMemoryCache();

        services.AddAutoMapper(typeof(Program));
        services.AddControllers().AddXmlSerializerFormatters();
        services.AddHealthChecks().AddCheck<HealthCheck>("authorization_health_check");
        services.AddSingleton(config);
        services.AddSingleton<IParties, PartiesWrapper>();
        services.AddSingleton<IProfile, ProfileWrapper>();
        services.AddSingleton<IRoles, RolesWrapper>();
        services.AddSingleton<IOedRoleAssignmentWrapper, OedRoleAssignmentWrapper>();
        services.AddSingleton<IContextHandler, ContextHandler>();
        services.AddSingleton<IDelegationContextHandler, DelegationContextHandler>();
        services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPoint>();
        services.AddSingleton<IPolicyInformationPoint, PolicyInformationPoint>();
        services.AddSingleton<IPolicyAdministrationPoint, PolicyAdministrationPoint>();
        services.AddSingleton<IPolicyRepository, PolicyRepository>();
        services.AddSingleton<IResourceRegistry, ResourceRegistryWrapper>();
        services.AddSingleton<IInstanceMetadataRepository, InstanceMetadataRepository>();
        services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepository>();
        services.AddSingleton<IDelegationChangeEventQueue, DelegationChangeEventQueue>();
        services.AddSingleton<IEventMapperService, EventMapperService>();
        services.AddSingleton<IAccessManagementWrapper, AccessManagementWrapper>();
        services.AddSingleton<IAccessListAuthorization, AccessListAuthorization>();
        services.AddSingleton<IPublicSigningKeyProvider, PublicSigningKeyProvider>();

        services.Configure<GeneralSettings>(config.GetSection("GeneralSettings"));
        services.Configure<AzureStorageConfiguration>(config.GetSection("AzureStorageConfiguration"));
        services.Configure<AzureCosmosSettings>(config.GetSection("AzureCosmosSettings"));
        services.Configure<PlatformSettings>(config.GetSection("PlatformSettings"));
        services.Configure<KeyVaultSettings>(config.GetSection("kvSetting"));
        OedAuthzMaskinportenClientSettings oedAuthzMaskinportenClientSettings = config.GetSection("OedAuthzMaskinportenClientSettings").Get<OedAuthzMaskinportenClientSettings>();
        services.Configure<OedAuthzMaskinportenClientSettings>(config.GetSection("OedAuthzMaskinportenClientSettings"));
        services.AddMaskinportenHttpClient<SettingsJwkClientDefinition, OedAuthzMaskinportenClientDefinition>(oedAuthzMaskinportenClientSettings);
        services.Configure<QueueStorageSettings>(config.GetSection("QueueStorageSettings"));
        services.AddHttpClient<AccessManagementClient>();
        services.AddHttpClient<IRegisterService, RegisterService>();
        services.AddHttpClient<PartyClient>();
        services.AddHttpClient<ProfileClient>();
        services.AddHttpClient<RolesClient>();
        services.AddHttpClient<SBLClient>();
        services.AddHttpClient<ResourceRegistryClient>();
        services.AddHttpClient<OedAuthzClient>();
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddTransient<ISigningCredentialsResolver, SigningCredentialsResolver>();
        services.AddSingleton<IEventsQueueClient, EventsQueueClient>();
        services.AddSingleton<IEventLog, EventLogService>();
        services.TryAddSingleton(TimeProvider.System);
        GeneralSettings generalSettings = config.GetSection("GeneralSettings").Get<GeneralSettings>();
        services.AddAuthentication(JwtCookieDefaults.AuthenticationScheme)
            .AddJwtCookie(JwtCookieDefaults.AuthenticationScheme, options =>
            {
                options.JwtCookieName = generalSettings.RuntimeCookieName;
                options.MetadataAddress = generalSettings.OpenIdWellKnownEndpoint;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };

                if (builder.Environment.IsDevelopment())
                {
                    options.RequireHttpsMetadata = false;
                }
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthzConstants.POLICY_STUDIO_DESIGNER, policy => policy.Requirements.Add(new ClaimAccessRequirement("urn:altinn:app", "studio.designer")))
            .AddPolicy(AuthzConstants.ALTINNII_AUTHORIZATION, policy => policy.Requirements.Add(new ClaimAccessRequirement("urn:altinn:app", "sbl.authorization")))
            .AddPolicy(AuthzConstants.POLICY_PLATFORMISSUER_ACCESSTOKEN, policy => policy.Requirements.Add(new AccessTokenRequirement(AuthzConstants.PLATFORM_ACCESSTOKEN_ISSUER)))
            .AddPolicy(AuthzConstants.DELEGATIONEVENT_FUNCTION_AUTHORIZATION, policy => policy.Requirements.Add(new ClaimAccessRequirement("urn:altinn:app", "platform.authorization")))
            .AddPolicy(AuthzConstants.AUTHORIZESCOPEACCESS, policy => policy.Requirements.Add(new ScopeAccessRequirement([AuthzConstants.AUTHORIZE_SCOPE, AuthzConstants.AUTHORIZE_ADMIN_SCOPE])));

        services.AddTransient<IAuthorizationHandler, ClaimAccessHandler>();
        services.AddTransient<IAuthorizationHandler, ScopeAccessHandler>();
        services.AddSingleton<IAuthorizationHandler, AccessTokenHandler>();

        services.AddPlatformAccessTokenSupport(config, builder.Environment.IsDevelopment());

        services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });

        services.AddMvc(options =>
        {
            // Adding custom model binders
            options.ModelBinderProviders.Insert(0, new XacmlRequestApiModelBinderProvider());
            options.RespectBrowserAcceptHeader = true;
        });

        // Add Swagger support (Swashbuckle)
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Altinn Platform Authorization", Version = "v1" });

            options.AddSecurityDefinition("AuthorizeAPI", new OpenApiSecurityScheme
            {
                Name = "AuthorizeAPI",
                Description = $"Requires one of the following Scopes: [{AuthzConstants.AUTHORIZE_SCOPE}, {AuthzConstants.AUTHORIZE_ADMIN_SCOPE}]",
                Type = SecuritySchemeType.Http,
                In = ParameterLocation.Header,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            options.AddSecurityDefinition("SubscriptionKey", new OpenApiSecurityScheme
            {
                Name = "SubscriptionKey",
                Description = $"Requires a valid product subscription key as header value: \"Ocp-Apim-Subscription-Key\"",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header
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

        services.AddUrnSwaggerSupport();
        services.AddSwaggerAutoXmlDoc();

        services.AddFeatureManagement();

        MapPostgreSqlConfiguration(builder, descriptor);
        var fs = new ManifestEmbeddedFileProvider(typeof(AuthorizationHost).Assembly, "Migration");
        var sqlBuilder = builder.AddAltinnPostgresDataSource(settings =>
            {
                // Apparently, pgsql is no longer in use in authorization, so this is mainly just here for it to compile
                settings.DisableHealthChecks = true;
                settings.DisableMetrics = true;
                settings.DisableTracing = true;
            })
            .MapEnum<DelegationChangeType>("delegation.delegationchangetype");

        if (config.GetValue<bool>("PostgreSQLSettings:EnableDBConnection")) 
        {
            sqlBuilder.AddYuniqlMigrations(typeof(AuthorizationHost), cfg =>
            {
                cfg.WorkspaceFileProvider = fs;
                cfg.Workspace = "/";
            });
        }

        return builder.Build();
    }

    private static void MapPostgreSqlConfiguration(IHostApplicationBuilder builder, AltinnServiceDescriptor serviceDescriptor)
    {
        var runMigrations = builder.Configuration.GetValue<bool>("PostgreSQLSettings:EnableDBConnection");
        var adminConnectionStringFmt = builder.Configuration.GetValue<string>("PostgreSQLSettings:AdminConnectionString");
        var adminConnectionStringPwd = builder.Configuration.GetValue<string>("PostgreSQLSettings:AuthorizationDbAdminPwd", defaultValue: string.Empty);
        var connectionStringFmt = builder.Configuration.GetValue<string>("PostgreSQLSettings:ConnectionString");
        var connectionStringPwd = builder.Configuration.GetValue<string>("PostgreSQLSettings:AuthorizationDbPwd", defaultValue: string.Empty);

        if (adminConnectionStringFmt is not null && connectionStringFmt is not null)
        {
            var adminConnectionString = string.Format(adminConnectionStringFmt, adminConnectionStringPwd);
            var connectionString = string.Format(connectionStringFmt, connectionStringPwd);

            var existing = builder.Configuration.GetValue<string>($"ConnectionStrings:{serviceDescriptor.Name}_db");
            if (!string.IsNullOrEmpty(existing))
            {
                return;
            }

            builder.Configuration.AddInMemoryCollection([
                KeyValuePair.Create($"ConnectionStrings:{serviceDescriptor.Name}_db", connectionString),
                KeyValuePair.Create($"ConnectionStrings:{serviceDescriptor.Name}_db_migrate", adminConnectionString),
                KeyValuePair.Create($"Altinn:Npgsql:{serviceDescriptor.Name}:Migrate:Enabled", runMigrations ? "true" : "false"),
            ]);
        }
    }
}
