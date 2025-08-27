using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using Altinn.Authorization.Host.Lease;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.Host.Job;

public abstract class JobBase : IJob
{
    public virtual Task<bool> CanRun(JobContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public JobResult? JobIsCancelled(CancellationToken cancellationToken)
    {
        Guard.IsNotNull(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            return JobResult.Cancelled("Hosted service is requesting cancellation.");
        }

        return null;
    }

    public JobResult? JobHasLease<T>(LeaseResult<T> lease)
    {
        Guard.IsNotNull(lease);
        if (!lease.HasLease)
        {
            return JobResult.LostLease("Lease got lost during job execution.");
        }

        return null;
    }

    public abstract Task<JobResult> Run(JobContext context, CancellationToken cancellationToken);
}
