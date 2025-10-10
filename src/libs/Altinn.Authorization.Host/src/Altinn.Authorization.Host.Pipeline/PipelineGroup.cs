namespace Altinn.Authorization.Host.Pipeline;

internal class PipelineGroup : IPipelineGroup
{
    internal string GroupName { get; set; }

    internal List<PipelineDescriptor> Builders { get; set; } = [];

    internal TimeSpan? Recurring { get; set; }

    internal string FeatureFlag { get; set; }

    public IPipelineGroup WithFeatureFlag(string featureFlag)
    {
        FeatureFlag = featureFlag;
        return this;
    }

    public IPipelineGroup WithRecurring(TimeSpan recurring)
    {
        Recurring = recurring;
        return this;
    }

    public IPipelineGroup WithGroupName(string groupName)
    {
        GroupName = groupName;
        return this;
    }

    public IPipelineDescriptor AddPipeline(string name)
    {
        var builder = new PipelineDescriptor(this);
        Builders.Add(builder);
        return builder.WithName(name);
    }
}

public interface IPipelineGroup
{
    IPipelineGroup WithRecurring(TimeSpan timeSpan);

    IPipelineGroup WithGroupName(string name);

    IPipelineGroup WithFeatureFlag(string featureFlag);

    IPipelineDescriptor AddPipeline(string name);
}