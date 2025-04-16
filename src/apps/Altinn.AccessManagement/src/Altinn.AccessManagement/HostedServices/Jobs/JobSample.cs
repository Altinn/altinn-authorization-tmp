using Altinn.Authorization.Host.Job;

namespace Altinn.AccessManagement.HostedServices.Jobs;

public class JobSample : IJob
{
    public Task<JobResult> Run(JobContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((JobResult)Random.Shared.Next(0, 3));
    }

    public Task<bool> ShouldRun(JobContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
