namespace Altinn.Authorization.Host.Pipeline;

internal class PipelineRegistry : IPipelineRegistry
{
    public List<PipelineGroup> Groups { get; } = [];
}

internal interface IPipelineRegistry
{
    public List<PipelineGroup> Groups { get; }
}
