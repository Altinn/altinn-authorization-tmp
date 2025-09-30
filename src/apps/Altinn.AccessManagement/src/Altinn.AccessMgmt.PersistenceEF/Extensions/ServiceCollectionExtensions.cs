using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Data;
using Altinn.AccessMgmt.PersistenceEF.Models.Legacy;
using Altinn.AccessMgmt.PersistenceEF.Models.Legacy.Enums;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Host.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccessManagementDatabase(this IServiceCollection services, Action<AccessManagementDatabaseOptions> configureOptions)
    {
        var options = new AccessManagementDatabaseOptions(configureOptions);
        ConstantGuard.ConstantIdsAreUnique();
        services.AddScoped<ReadOnlyInterceptor>();
        services.AddScoped<IAuditContextAccessor, AuditContextAccessor>();
        services.AddScoped<ITranslationService, TranslationService>();
        return options.Source switch
        {
            SourceType.App => services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var db = sp.GetRequiredService<IAltinnDatabase>();
                var connectionString = db.CreatePgsqlConnection(SourceType.App);
                options.UseNpgsql(connectionString, ConfigureNpgsql);
            }),
            SourceType.Migration => services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var db = sp.GetRequiredService<IAltinnDatabase>();
                var connectionString = db.CreatePgsqlConnection(SourceType.Migration);
                var configuration = sp.GetRequiredService<IConfiguration>();
                options.UseAsyncSeeding(async (dbcontext, anyChanges, ct) =>
                {
                    var appDbContext = (AppDbContext)dbcontext;
                    var ingest = new StaticDataIngest(appDbContext, new TranslationService(appDbContext), configuration);
                    await ingest.IngestAll(ct);
                });

                options.UseNpgsql(connectionString, ConfigureNpgsql)
                    .ReplaceService<IMigrationsSqlGenerator, CustomMigrationsSqlGenerator>();

                options.UseNpgsql(opt =>
                {
                    opt.MapEnum<UuidType>("delegation", nameof(UuidType).ToLower());
                    opt.MapEnum<DelegationChangeType>("delegation", nameof(DelegationChangeType).ToLower());
                });
            }),
            _ => throw new ArgumentException("Invalid configured source must be either <App, Migration>", nameof(configureOptions)),
        };
    }

    private static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder builder) { }

    public class AccessManagementDatabaseOptions
    {
        public AccessManagementDatabaseOptions(Action<AccessManagementDatabaseOptions> configureOptions)
        {
            configureOptions(this);
        }

        public SourceType Source { get; set; } = SourceType.App;
    }
}
