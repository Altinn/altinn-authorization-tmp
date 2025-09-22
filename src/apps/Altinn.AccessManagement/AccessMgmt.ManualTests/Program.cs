using Altinn.AccessMgmt.Core.Data;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Services.AddScoped<IAuditContextAccessor, AuditContextAccessor>();
builder.Services.AddScoped<ITranslationService, TranslationService>();
builder.Services.AddScoped<StaticDataIngest>();

var connString = "Database=accessmgmt_ef_05;Host=localhost;Username=platform_authorization_admin;Password=Password;Include Error Detail=true";

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseNpgsql(connString);
    options.ReplaceService<IMigrationsSqlGenerator, CustomMigrationsSqlGenerator>();
    options.EnableSensitiveDataLogging();
    options.LogTo(Console.WriteLine);
});

// 3) Add a simple runner
builder.Services.AddHostedService<DemoRunner>();

var app = builder.Build();
await app.RunAsync();

public sealed class DemoRunner(IServiceProvider services) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var audit = scope.ServiceProvider.GetRequiredService<IAuditContextAccessor>();
        var seed = scope.ServiceProvider.GetRequiredService<StaticDataIngest>();

        audit.Current = new AuditValues(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString());

        //await seed.IngestAll();
        await seed.IngestProviderType();

        var providerType = await db.ProviderTypes.FirstAsync();
        var provider = await db.Providers.FirstOrDefaultAsync(p => p.Code == "T");

        if (provider == null)
        {
            provider = new Provider() { Id = Guid.NewGuid(), Code = "T", Name = "Test-", RefId = "TTT", TypeId = providerType.Id };
            db.Providers.Add(provider);
        }
        else
        {
            provider.Name = provider.Name + "A";
        }

        if (provider.Name.Length > 8) 
        { 
            // Kjør 5 ganger så skal det slettes
            db.Providers.Remove(provider);
            await db.SaveChangesAsync(new AuditValues(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid().ToString()), stoppingToken);
        }

        var affected = await db.SaveChangesAsync(stoppingToken);
        Console.WriteLine($"Saved {affected} changes.");
    }
}
