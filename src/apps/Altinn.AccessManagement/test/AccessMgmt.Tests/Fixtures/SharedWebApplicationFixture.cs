using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Tests.Contexts;
using Altinn.AccessManagement.Tests.Mocks;
using Altinn.AccessManagement.Tests.Scenarios;
using Altinn.Common.AccessToken.Services;
using Altinn.Common.PEP.Interfaces;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Tests.Fixtures;

/// <summary>
/// Shared test server fixture that reuses a single Postgres database, migrated once, across multiple test classes.
/// Does not modify existing WebApplicationFixture; use this in new/ported tests needing faster startup.
/// Uses PostgresServer (from PostgresFixture.cs) the same way: StartUsing/StopUsing and NewDatabase for initial creation.
/// </summary>
public class SharedWebApplicationFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly object DbLock = new();
    private static PostgresDatabase _sharedDatabase;
    private HttpClient _client; // shared client

    /// <summary>
    /// Exposes the shared database (e.g. for direct seeding if required).
    /// </summary>
    public PostgresDatabase Database => _sharedDatabase ?? throw new InvalidOperationException("Database not initialized yet");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Ensure database created only once
        if (_sharedDatabase == null)
        {
            lock (DbLock)
            {
                _sharedDatabase ??= PostgresServer.NewDatabase();
            }
        }

        var db = _sharedDatabase;

        var appsettings = new ConfigurationBuilder()
           .AddJsonFile("appsettings.test.json")
           .AddInMemoryCollection(new Dictionary<string, string>
           {
               ["PostgreSQLSettings:AdminConnectionString"] = db.Admin.ToString(),
               ["PostgreSQLSettings:ConnectionString"] = db.User.ToString(),
               ["PostgreSQLSettings:EnableDBConnection"] = "true",
               ["Logging:LogLevel:*"] = "Error",
               ["FeatureManagement:AccessManagement.MigrationDb"] = "true",
               ["FeatureManagement:AccessManagement.MigrationDbWithBasicData"] = "true",
           });

        builder.UseConfiguration(appsettings.Build());

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<RepositoryContainer>();
        });

        builder.ConfigureTestServices(services =>
        {
            // Register a default MockContext so mock clients depending on it can resolve even when
            // tests do not use scenario-based host configuration.
            services.AddSingleton<MockContext>(_ => new MockContext());
            AddMockClients(services);
        });
    }

    /// <summary>
    /// Returns a shared HttpClient instance. Created lazily after host build.
    /// </summary>
    public HttpClient GetClient()
    {
        _client ??= CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        if (!_client.DefaultRequestHeaders.Accept.Any(h => h.MediaType == "application/json"))
        {
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }
        return _client;
    }

    /// <summary>
    /// Creates an ad-hoc host with additional scenarios but still reusing the same underlying database.
    /// (Used for AcceptanceCriteriaComposer style tests that need scenario seeding.)
    /// </summary>
    public SharedHost ConfigureHostBuilderWithScenarios(params Scenario[] scenarios)
    {
        var mock = new MockContext();
        foreach (var scenario in scenarios)
        {
            scenario(mock);
        }

        mock.Parties = mock.Parties.DistinctBy(p => p.PartyId).ToList();
        mock.UserProfiles = mock.UserProfiles.DistinctBy(u => u.UserId).ToList();
        mock.Resources = mock.Resources.DistinctBy(r => r.Identifier).ToList();

        // Reuse same DB via static field. WithWebHostBuilder only alters DI/mocks.
        var factory = WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                // Replace existing MockContext with scenario-specific one
                services.AddSingleton(mock);
            });
        });

        var client = factory.CreateClient();
        foreach (var header in mock.HttpHeaders)
        {
            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        return new SharedHost(factory, client, mock);
    }

    private static void AddMockClients(IServiceCollection services)
    {
        services.AddSingleton<IPartiesClient, Contexts.PartiesClientMock>();
        services.AddSingleton<IProfileClient, Contexts.ProfileClientMock>();
        services.AddSingleton<IResourceRegistryClient, ResourceRegistryMock>();

        services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
        services.AddSingleton<IPolicyFactory, PolicyFactoryMock>();
        services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
        services.AddSingleton<IPublicSigningKeyProvider, SigningKeyResolverMock>();

        services.AddSingleton<IAltinnRolesClient, AltinnRolesClientMock>();
        services.AddSingleton<IPDP, PdpPermitMock>();
        services.AddSingleton<IAltinn2RightsClient, Altinn2RightsClientMock>();
        services.AddSingleton<IDelegationChangeEventQueue>(new DelegationChangeEventQueueMock());
    }

    public Task InitializeAsync()
    {
        // Matches pattern from WebApplicationFixture / PostgresFixture
        PostgresServer.StartUsing(this);
        return Task.CompletedTask;
    }

    public new Task DisposeAsync()
    {
        PostgresServer.StopUsing(this);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Container for API factory, client and mock context (unique to SharedWebApplicationFixture to avoid name clash).
/// </summary>
public class SharedHost(WebApplicationFactory<Program> api, HttpClient client, MockContext mock)
{
    public MockContext Mock { get; set; } = mock;

    public WebApplicationFactory<Program> Api { get; } = api;

    public HttpClient Client { get; } = client;

    public RepositoryContainer Repository => Api.Services.GetRequiredService<RepositoryContainer>();
}
