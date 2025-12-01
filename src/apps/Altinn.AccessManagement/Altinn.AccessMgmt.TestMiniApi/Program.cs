using Altinn.AccessManagement;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Contracts;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessMgmt.ConsoleTester;
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

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------
// 1. Add configuration (UNCHANGED)
// -----------------------------------------------
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// -----------------------------------------------
// 2. Register services (UNCHANGED)
// -----------------------------------------------
builder.Services.Configure<AccessManagementDatabaseOptions>(
    builder.Configuration.GetSection("PostgreSQLSettings"));

var configManager = new ConfigurationManager();
configManager.AddConfiguration(builder.Configuration);

var connectionStrings = GetConnectionStrings(configManager);

builder.Services.AddAccessManagementDatabase(options =>
{
    var appsettings = new AccessManagementAppsettings(builder.Configuration);
    options.AppConnectionString = connectionStrings.AppSource;
    options.MigrationConnectionString = connectionStrings.MigrationSource;
    options.ReadOnlyConnectionStrings = connectionStrings.ReadOnlySources;
    options.Source = appsettings.RunInitOnly ? SourceType.Migration : SourceType.App;
});

builder.Services.AddScoped<ConnectionQuery>();

// Load-test services
builder.Services.AddScoped<ReadOnlyRoundRobinTester>();
builder.Services.AddScoped<ReadOnlyRoundRobinTester2>();

// -----------------------------------------------
// 3. Build WebApp
// -----------------------------------------------
var app = builder.Build();

app.MapGet("/test/replicas2", async (
    ReadOnlyRoundRobinTester2 tester,
    CancellationToken ct) =>
{
    // This method does ONE query per call
    var result = await tester.Go(ct);
    return Results.Ok(result);
});

app.MapGet("/test/replicas", async (
    ReadOnlyRoundRobinTester tester,
    CancellationToken ct) =>
{
    // This method does ONE query per call
    var result = await tester.Go(ct);
    return Results.Ok(result);
});

app.MapGet("/test/replicas3", async (
    ConnectionQuery tester,
    CancellationToken ct) =>
{
    // This method does ONE query per call
    var result = tester.GenerateDebugQuery(new ConnectionQueryFilter() { ToIds = [Guid.Parse("1ed8a4e3-6d2b-4cf0-9d8e-25d0439c9c57")] }, ConnectionQueryDirection.FromOthers, true);
    return Results.Ok(result);
});

app.MapGet("/test/replicas4", async (
    ConnectionQuery tester,
    CancellationToken ct) =>
{
    // This method does ONE query per call
    var result = await tester.GetConnectionsFromOthersAsync(new ConnectionQueryFilter() { ToIds = [Guid.Parse("1ed8a4e3-6d2b-4cf0-9d8e-25d0439c9c57")] }, true, ct);
    return Results.Ok(result);
});

app.UseHttpsRedirection();
app.Run();


// ---------------------------------------------------------------
// Helper: GetConnectionStrings (UNCHANGED FROM YOUR VERSION)
// ---------------------------------------------------------------
static (string AppSource, string MigrationSource, Dictionary<string, string> ReadOnlySources, bool Valid)
    GetConnectionStrings(ConfigurationManager configuration)
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

    return (appConn, adminConn, list, true);
}
