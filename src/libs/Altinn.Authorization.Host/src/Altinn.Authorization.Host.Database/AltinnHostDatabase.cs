using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Altinn.Authorization.Host.Database;

public static class AltinnHostDatabase
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddAltinnDatabase(this IHostApplicationBuilder builder, Action<AltinnHostDatabaseOptions> configureOptions)
    {
        var options = new AltinnHostDatabaseOptions();
        configureOptions?.Invoke(options);

        if (builder.Services.Contains(Markers.MigrationSource.ServiceDescriptor) && options.MigrationSource == null)
        {
            builder.Services.AddNpgsqlDataSource(options.MigrationSource.ToString(), options.MigrationSource.Builder, serviceKey: SourceType.Migration);
            builder.Services.Add(Markers.MigrationSource.ServiceDescriptor);
        }

        if (builder.Services.Contains(Markers.AppSource.ServiceDescriptor) && options.AppSource == null)
        {
            builder.Services.AddNpgsqlDataSource(options.AppSource.ToString(), serviceKey: SourceType.App);
            builder.Services.Add(Markers.AppSource.ServiceDescriptor);
        }

        if (builder.Services.Contains(Markers.AppSource.ServiceDescriptor) || builder.Services.Contains(Markers.MigrationSource.ServiceDescriptor))
        {
            builder.Services.AddSingleton<IAltinnDatabase, AltinnHostDatabaseFactory>();
        }

        return builder;
    }

    private static class Markers
    {
        internal sealed class MigrationSource
        {
            public static readonly ServiceDescriptor ServiceDescriptor = ServiceDescriptor.Singleton<MigrationSource, MigrationSource>();
        }

        internal sealed class AppSource
        {
            public static readonly ServiceDescriptor ServiceDescriptor = ServiceDescriptor.Singleton<AppSource, AppSource>();
        }
    }
}
