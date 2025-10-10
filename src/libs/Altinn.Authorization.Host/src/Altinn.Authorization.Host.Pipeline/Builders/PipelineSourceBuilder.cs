using Altinn.Authorization.Host.Pipeline.Services;

namespace Altinn.Authorization.Host.Pipeline.Builders;

internal class PipelineSourceBuilder(PipelineGroup descriptor) : ISourceBuilder
{
    internal string Name { get; private set; }

    internal object Func { get; private set; }

    internal object Segment { get; private set; }

    public ISegmentBuilder<TOut> AddSource<TOut>(PipelineSource<TOut> source)
    {
        var segment = new PipelineSegmentBuilder<TOut>(descriptor);
        Func = source;
        Segment = segment;
        return segment;
    }
}

public interface ISourceBuilder
{
    ISegmentBuilder<TOut> AddSource<TOut>(PipelineSource<TOut> source);
}
