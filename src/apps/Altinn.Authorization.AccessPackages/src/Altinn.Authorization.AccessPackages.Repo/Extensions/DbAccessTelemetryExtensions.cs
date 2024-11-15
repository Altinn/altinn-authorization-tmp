using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Altinn.Authorization.AccessPackages.Repo.Extensions;

public static class DbAccessTelemetryExtensions
{
    public static IHostApplicationBuilder AddDbAccessRepoTelemetry(this IHostApplicationBuilder builder, Action<TelemetryConfig>? configureOptions = null)
    {
        var config = new TelemetryConfig(config =>
        {
            builder.Configuration.GetSection("TelemetryConfig").Bind(config);
            configureOptions?.Invoke(config);
        });

        Sdk.CreateTracerProviderBuilder()
              .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Repo", serviceInstanceId: config.ServiceName))
              .AddSource("Altinn.Authorization.AccessPackages.Repo")
              .AddOtlpExporter()
              .Build();

        return builder;
    }

    public static IHostApplicationBuilder AddDbAccessDataTelemetry(this IHostApplicationBuilder builder, Action<TelemetryConfig>? configureOptions = null)
    {
        var config = new TelemetryConfig(config =>
        {
            builder.Configuration.GetSection("TelemetryConfig").Bind(config);
            configureOptions?.Invoke(config);
        });

        Sdk.CreateTracerProviderBuilder()
              .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DbAccess", serviceInstanceId: config.ServiceName))
              .AddSource("Altinn.Authorization.AccessPackages.DbAccess")
              .AddOtlpExporter()
              .Build();

        return builder;
    }

    [Obsolete]
    public static IHostApplicationBuilder AddDbAccessTelemetry(this IHostApplicationBuilder builder, Action<TelemetryConfig>? configureOptions = null)
    {
        builder.Services.Configure<TelemetryConfig>(c => 
        {
            builder.Configuration.GetRequiredSection("TelemetryConfig").Bind(c);
            configureOptions?.Invoke(c);
        });

        var config = new TelemetryConfig(config =>
        {
            builder.Configuration.GetSection("TelemetryConfig").Bind(config);
            configureOptions?.Invoke(config);
        });
        
        //builder.Logging.AddOpenTelemetry(options =>
        //{
        //    options
        //        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(config.ServiceName))
        //        .AddOtlpExporter()
        //        .AddConsoleExporter();
        //});

        return builder;
    }

    [Obsolete]
    public static IHostApplicationBuilder AddDbAccessDataTelemetryOld(this IHostApplicationBuilder builder, Action<TelemetryConfig>? configureOptions = null)
    {
        var config = new TelemetryConfig(config =>
        {
            builder.Configuration.GetSection("TelemetryConfig").Bind(config);
            configureOptions?.Invoke(config);
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("DbAccess"))
            .WithTracing(opts => opts
                .AddSource("Altinn.Authorization.DbAccess")
                .AddConsoleExporter()
                .AddOtlpExporter()
            );

        return builder;
    }

    [Obsolete]
    public static IHostApplicationBuilder AddDbAccessRepoTelemetryOld(this IHostApplicationBuilder builder, Action<TelemetryConfig>? configureOptions = null)
    {
        var config = new TelemetryConfig(config =>
        {
            builder.Configuration.GetSection("TelemetryConfig").Bind(config);
            configureOptions?.Invoke(config);
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("Repo"))
            .WithTracing(opts => opts
                .AddSource("Altinn.Authorization.Repo")
                .AddConsoleExporter()
                .AddOtlpExporter()
            );

        return builder;
    }
}
