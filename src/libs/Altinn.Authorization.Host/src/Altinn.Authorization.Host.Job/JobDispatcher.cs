using System.Diagnostics;
using System.Diagnostics.Metrics;
using Altinn.Authorization.Host.Job.Telemetry;
using Altinn.Authorization.Host.Lease;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;

namespace Altinn.Authorization.Host.Job;

public class JobDispatchers(IFeatureManager featureManagement, IAltinnLease lease, IServiceProvider provider) : IHostedService
{
    private IFeatureManager FeatureManagement { get; } = featureManagement;

    private IAltinnLease Lease { get; } = lease;

    private IServiceProvider Provider { get; } = provider;

    private readonly CancellationTokenSource _stop = new();

    private List<Task> Jobs { get; } = [];

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var jobsList = Provider.GetServices<JobList>();
        foreach (var jobs in jobsList)
        {
            Jobs.Add(DispatchDomain(jobs, cancellationToken));
        }

        return Task.CompletedTask;
    }

    public async Task DispatchDomain(JobList jobs, CancellationToken cancellationToken)
    {
        while (!_stop.IsCancellationRequested)
        {
            using (var activity = JobTelemetry.ActivitySource(jobs.Domain).StartActivity($"jobs.{jobs.Domain}", ActivityKind.Internal))
            {
                try
                {
                    var plan = jobs.CreatePlan(Provider);

                    var tasks = plan.Roots
                        .Select(child => Dispatch(jobs.Domain, child, JobResult.Success, false));

                    var results = await Task.WhenAll(tasks);

                    if (results.Max() == JobResult.Failure)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, "One or more jobs returned status failure.");
                    }

                    JobTelemetry.JobExecution.Record(jobs.Domain, results.Max());
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, "An unhandled exception was thrown by the Job framework.");
                    activity?.AddException(ex);
                }

                activity?.SetJobNextSchedule(jobs.Interval);
            }

            await Task.Delay(jobs.Interval, cancellationToken);
        }
    }

    public async Task<JobResult> Dispatch(string domain, ExecutionNode node, JobResult prevResult, bool hasFailed)
    {
        using var activity = JobTelemetry.ActivitySource(domain).StartActivity($"jobs.{domain}.{node.Options.Name}", ActivityKind.Internal);
        activity?.SetDispatchResultsRun(JobResult.Success);
        activity?.SetDispatchDependsOn(node.Options.DependsOn);
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.SetDispatchRunAlways(node.Options.RunAlways);

        if (node.Options.RunAlways)
        {
            return await ExecuteJob(domain, node, hasFailed);
        }
        else if (prevResult != JobResult.Success)
        {
            activity?.SetDispatchStatus("Skipping as previous job didn't run successfully and RunAlways is disabled.");
            activity?.SetDispatchResultsRun(JobResult.Skipped);
            return await DispatchChilds(domain, node, JobResult.Skipped, hasFailed);
        }

        if (!string.IsNullOrEmpty(node.Options.FeatureFlag))
        {
            var enabled = await FeatureManagement.IsEnabledAsync(node.Options.FeatureFlag);
            activity?.SetDispatchFeatureFlag(node.Options.FeatureFlag, enabled);
            if (!enabled)
            {
                activity?.SetDispatchStatus($"Skipping job as configured feature flag '{node.Options.FeatureFlag}' is disabled.");
                activity?.SetDispatchResultsRun(JobResult.Skipped);
                return await DispatchChilds(domain, node, JobResult.Skipped, hasFailed);
            }
        }

        return await ExecuteJob(domain, node, hasFailed);
    }

    private async Task<JobResult> ExecuteJob(string domain, ExecutionNode node, bool hasFailed)
    {
        var activity = Activity.Current;
        try
        {
            var shouldRun = await node.Job.ShouldRun(new(), _stop.Token);
            activity?.SetDispatchResultsShouldRun(shouldRun);
            if (!shouldRun)
            {
                activity?.SetDispatchStatus($"Skipping job as function {nameof(node.Job.ShouldRun)} returned false.");
                activity?.SetDispatchResultsRun(JobResult.Skipped);
                return await DispatchChilds(domain, node, JobResult.Skipped, hasFailed);
            }
        }
        catch (Exception ex)
        {
            activity?.SetDispatchStatus($"An unhandled exception was thrown during execution of function '{nameof(node.Job.ShouldRun)}'.");
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error);
            return await DispatchChilds(domain, node, JobResult.Failure, true);
        }

        try
        {
            var result = await node.Job.Run(new(), _stop.Token);
            activity?.SetStatus(result == JobResult.Failure ? ActivityStatusCode.Error : ActivityStatusCode.Ok);
            activity?.SetDispatchStatus("Job ran successfully.");
            return await DispatchChilds(domain, node, result, hasFailed || result == JobResult.Failure);
        }
        catch (Exception ex)
        {
            activity?.SetDispatchStatus($"An unhandled exception was thrown during execution of function '{nameof(node.Job.Run)}'.");
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error);
            return await DispatchChilds(domain, node, JobResult.Failure, true);
        }
    }

    public async Task<JobResult> DispatchChilds(string domain, ExecutionNode node, JobResult prevResult, bool hasFailed)
    {
        var tasks = node.Childs
            .Select(child => Dispatch(domain, child, prevResult, hasFailed));

        var results = await Task.WhenAll(tasks);

        return results.Append(prevResult).Max();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _stop.CancelAsync();
        Task.WaitAll(Jobs, CancellationToken.None);
    }
}
