using Altinn.Authorization.Host.Job.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.Host.Job;

public static class ApplicationBuilderExtensions
{
    public static IServiceCollection AddJobs(this IServiceCollection services, string domain, Action<JobOptions> configureJobs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain, nameof(domain));
        services.AddHostedService<JobDispatchers>();

        var jobs = new JobOptions(services, domain);
        configureJobs?.Invoke(jobs);
        foreach (var job in jobs.Jobs)
        {
            services.AddSingleton(job.Type);
            services.AddSingleton(typeof(IJob), job.Type);
        }

        services.AddSingleton(jobs);
        if (jobs.EnableTelemetry)
        {
            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder.AddSource($"jobs.{domain}");
                }).WithMetrics(builder =>
                {
                    builder.AddMeter($"jobs.{domain}");
                });
        }

        return services;
    }
}
