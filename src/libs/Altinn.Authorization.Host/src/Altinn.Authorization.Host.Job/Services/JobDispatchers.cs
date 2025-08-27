using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.Host.Job.Telemetry;
using Altinn.Authorization.Host.Lease;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;

namespace Altinn.Authorization.Host.Job.Service;

public class JobDispatchers(
    IFeatureManager FeatureManagement,
    IAltinnLease Lease,
    IServiceProvider ServiceProvider) : IHostedService
{
    private List<Task> Jobs { get; } = [];

    [DoesNotReturn]
    private void Unreachable() =>
        throw new UnreachableException();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var jobsList = ServiceProvider.GetServices<JobOptions>();
        foreach (var jobs in jobsList)
        {
            Jobs.Add(DispatchDomain(jobs, cancellationToken));
        }

        return Task.CompletedTask;
    }

    public async Task DispatchDomain(JobOptions jobs, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using (var activity = JobTelemetry.ActivitySource(jobs.Domain).StartActivity($"jobs.{jobs.Domain}", ActivityKind.Internal))
            {
                await ExecuteDomain(jobs, cancellationToken);
                activity?.SetNextSchedule(jobs.Interval);
            }

            await Task.Delay(jobs.Interval, cancellationToken);
        }
    }

    private async Task ExecuteDomain(JobOptions jobs, CancellationToken cancellationToken)
    {
        var activity = Activity.Current;
        try
        {
            if (!string.IsNullOrEmpty(jobs.FeatureFlag))
            {
                var enabled = await FeatureManagement.IsEnabledAsync(jobs.FeatureFlag);
                activity.SetFeatureFlag(jobs.FeatureFlag, enabled);
                if (!enabled)
                {
                    return;
                }
            }

            if (!string.IsNullOrEmpty(jobs.Lease))
            {
                await using var ls = await Lease.TryAquireNonBlocking<LeaseContent>(jobs.Lease, cancellationToken);
                if (ls.HasLease)
                {
                    activity.SetLeastStatus(jobs.Lease, ls.HasLease);
                    var domainCt = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    using var timer = new Timer(async _ => await LeaseRefresher(ls, domainCt), null, TimeSpan.Zero, TimeSpan.FromSeconds(15));
                    var result = await ScheduleJobs(jobs, domainCt.Token);
                    activity.SetAggregatedResults(result);
                }
            }
            else
            {
                var result = await ScheduleJobs(jobs, cancellationToken);
                activity.SetAggregatedResults(result);
            }
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "An unhandled exception was thrown by the Job framework.");
            activity?.AddException(ex);
            activity.SetAggregatedResults(JobStatus.Failure);
        }
    }

    private async Task LeaseRefresher(LeaseResult<LeaseContent> ls, CancellationTokenSource cancellationTokenSource)
    {
        var activity = Activity.Current;
        try
        {
            activity.SetLeaseLost(false);
            await Lease.RefreshLease(ls, cancellationTokenSource.Token);

            if (!ls.HasLease)
            {
                activity.SetLeaseLost(true);
                cancellationTokenSource.Cancel();
            }
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            activity?.SetLeaseLost(true);
            cancellationTokenSource.Cancel();
        }
    }

    private async Task<JobStatus> ScheduleJobs(JobOptions options, CancellationToken cancellationToken = default)
    {
        var scheduler = JobScheduler.Create(options);
        var tasks = scheduler.Nodes
            .Select(async node =>
            {
                using var activity = JobTelemetry.ActivitySource(options.Domain)
                    .StartActivity($"jobs.{options.Domain}.{node.Descriptor.Name}", ActivityKind.Internal);
                try
                {
                    var result = await DispatchJob(options, scheduler, node, cancellationToken);
                    await scheduler.NotifyDependents(result, node);
                    return result;
                }
                catch (Exception ex)
                {
                    Activity.Current?.AddException(ex);
                    await scheduler.NotifyDependents(JobResult.Failure(), node);
                    return JobResult.Failure();
                }
            });

        var results = await Task.WhenAll(tasks);

        return results
            .Max(f => f.JobStatus);
    }

    private async Task<JobResult> DispatchJob(JobOptions options, JobScheduler scheduler, JobSchedulerNode node, CancellationToken cancellationToken)
    {
        var service = ServiceProvider.GetService(node.Descriptor.Type) as IJob;

        var dependencies = await scheduler.WaitForDependencies(node, cancellationToken);
        using var scope = ServiceProvider.CreateScope();
        var jobContext = new JobContext
        {
            Options = options,
            Descriptor = node.Descriptor,
            DependsOnResults = dependencies,
            Lease = Lease,
            ServiceProvider = scope.ServiceProvider,
        };

        if (cancellationToken.IsCancellationRequested)
        {
            return JobResult.Cancelled();
        }

        if (dependencies.Any(a => a == null))
        {
            Unreachable();
        }

        if (node.Descriptor.RunAlways)
        {
            return await Execute(jobContext, service, cancellationToken);
        }

        var result = dependencies.Any() ? dependencies
            .Select(r => r.Result.JobStatus)
            .Max() : JobStatus.Success;

        if (result == JobStatus.Success)
        {
            return await Execute(jobContext, service, cancellationToken);
        }

        return new JobResult
        {
            JobStatus = result,
        };
    }

    private async Task<JobResult> Execute(JobContext context, IJob job, CancellationToken cancellationToken = default)
    {
        if (!await ExecuteJobCanRun(context, job, cancellationToken))
        {
            return JobResult.CouldNotRun();
        }

        return await ExecuteJobRun(context, job, cancellationToken);
    }

    private static async Task<JobResult> ExecuteJobRun(JobContext context, IJob job, CancellationToken cancellationToken)
    {
        using var activity = JobTelemetry.ActivitySource(context.Options.Domain)
            .StartActivity($"jobs.{context.Options.Domain}.{context.Descriptor.Name}.run", ActivityKind.Internal);

        try
        {
            var result = await job.Run(context, cancellationToken);
            return result ?? JobResult.Success();
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error);
            return JobResult.Failure();
        }
    }

    private static async Task<bool> ExecuteJobCanRun(JobContext context, IJob job, CancellationToken cancellationToken)
    {
        var canRun = false;
        using var activity = JobTelemetry.ActivitySource(context.Options.Domain)
            .StartActivity($"jobs.{context.Options.Domain}.{context.Descriptor.Name}.can_run", ActivityKind.Internal);

        try
        {
            canRun = await job.CanRun(context, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            activity?.SetStatus(ActivityStatusCode.Error);
        }
        finally
        {
            activity?.SetResultsCanRun(canRun);
        }

        return canRun;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(Jobs);
    }

    public class LeaseContent
    {
    }
}
