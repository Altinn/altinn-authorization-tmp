using AccessMgmt.WebDemo.Components;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Data;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Services.AddScoped<IAuditContextAccessor, AuditContextAccessor>();
builder.Services.AddScoped<ITranslationService, TranslationService>();
builder.Services.AddScoped<StaticDataIngest>();

builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IEntityService, EntityService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IResourceService, ResourceService>();

var connString = "Database=accessmgmt_ef_02;Host=localhost;Username=platform_authorization_admin;Password=Password;Include Error Detail=true";

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseNpgsql(connString);
    options.ReplaceService<IMigrationsSqlGenerator, CustomMigrationsSqlGenerator>();
    options.EnableSensitiveDataLogging();
    options.LogTo(Console.WriteLine);
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();
using var scope = app.Services.CreateScope();
var seed = scope.ServiceProvider.GetRequiredService<StaticDataIngest>();
seed.IngestAll().Wait();
//seed.IngestRequestStatus().Wait();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();
