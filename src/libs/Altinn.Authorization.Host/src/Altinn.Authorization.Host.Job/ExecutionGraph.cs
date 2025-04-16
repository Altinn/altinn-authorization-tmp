namespace Altinn.Authorization.Host.Job;

public class ExecutionGraph
{
    public List<ExecutionNode> Roots { get; set; } = [];

    public bool Add(IJob job, JobOptions options)
    {
        if (string.IsNullOrEmpty(options.DependsOn))
        {
            Roots.Add(new(job, options));
            return true;
        }
        else
        {
            foreach (var node in Roots)
            {
                return node.TryAdd(job, options);
            }
        }

        return false;
    }
}

public class ExecutionNode(IJob job, JobOptions jobOptions)
{
    public IJob Job { get; set; } = job;

    public JobOptions Options { get; set; } = jobOptions;

    public List<ExecutionNode> Childs { get; set; } = [];

    public bool TryAdd(IJob newJob, JobOptions newOptions)
    {
        if (string.Equals(newOptions.DependsOn, Options.Name, StringComparison.InvariantCultureIgnoreCase))
        {
            Childs.Add(new(newJob, newOptions));
            return true;
        }

        foreach (var child in Childs)
        {
            if (child.TryAdd(newJob, newOptions))
            {
                return true;
            }
        }

        return false;
    }
}
