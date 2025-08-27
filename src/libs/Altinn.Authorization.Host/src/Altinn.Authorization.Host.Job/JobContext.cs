using Altinn.Authorization.Host.Job.Service;
using Altinn.Authorization.Host.Lease;

namespace Altinn.Authorization.Host.Job;

public class JobContext
{
    public JobOptions Options { get; set; }

    public IAltinnLease Lease { get; init; }

    public JobDescriptor Descriptor { get; init; }

    public IEnumerable<JobSchedulerResult> DependsOnResults { get; init; }

    public IServiceProvider ServiceProvider { get; init; }
}

public interface IJob
{
    Task<bool> CanRun(JobContext context, CancellationToken cancellationToken);

    Task<JobResult> Run(JobContext context, CancellationToken cancellationToken);
}
