using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Altinn.Authorization.Host.Job.Telemetry;

public static class JobTelemetry
{
    private static readonly ConcurrentDictionary<string, ActivitySource> _sources = new();

    private static readonly ConcurrentDictionary<string, Meter> _meters = new();

    public static ActivitySource ActivitySource(string domain) =>
        _sources.GetOrAdd(domain, d => new ActivitySource($"jobs.{d}"));

    public static Meter Meter(string domain) =>
        _meters.GetOrAdd($"jobs.{domain}", d => new(d));

    public static class JobExecution
    {
        private static readonly ConcurrentDictionary<string, Counter<int>> _counters = new();

        public static void Record(string domain, JobResult result)
        {
            var counter = _counters.GetOrAdd(domain, d =>
                Meter(d).CreateCounter<int>(
                    name: "job.execution.count",
                    unit: "1",
                    description: "Counts job executions by status"
                )
            );

            counter.Add(1, new TagList
        {
            { "domain", domain },
            { "result", result.ToString().ToLowerInvariant() }
        });
        }

        private static Meter Meter(string domain) => new($"jobs.{domain}");
    }
}

public static class ActivityExtensions
{
    public static void SetJobNextSchedule(this Activity? activity, TimeSpan span)
    {
        activity?.SetTag("job.schedule.utc", DateTime.UtcNow.Add(span));
        activity?.SetTag("job.schedule.remaining_seconds", span.TotalSeconds);
    }

    public static void SetDispatchResultsRun(this Activity? activity, JobResult result)
    {
        activity?.SetTag("job.dispatch.results.run", result);
    }

    public static void SetDispatchResultsShouldRun(this Activity? activity, bool shouldRun)
    {
        activity?.SetTag($"job.dispatch.results.should_run", shouldRun);
    }

    public static void SetDispatchRunAlways(this Activity? activity, bool value)
    {
        activity?.SetTag($"job.dispatch.run_always", value);
    }

    public static void SetDispatchStatus(this Activity? activity, string status)
    {
        activity?.SetTag("job.dispatch.status", status);
    }

    public static void SetDispatchFeatureFlag(this Activity? activity, string featureflag, bool value)
    {
        activity?.SetTag($"job.dispatch.feature_management.{featureflag}", value);
    }

    public static void SetDispatchDependsOn(this Activity? activity, string? dependsOn)
    {
        if (!string.IsNullOrEmpty(dependsOn))
        {
            activity?.SetTag("job.dispatch.depends_on", dependsOn);
        }
    }
}
