using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Altinn.Authorization.Host.Job;

public class JobOptions
{
    public JobOptions(IServiceCollection services, string domain)
    {
        Services = services;
        Domain = domain;
    }

    private IServiceCollection Services { get; init; }

    internal List<JobDescriptor> Jobs { get; } = [];

    public string Domain { get; init; }

    public TimeSpan Interval { get; } = TimeSpan.FromMinutes(1);

    public string Lease { get; set; }

    public string FeatureFlag { get; set; }

    public bool EnableTelemetry { get; set; } = true;

    public JobOptions Add<T>(Action<JobDescriptor> configureOptions = null)
        where T : class, IJob
    {
        var descriptor = new JobDescriptor()
        {
            Name = typeof(T).FullName,
            Type = typeof(T),
        };

        Services.TryAddSingleton<T>();

        configureOptions?.Invoke(descriptor);

        Jobs.Add(descriptor);

        return this;
    }
}

public class JobDescriptor
{
    public string Name { get; set; }

    public List<string> Dependencies { get; set; } = [];

    public bool RunAlways { get; set; } = false;

    internal Type Type { get; init; }

    public JobDescriptor DependsOn(string jobName)
    {
        Dependencies.Add(jobName);
        return this;
    }

    public JobDescriptor DependsOn<T>()
        where T : IJob
    {
        Dependencies.Add(typeof(T).FullName);
        return this;
    }
}
