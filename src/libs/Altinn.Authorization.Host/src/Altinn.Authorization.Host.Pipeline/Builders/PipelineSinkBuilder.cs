namespace Altinn.Authorization.Host.Pipeline.Builders;

internal class PipelineSinkBuilder<TIn>(
    PipelineGroup descriptor
) : ISinkBuilder<TIn>
{
    public IPipelineGroup Build()
    {
        return descriptor;
    }
}

public interface ISinkBuilder<TIn>
{
    IPipelineGroup Build();
}
