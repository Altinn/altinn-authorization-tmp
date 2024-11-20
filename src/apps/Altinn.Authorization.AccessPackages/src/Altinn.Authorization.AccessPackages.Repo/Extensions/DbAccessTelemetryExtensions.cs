using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Altinn.Authorization.AccessPackages.Repo.Extensions;

/// <summary>
/// DbAccess Telemetry Extensions
/// </summary>
public static class DbAccessTelemetryExtensions
{
    /// <summary>
    /// Add DbAccessRepo Telemetry 
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <param name="configureOptions">TelemetryConfig</param>
    /// <returns></returns>
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

    /// <summary>
    /// Add DbAccessData Telemetry
    /// </summary>
    /// <param name="builder">IHostApplicationBuilder</param>
    /// <param name="configureOptions">TelemetryConfig</param>
    /// <returns></returns>
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
}

/// <summary>
/// TelemetryConfig
/// </summary>
public class TelemetryConfig
{
    /// <summary>
    /// ServiceName
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// TelemetryConfig
    /// </summary>
    /// <param name="configureOptions">Configuration</param>
    public TelemetryConfig(Action<TelemetryConfig> configureOptions)
    {
        configureOptions?.Invoke(this);
    }
}
