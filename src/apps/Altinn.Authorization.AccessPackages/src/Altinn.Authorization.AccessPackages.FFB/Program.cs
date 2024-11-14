using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.DbAccess.Migrate.Models;
using Altinn.Authorization.AccessPackages.Repo.Extensions;
using Altinn.Authorization.FFB.Components;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using OpenTelemetry.Resources;
using OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Services.Configure<DbObjDefConfig>(builder.Configuration.GetRequiredSection("DbObjDefConfig"));
builder.Services.AddSingleton<DatabaseDefinitions>();

//builder.Services.Configure<DbMigrationConfig>(builder.Configuration.GetRequiredSection("DbMigration"));
//builder.Services.AddDbAccessMigrations();

//builder.Services.Configure<JsonIngestConfig>(builder.Configuration.GetRequiredSection("JsonIngest"));
//builder.Services.AddDbAccessIngests();

builder.Services.AddDbAccessData();

builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
              .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("AccessPackages", serviceInstanceId: "web"))
              .AddSource("Altinn.Authorization.AccessPackages.Repo")
              .AddOtlpExporter()
              .Build();

using var tracerProvider2 = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DbAccess", serviceInstanceId: "web"))
                .AddSource("Altinn.Authorization.DbAccess")
                .AddOtlpExporter()
                .Build();

using var tracerProvider3 = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("FFB", serviceInstanceId: "web"))
                .AddSource("Altinn.Authorization.FFB")
                .AddOtlpExporter()
                .Build();

var dbDef = app.Services.GetRequiredService<DatabaseDefinitions>();
dbDef.SetDatabaseDefinitions();

//// await app.Services.UseDbAccessMigrations();
//// await app.Services.UseDbAccessIngests();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
