using Altinn.Authorization.AccessPackages.CLI;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.DbAccess.Migrate.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Altinn.Authorization.AccessPackages.Repo.Extensions;
using Altinn.Authorization.Importers.BRREG;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;



HostApplicationBuilder builder = Host.CreateApplicationBuilder();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Services.AddLogging();

builder.Services.Configure<DbObjDefConfig>(builder.Configuration.GetRequiredSection("DbObjDefConfig"));
builder.Services.AddSingleton<DatabaseDefinitions>();

builder.Services.Configure<DbMigrationConfig>(builder.Configuration.GetSection("DbMigration"));
builder.Services.AddDbAccessMigrations();

builder.Services.Configure<JsonIngestConfig>(builder.Configuration.GetSection("JsonIngest"));
builder.Services.AddDbAccessIngests();

builder.Services.AddDbAccessData();

builder.Services.AddSingleton<Ingestor>();
builder.Services.AddSingleton<Importer>();

var host = builder.Build();

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
              .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("AccessPackages", serviceInstanceId: "cli"))
              .AddSource("Altinn.Authorization.AccessPackages.Repo")
              .AddOtlpExporter()
              .Build();

using var tracerProvider2 = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DbAccess", serviceInstanceId: "cli"))
                .AddSource("Altinn.Authorization.DbAccess")
                .AddOtlpExporter()
                .Build();

var definitions = host.Services.GetRequiredService<DatabaseDefinitions>();
definitions.SetDatabaseDefinitions();

await host.Services.UseDbAccessMigrations();
await host.Services.UseDbAccessIngests();

var ingestor = host.Services.GetRequiredService<Ingestor>();
//await ingestor.IngestAll();

//var importer = host.Services.GetRequiredService<Importer>();
/*
await importer.ImportUnit();
await importer.ImportSubUnit();
await importer.ImportRoles();
importer.WriteChangeRefsToConsole();
*/

/* Testing stuff */

using var a = Telemetry.Source.StartActivity();
a?.AddEvent(new System.Diagnostics.ActivityEvent("dfdfdf"));

var providerService = host.Services.GetRequiredService<IProviderService>();
var res = await providerService.Get();
foreach (var item in res)
{
    Console.WriteLine(item.Name);
}


/*
// Test Provider
var providerService = host.Services.GetRequiredService<IProviderService>();
var providerResult = await providerService.Repo.Get(requestOption);
foreach (var item in providerResult)
{
    Console.WriteLine($"{item.Id}:{item.Name}");
}

// Test Variant
var variantService = host.Services.GetRequiredService<IEntityVariantService>();
var variantResult = await variantService.Repo.Get(requestOption);
foreach (var item in variantResult)
{
    Console.WriteLine($"{item.Id}:{item.Name}");
}

// Test Package
var packageService = host.Services.GetRequiredService<IPackageService>();
var packageResult = await packageService.Repo.Get(requestOption);
foreach (var item in packageResult)
{
    Console.WriteLine($"{item.Id}:{item.Name}");
}
*/