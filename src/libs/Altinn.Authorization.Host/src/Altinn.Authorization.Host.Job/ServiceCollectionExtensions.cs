using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.Host.Lease;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.Host.Job;

public static class ApplicationBuilderExtensions
{
    public static IServiceCollection AddJobs(this IServiceCollection services, string domain, Action<JobList> configureJobs)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain, nameof(domain));
        services.AddHostedService<JobDispatchers>();

        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder.AddSource($"jobs.{domain}");
            }).WithMetrics(builder =>
            {
                builder.AddMeter($"jobs.{domain}");
            });

        var jobs = new JobList(domain);
        configureJobs?.Invoke(jobs);
        foreach (var job in jobs.Jobs)
        {
            services.AddSingleton(job.Type);
            services.AddSingleton(typeof(IJob), job.Type);
        }

        services.AddSingleton(jobs);

        return services;
    }
}

public class JobList(string domain)
{
    internal List<JobItem> Jobs { get; } = [];

    internal string LeaseName { get; set; }

    public string Domain { get; } = domain;

    public TimeSpan Interval { get; } = TimeSpan.FromSeconds(1);

    public JobList Add<T>(Action<JobOptions> configureOptions = null)
        where T : IJob
    {
        var options = new JobOptions()
        {
            Name = typeof(T).Name,
        };
        configureOptions?.Invoke(options);

        Jobs.Add(new(typeof(T), options));

        return this;
    }

    [DoesNotReturn]
    private void Unreachable()
    {
        throw new UnreachableException();
    }

    public ExecutionGraph CreatePlan(IServiceProvider provider)
    {
        var plan = new ExecutionGraph();
        var notAddedJobs = new List<JobItem>();
        var jobToAdd = Jobs.ToList();

        while (jobToAdd.Count != 0)
        {
            var count = jobToAdd.Count;
            foreach (var job in jobToAdd)
            {
                var service = provider.GetService(job.Type) as IJob;
                if (service == null)
                {
                    Unreachable();
                }

                if (!plan.Add(service, job.Options))
                {
                    notAddedJobs.Add(job);
                }
            }

            jobToAdd = notAddedJobs;
            if (count == jobToAdd.Count)
            {
                Unreachable();
            }

            notAddedJobs = [];
        }

        return plan;
    }

    public class JobItem(Type type, JobOptions options)
    {
        internal Type Type { get; set; } = type;

        internal JobOptions Options { get; set; } = options;
    }
}

public interface IJob
{
    Task<bool> ShouldRun(JobContext context, CancellationToken cancellationToken = default);

    Task<JobResult> Run(JobContext context, CancellationToken cancellationToken = default);
}

public class JobContext
{
    public IAltinnLease Lease { get; set; }
}

public class JobOptions
{
    public string Name { get; set; }

    public bool RequireLease { get; set; } = true;

    public string DependsOn { get; set; }

    public bool EnsureLease { get; set; } = true;

    public bool RunAlways { get; set; } = false;

    public string FeatureFlag { get; set; }
}

public enum JobResult
{
    Success = 0,

    Skipped = 1,

    Failure = 2,
}
