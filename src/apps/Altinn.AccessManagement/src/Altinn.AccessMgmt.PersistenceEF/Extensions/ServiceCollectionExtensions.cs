using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Data;
using Altinn.AccessMgmt.PersistenceEF.Extensions.Hint;
using Altinn.AccessMgmt.PersistenceEF.Extensions.ReadOnly;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Host.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccessManagementDatabase(this IServiceCollection services, Action<AccessManagementDatabaseOptions> configureOptions)
    {
        var options = new AccessManagementDatabaseOptions(configureOptions);
        ConstantGuard.ConstantIdsAreUnique();

        services.AddSingleton<IConnectionStringSelector>(sp => new ConnectionStringSelector(options, sp.GetRequiredService<IHintService>()));
        services.AddSingleton<IHintService, HintService>();
        services.AddSingleton<ConnectionStringSelectorInterceptor>();
        services.AddSingleton<HintSaveChangesInterceptor>();

        services.AddScoped<IAuditAccessor, AuditAccessor>();
        services.AddScoped<ITranslationService, TranslationService>();
        services.AddScoped<ConnectionQuery>();
        services.AddScoped<AppDbContextFactory>();

        services.AddSingleton<AuditMiddleware>();

        if (options.EnableEFPooling)
        {
            return options.Source switch
            {
                SourceType.App => services.AddPooledDbContextFactory<AppDbContext>((sp, opt) => AddAppDbContext(sp, opt, options)),
                SourceType.Migration => services.AddPooledDbContextFactory<AppDbContext>((sp, opt) => AddMigrationDbContext(sp, opt, options)),
                _ => throw new ArgumentException("Invalid configured source must be either <App, Migration>", nameof(configureOptions)),
            };
        }

        return options.Source switch
        {
            SourceType.App => services.AddDbContextFactory<AppDbContext>((sp, opt) => AddAppDbContext(sp, opt, options)),
            SourceType.Migration => services.AddDbContextFactory<AppDbContext>((sp, opt) => AddMigrationDbContext(sp, opt, options)),
            _ => throw new ArgumentException("Invalid configured source must be either <App, Migration>", nameof(configureOptions)),
        };
    }

    private static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder builder)
    {
        builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
    }

    private static void AddMigrationDbContext(IServiceProvider sp, DbContextOptionsBuilder options, AccessManagementDatabaseOptions databaseOptions)
    {
        options.UseAsyncSeeding(async (dbcontext, anyChanges, ct) => await StaticDataIngest.IngestAll((AppDbContext)dbcontext, ct));
        options.UseNpgsql(databaseOptions.MigrationConnectionString, ConfigureNpgsql).ReplaceService<IMigrationsSqlGenerator, CustomMigrationsSqlGenerator>();
    }

    private static void AddAppDbContext(IServiceProvider sp, DbContextOptionsBuilder options, AccessManagementDatabaseOptions databaseOptions)
    {
        options.UseNpgsql(databaseOptions.AppConnectionString, ConfigureNpgsql);

        var connectionInterceptor = sp.GetRequiredService<ConnectionStringSelectorInterceptor>();
        var saveChangesInterceptor = sp.GetRequiredService<HintSaveChangesInterceptor>();

        options.AddInterceptors(connectionInterceptor, saveChangesInterceptor);
    }

    public class AccessManagementDatabaseOptions
    {
        public AccessManagementDatabaseOptions(Action<AccessManagementDatabaseOptions> configureOptions)
        {
            configureOptions(this);
        }

        public SourceType Source { get; set; } = SourceType.App;

        public bool EnableEFPooling { get; set; } = false;

        public string MigrationConnectionString { get; set; } = string.Empty;

        public string AppConnectionString { get; set; } = string.Empty;

        public Dictionary<string, string> ReadOnlyConnectionStrings { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        public bool IncludePrimaryInReadOnlyPool { get; set; } = false;
        
        public bool EnableReadOnlyHints { get; set; } = false;
    }
}
