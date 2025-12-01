using Altinn.AccessManagement;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Contracts;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessMgmt.Core.Extensions;
using Altinn.AccessMgmt.Core.HostedServices.Services;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Host.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static Altinn.AccessMgmt.PersistenceEF.Extensions.ServiceCollectionExtensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        var configManager = new ConfigurationManager();
        configManager.AddConfiguration(config);

        services.Configure<AccessManagementDatabaseOptions>(
            context.Configuration.GetSection("PostgreSQLSettings")
        );

        var connectionStrings = GetConnectionStrings(configManager);
        services.AddAccessManagementDatabase(options =>
        {
            var appsettings = new AccessManagementAppsettings(config);
            options.AppConnectionString = connectionStrings.AppSource;
            options.MigrationConnectionString = connectionStrings.MigrationSource;
            options.ReadOnlyConnectionStrings = connectionStrings.ReadOnlySources;
            options.Source = appsettings.RunInitOnly ? SourceType.Migration : SourceType.App;
        });


        services.AddScoped<IIngestService, IngestService>();
        services.AddScoped<IConnectionService, ConnectionService>();
        services.AddScoped<IPartyService, PartyService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<IDelegationService, DelegationService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<IEntityService, EntityService>();        
        services.AddScoped<IAuthorizedPartyRepoService, AuthorizedPartyRepoService>();
        services.AddScoped<IAuthorizedPartyRepoServiceEf, AuthorizedPartyRepoServiceEf>();
        services.AddScoped<IAuthorizedPartiesService, AuthorizedPartiesServiceEf>();

        services.AddTransient<IBasicEFTester, Greeter>();
        services.AddHostedService<Worker>();

    })
    .Build();

await host.RunAsync();

static (string AppSource, string MigrationSource, Dictionary<string, string> ReadOnlySources, bool Valid) GetConnectionStrings(ConfigurationManager configuration)
{
    var adminConnectionStringFmt = configuration.GetValue<string>("PostgreSQLSettings:AdminConnectionString");
    var connectionStringFmt = configuration.GetValue<string>("PostgreSQLSettings:ConnectionString");

    if (string.IsNullOrEmpty(connectionStringFmt) || string.IsNullOrEmpty(adminConnectionStringFmt))
    {
        return (connectionStringFmt, adminConnectionStringFmt, new(), false);
    }

    var adminConnectionStringPwd = configuration.GetValue<string>("PostgreSQLSettings:AuthorizationDbAdminPwd");
    var connectionStringPwd = configuration.GetValue<string>("PostgreSQLSettings:AuthorizationDbPwd");
    var readonlyPwd = configuration.GetValue<string>("PostgreSQLSettings:AuthorizationDbReadOnlyPwd") ?? connectionStringPwd;

    var appConn = string.Format(connectionStringFmt, connectionStringPwd);
    var adminConn = string.Format(adminConnectionStringFmt, adminConnectionStringPwd);

    var list = new Dictionary<string, string>();
    var section = configuration.GetSection("PostgreSQLSettings:ReadOnlyConnectionStrings");

    if (section.Exists())
    {
        foreach (var child in section.GetChildren())
        {
            var fmt = child.Value;
            if (!string.IsNullOrWhiteSpace(fmt))
            {
                var readonlyConn = string.Format(fmt, readonlyPwd);
                list.Add(child.Key, readonlyConn);
            }
        }
    }

    list.Add("Primary", appConn);

    return (appConn, adminConn, list, true);
}


/// <summary>
/// Example interface for DI.
/// </summary>
public interface IBasicEFTester
{
    void Go();
}

/// <summary>
/// Simple implementation.
/// </summary>
public class Greeter(ConnectionQuery connectionQuery) : IBasicEFTester
{
    public void Go()
    {
        var query = connectionQuery.GenerateDebugQuery(new ConnectionQueryFilter() { ToIds = [Guid.Empty] }, ConnectionQueryDirection.FromOthers, true);
        Console.WriteLine(query);
    }
}

/// <summary>
/// Background worker that runs once.
/// </summary>
public class Worker : BackgroundService
{
    private readonly IBasicEFTester _greeter;
    private readonly ILogger<Worker> _logger;

    public Worker(IBasicEFTester greeter, ILogger<Worker> logger)
    {
        _greeter = greeter;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started");
        _greeter.Go();
        _logger.LogInformation("Worker finished");
        return Task.CompletedTask;
    }
}
