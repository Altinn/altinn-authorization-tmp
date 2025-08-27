using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Altinn.Authorization.Host.Job.Telemetry;

public static class JobTelemetry
{
    private static readonly ConcurrentDictionary<string, ActivitySource> _sources = new();

    private static readonly ConcurrentDictionary<string, Meter> _meters = new();

    internal static ActivitySource ActivitySource(string domain) =>
        _sources.GetOrAdd(domain, d => new ActivitySource($"jobs.{d}"));

    internal static Meter Meter(string domain) =>
        _meters.GetOrAdd($"jobs.{domain}", d => new(d));

    internal static class JobExecution
    {
        private static readonly ConcurrentDictionary<string, Counter<int>> _counters = new();

        internal static void Record(string domain, JobResult result)
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

internal static class ActivityExtensions
{
    internal static void SetAggregatedResults(this Activity? activity, JobStatus status)
    {
        activity?.SetTag($"jobs.results", status);
    }

    internal static void SetLeaseLost(this Activity? activity, bool lost)
    {
        activity?.SetTag($"lease.lost", lost);
    }

    internal static void SetLeastStatus(this Activity? activity, string lockName, bool gotLease)
    {
        activity?.SetTag($"lease.name", gotLease);
        activity?.SetTag($"lease.got_lease", gotLease);
    }

    internal static void SetNextSchedule(this Activity? activity, TimeSpan span)
    {
        activity?.SetTag("schedule.utc", DateTime.UtcNow.Add(span));
        activity?.SetTag("schedule.remaining_seconds", span.TotalSeconds);
    }

    internal static void SetResultsRun(this Activity? activity, JobResult result)
    {
        activity?.SetTag("results.run", result);
    }

    internal static void SetResultsCanRun(this Activity? activity, bool canRun)
    {
        activity?.SetTag($"results.can_run", canRun);
    }

    internal static void SetFeatureFlag(this Activity? activity, string featureFlag, bool enabled)
    {
        activity?.SetTag($"feature_flag.name", featureFlag);
        activity?.SetTag($"feature_flag.enabled", enabled);
    }
}
