using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Altinn.Authorization.AccessPackages.Repo.Extensions;

public static class DbAccessTelemetryExtensions
{
    public static void AddDbAccessTelemetry()
    {
        //services.ConfigureOpenTelemetryTracerProvider(cfg => 
        //{
        //    cfg.ConfigureResource(c => c.AddService("AccessPackages"))
        //    .AddSource("Altinn.Authorization.AccessPackages.Repo")
        //    .AddSource("Altinn.Authorization.DbAccess")
        //    .AddOtlpExporter();
        //});

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("AccessPackages"))
                .AddSource("Altinn.Authorization.AccessPackages.Repo")
                ////.AddConsoleExporter()
                .AddOtlpExporter()
                .Build();

        using var tracerProvider2 = Sdk.CreateTracerProviderBuilder()
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DbAccess"))
                        .AddSource("Altinn.Authorization.DbAccess")
                        ////.AddConsoleExporter()
                        .AddOtlpExporter()
                        .Build();
    }
}
