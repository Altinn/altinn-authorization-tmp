using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.TestUtils.Factories;
using Altinn.AccessMgmt.TestUtils.Mocks;
using Altinn.AccessMgmt.TestUtils.Utils;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Altinn.AccessMgmt.TestUtils.Fixtures;

public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgresDatabase _database = null!;

    private int _seedonce = 0;

    private List<Action<IConfigurationBuilder>> ConfigurationBuilderActions { get; } = [];

    private List<Action<IServiceCollection>> ConfigureServices { get; } = [];

    public HttpClient BuildConfiguration(params Action<HttpClient>[] configureServices)
    {
        var client = Server.CreateClient();
        foreach (var configure in configureServices)
        {
            configure(client);
        }

        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var appsettings = new ConfigurationBuilder()
           .AddJsonFile("appsettings.default.json", optional: true)
           .AddInMemoryCollection(new Dictionary<string, string>
           {
               ["PostgreSQLSettings:AuthorizationDbAdminPwd"] = _database.Admin.Password,
               ["PostgreSQLSettings:AuthorizationDbPwd"] = _database.User.Password,
               ["PostgreSQLSettings:AdminConnectionString"] = _database.Admin.ToString(),
               ["PostgreSQLSettings:ConnectionString"] = _database.User.ToString(),
               ["Logging:LogLevel:*"] = "Error",
           });

        foreach (var action in ConfigurationBuilderActions)
        {
            action(appsettings);
        }

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IPublicSigningKeyProvider>();
            services.PostConfigure<JwtCookieOptions>(JwtCookieDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = TestTokenGenerator.SigningKey,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                    };
                });

            services.AddSingleton<IPublicSigningKeyProvider, PublicSigningKeyProviderMock>();
            services.AddSingleton<IPDP, PermitPdpMock>();
            foreach (var configure in ConfigureServices)
            {
                configure(services);
            }
        });

        builder.UseConfiguration(appsettings.Build());
    }

    public void ConfiureServices(Action<IServiceCollection> configureServices)
    {
        ConfigureServices.Add(configureServices);
    }

    public void ConfigureAppsettings(Action<IConfigurationBuilder> configure)
    {
        ConfigurationBuilderActions.Add(configure);
    }

    public async Task QueryDb(params Func<AppDbContext, Task>[] configureDb)
    {
        if (Interlocked.Increment(ref _seedonce) == 1)
        {
            var audit = new AuditValues(SystemEntityConstants.StaticDataIngest);
            using var scope = Services.CreateEFScope(audit);
            using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            foreach (var configure in configureDb)
            {
                await configure(db);
            }
        }
    }

    public void EnsureSeedOnce(params Action<AppDbContext>[] configureDb)
    {
        if (Interlocked.Increment(ref _seedonce) == 1)
        {
            var audit = new AuditValues(SystemEntityConstants.StaticDataIngest);
            using var scope = Services.CreateEFScope(audit);
            using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            foreach (var configure in configureDb)
            {
                configure(db);
            }
        }
    }

    public void WithEnabledFeatureFlag(string featureFlag)
    {
        ConfigureAppsettings(appsettings =>
        {
            appsettings.AddInMemoryCollection(new Dictionary<string, string>
            {
                [$"FeatureManagement:{featureFlag}"] = "true",
            });
        });
    }

    public void WithDisabledFeatureFlag(string featureFlag)
    {
        ConfigureAppsettings(appsettings =>
        {
            appsettings.AddInMemoryCollection(new Dictionary<string, string>
            {
                [$"FeatureManagement:{featureFlag}"] = "false",
            });
        });
    }

    public async ValueTask InitializeAsync()
    {
        _database = await EFPostgresFactory.Create();
    }
}
