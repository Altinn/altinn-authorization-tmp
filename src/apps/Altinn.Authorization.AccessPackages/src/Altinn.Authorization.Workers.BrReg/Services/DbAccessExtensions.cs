using Altinn.Authorization.Workers.BrReg.Services;

namespace Altinn.Authorization.Workers.BrReg.Extensions;

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
    public static IHostApplicationBuilder AddBrregIngestor(this IHostApplicationBuilder builder, Action<BrRegConfig>? configureOptions = null)
    {
        builder.Services.Configure<BrRegConfig>(config =>
        {
            builder.Configuration.GetSection("BrRegConfig").Bind(config);
            configureOptions?.Invoke(config);
        });

        builder.Services.AddMetrics();
        builder.Services.AddSingleton<Ingestor>();
        return builder;
    }

    /// <summary>
    /// UseBrregIngestor
    /// </summary>
    /// <param name="services">IServiceProvider</param>
    /// <returns></returns>
    public async static Task<IServiceProvider> UseBrregIngestor(this IServiceProvider services)
    {
        var definitions = services.GetRequiredService<Ingestor>();
        await definitions.IngestAll();
        return services;
    }

    /// <summary>
    /// AddBrregIngestor
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <param name="configureOptions">BrRegIngestorConfig</param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddBrregImporter(this IHostApplicationBuilder builder, Action<BrRegConfig>? configureOptions = null)
    {
        builder.Services.Configure<BrRegConfig>(config =>
        {
            builder.Configuration.GetSection("BrRegConfig").Bind(config);
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
        await service.ImportUnit();
        await service.ImportSubUnit();
        await service.ImportRoles();
        service.WriteChangeRefsToConsole();
        return services;
    }
}
