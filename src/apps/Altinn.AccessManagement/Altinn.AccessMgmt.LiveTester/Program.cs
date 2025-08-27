using Altinn.AccessMgmt.Core.Data;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options
                .UseNpgsql("Database=accessmgmt_ef_04;Host=localhost;Username=platform_authorization_admin;Password=Password;Include Error Detail=true")
                .ReplaceService<IMigrationsSqlGenerator, CustomMigrationsSqlGenerator>()
                .EnableSensitiveDataLogging(); // valgfritt ved behov
            // .AddInterceptors(sp.GetRequiredService<ReadOnlyInterceptor>(), sp.GetRequiredService<AuditConnectionInterceptor>()); // hvis du har disse
        });

        builder.Services.AddScoped<StaticDataIngest>();
        builder.Services.AddScoped<ITranslationService, TranslationService>();
        //builder.Services.AddScoped<IIngestService, IngestService>();
        //builder.Services.AddScoped<AuditValues>();

        var host = builder.Build();

        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var ingest = scope.ServiceProvider.GetRequiredService<StaticDataIngest>();
            var translate = scope.ServiceProvider.GetRequiredService<ITranslationService>();

            //// db.Database.Migrate();

            //// ingest.IngestAll().Wait();

            var p = db.Providers.FirstOrDefault();
            Console.WriteLine(p is null ? "Ingen provider funnet" : "Provider found: " + p.Name);

            var translatedResult = new List<(Guid id, string lang, string value)>();

            var entityTypes = db.EntityTypes.ToList();
            foreach (var entityType in entityTypes)
            {
                translatedResult.Add((entityType.Id, "nob", entityType.Name));
            }

            entityTypes.ForEach(t => translate.Translate(t, "eng"));
            foreach (var entityType in entityTypes)
            {
                translatedResult.Add((entityType.Id, "eng", entityType.Name));
            }

            foreach (var entityType in translatedResult.Where(t => t.lang == "nob"))
            {
                Console.WriteLine(entityType.value + " => " + translatedResult.FirstOrDefault(t => t.id == entityType.id && t.lang == "eng").value);
            }

            var rel = db.Relations.Include(t => t.From).Include(t => t.To).Include(t => t.Role).FirstOrDefault();
            Console.WriteLine(rel is null ? "Ingen connection funnet" : $"Connection found: {rel.From.Name} - {rel.Role.Name} - {rel.To.Name} ");
        }
    }
}
