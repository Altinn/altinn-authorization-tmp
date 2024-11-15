using Altinn.Authorization.Importers.BRREG.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altinn.Authorization.Importers.BRREG.Extensions;

/// <summary>
/// DbAccessExtensions
/// </summary>
public static class DbAccessExtensions
{
    /// <summary>
    /// AddBrregIngestor
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <param name="configureOptions">BrRegIngestorConfig</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddBrregIngestor(this IHostApplicationBuilder builder, Action<BrRegIngestorConfig>? configureOptions = null)
    {
        builder.Services.Configure<BrRegIngestorConfig>(config =>
        {
            builder.Configuration.GetSection("BrRegIngestorConfig").Bind(config);
            configureOptions?.Invoke(config);
        });

        builder.Services.AddMetrics();
        builder.Services.AddSingleton<BrregIngestMetricsOld>();
        builder.Services.AddSingleton<BrRegIngestor>();
        return builder;
    }

    /// <summary>
    /// UseBrregIngestor
    /// </summary>
    /// <param name="services">IServiceProvider</param>
    /// <returns></returns>
    public async static Task<IServiceProvider> UseBrregIngestor(this IServiceProvider services)
    {
        var definitions = services.GetRequiredService<BrRegIngestor>();
        await definitions.IngestAll();
        return services;
    }

    /// <summary>
    /// AddBrregIngestor
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <param name="configureOptions">BrRegIngestorConfig</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddBrregImporter(this IHostApplicationBuilder builder, Action<BrRegImporterConfig>? configureOptions = null)
    {
        builder.Services.Configure<BrRegImporterConfig>(config =>
        {
            builder.Configuration.GetSection("BrRegImporterConfig").Bind(config);
            configureOptions?.Invoke(config);
        });

        builder.Services.AddSingleton<Importer>();
        return builder;
    }

    /// <summary>
    /// UseBrregIngestor
    /// </summary>
    /// <param name="services">IServiceProvider</param>
    /// <returns></returns>
    public async static Task<IServiceProvider> UseBrregImporter(this IServiceProvider services)
    {
        var service = services.GetRequiredService<Importer>();
        //await service.ImportUnit();
        //await service.ImportSubUnit();
        //await service.ImportRoles();
        service.WriteChangeRefsToConsole();
        return services;
    }
}
