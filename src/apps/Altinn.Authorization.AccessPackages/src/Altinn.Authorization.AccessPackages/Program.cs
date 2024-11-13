using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.DbAccess.Migrate.Models;
using Altinn.Authorization.AccessPackages.Extensions;
using Altinn.Authorization.AccessPackages.Repo.Extensions;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DbObjDefConfig>(builder.Configuration.GetRequiredSection("DbObjDefConfig"));
builder.Services.AddSingleton<DatabaseDefinitions>();

builder.Services.Configure<DbMigrationConfig>(builder.Configuration.GetSection("DbMigration"));
builder.Services.AddDbAccessMigrations();

builder.Services.Configure<JsonIngestConfig>(builder.Configuration.GetSection("JsonIngest"));
builder.Services.AddDbAccessIngests();

builder.Services.AddDbAccessData();

var app = builder.Build();

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
              .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("AccessPackages", serviceInstanceId: "api"))
              .AddSource("Altinn.Authorization.AccessPackages.Repo")
              .AddOtlpExporter()
              .Build();

using var tracerProvider2 = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DbAccess", serviceInstanceId:"api"))
                .AddSource("Altinn.Authorization.DbAccess")
                .AddOtlpExporter()
                .Build();

var definitions = app.Services.GetRequiredService<DatabaseDefinitions>();
definitions.SetDatabaseDefinitions();

await app.Services.UseDbAccessMigrations();
await app.Services.UseDbAccessIngests();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/icon/{type}/{category}/{name}", (string type, string category, string name) =>
{
    return Results.File(@$"resources/{type}/{category}/{name}.svg", contentType: "image/svg+xml");
}).WithOpenApi().WithTags("Icon").WithSummary("Gets icons");

app.MapDbAccessEndpoints();

app.Run();