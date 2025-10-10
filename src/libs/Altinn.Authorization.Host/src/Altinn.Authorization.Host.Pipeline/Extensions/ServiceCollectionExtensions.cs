using Altinn.Authorization.Host.Pipeline.HostedServices;
using Altinn.Authorization.Host.Pipeline.Services;
using Altinn.Authorization.Host.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Altinn.Authorization.Host.Pipeline.Extensions;

public static class ServiceCollectionExtensions
{
    public static TracerProviderBuilder AddPipeline(this TracerProviderBuilder builder) =>
        builder.AddSource(PipelineTelemetry.ActivitySource.Name);

    public static IServiceCollection AddPipelinesOtel(this IServiceCollection services)
    {
        services.ConfigureOpenTelemetryTracerProvider(otel => otel.AddPipeline());
        return services;
    }

    public static IServiceCollection AddPipelines(
        this IServiceCollection services,
        Action<IPipelineGroup> configureDescriptor)
    {
        var descriptor = new PipelineGroup();
        configureDescriptor(descriptor);

        if (!services.Any(s => s.ServiceType == typeof(PipelineMarker)))
        {
            services.AddSingleton<PipelineSourceJob>();
            services.AddSingleton<PipelineSegmentJob>();
            services.AddSingleton<PipelineSinkJob>();
            services.AddSingleton<PipelineMarker>();
            services.AddHostedService<PipelineHostedService>();
        }

        var registry = GetOrCreateRegistry(services);
        registry.Groups.Add(descriptor);

        return services;
    }

    private static PipelineRegistry GetOrCreateRegistry(IServiceCollection services)
    {
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IPipelineRegistry));

        if (descriptor?.ImplementationInstance is PipelineRegistry existing)
        {
            return existing;
        }

        var registry = new PipelineRegistry();
        services.AddSingleton<IPipelineRegistry>(registry);
        return registry;
    }

    private record PipelineMarker { }
}
