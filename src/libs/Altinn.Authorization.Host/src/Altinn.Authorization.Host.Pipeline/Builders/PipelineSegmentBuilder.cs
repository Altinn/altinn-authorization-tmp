using Altinn.Authorization.Host.Pipeline.Services;

namespace Altinn.Authorization.Host.Pipeline.Builders;

internal class PipelineSegmentBuilder<TIn>(PipelineGroup descriptor) : ISegmentBuilder<TIn>
{
    public string Name { get; private set; }

    public object? Func { get; private set; }

    public object? Segment { get; private set; }

    public object? Sink { get; private set; }

    public ISegmentBuilder<TOut> AddSegment<TOut>(PipelineSegment<TIn, TOut> func)
    {
        var segment = new PipelineSegmentBuilder<TOut>(descriptor);
        Func = func;
        Segment = segment;
        return segment;
    }

    public ISinkBuilder<TIn> AddSink(PipelineSink<TIn> func)
    {
        var sink = new PipelineSinkBuilder<TIn>(descriptor);
        Func = func;
        Sink = sink;
        return sink;
    }
}

public interface ISegmentBuilder<TIn>
{
    ISegmentBuilder<TOut> AddSegment<TOut>(PipelineSegment<TIn, TOut> segment);

    ISinkBuilder<TIn> AddSink(PipelineSink<TIn> sink);
}
