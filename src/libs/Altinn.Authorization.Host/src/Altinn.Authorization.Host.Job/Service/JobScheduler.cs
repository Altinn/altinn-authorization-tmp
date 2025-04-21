using System.Diagnostics;
using System.Threading.Channels;
using Altinn.Authorization.Host.Job.Telemetry;

namespace Altinn.Authorization.Host.Job.Service;

internal class JobScheduler
{
    public Dictionary<string, List<JobSchedulerNode>> DependsOn { get; } = [];

    public List<JobSchedulerNode> Nodes { get; set; } = [];

    public async Task<IEnumerable<JobSchedulerResult>> WaitForDependencies(JobSchedulerNode node, CancellationToken cancellationToken = default)
    {
        var aggregate = new List<JobSchedulerResult>();
        var activity = Activity.Current;
        for (var i = 0; i < node.Descriptor.DependsOn.Count; i++)
        {
            var result = await node.DependsOnResults.Reader.ReadAsync(cancellationToken);
            aggregate.Add(result);
        }

        return aggregate;
    }

    public async Task NotifyDependents(JobResult result, JobSchedulerNode node, CancellationToken cancellationToken = default)
    {
        var activity = Activity.Current;
        if (DependsOn.TryGetValue(node.Descriptor.Name, out var jobs))
        {
            var value = new JobSchedulerResult(result, node.Descriptor);
            foreach (var job in jobs)
            {
                await job.DependsOnResults.Writer.WriteAsync(value, cancellationToken);
            }
        }
    }

    public static JobScheduler Create(JobOptions list)
    {
        var executor = new JobScheduler();
        foreach (var job in list.Jobs)
        {
            var node = new JobSchedulerNode(job);
            if (job.DependsOn.Count != 0)
            {
                foreach (var dependsOn in job.DependsOn)
                {
                    if (executor.DependsOn.TryGetValue(dependsOn, out var depend))
                    {
                        depend.Add(node);
                    }
                    else
                    {
                        executor.DependsOn[dependsOn] = [node];
                    }
                }
            }

            executor.Nodes.Add(node);
        }

        return executor;
    }
}

public class JobSchedulerResult(JobResult result, JobDescriptor descriptor)
{
    public JobResult Result { get; } = result;

    public JobDescriptor Descriptor { get; } = descriptor;
}

internal class JobSchedulerNode(JobDescriptor descriptor)
{
    internal JobDescriptor Descriptor { get; } = descriptor;

    private readonly Lazy<Channel<JobSchedulerResult>> _parentResults = new(() =>
        Channel.CreateBounded<JobSchedulerResult>(descriptor.DependsOn.Count > 0 ? descriptor.DependsOn.Count : 1));

    internal Channel<JobSchedulerResult> DependsOnResults => _parentResults.Value;
}
