namespace Altinn.Authorization.Host.Job;

public class JobOptions(string domain)
{
    internal List<JobDescriptor> Jobs { get; } = [];

    public string Domain { get; } = domain;

    public TimeSpan Interval { get; } = TimeSpan.FromMinutes(1);

    public string Lease { get; set; }

    public string FeatureFlag { get; set; }

    public bool EnableTelemetry { get; set; } = true;

    public JobOptions Add<T>(Action<JobDescriptor> configureOptions = null)
        where T : IJob
    {
        var descriptor = new JobDescriptor()
        {
            Name = typeof(T).Name,
            Type = typeof(T),
        };

        configureOptions?.Invoke(descriptor);

        Jobs.Add(descriptor);

        return this;
    }
}

public class JobDescriptor
{
    public string Name { get; set; }

    public List<string> DependsOn { get; set; } = [];

    public bool RunAlways { get; set; } = false;

    internal Type Type { get; init; }
}
