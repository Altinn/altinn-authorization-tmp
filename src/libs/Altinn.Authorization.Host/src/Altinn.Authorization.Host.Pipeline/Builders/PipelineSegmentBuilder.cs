using Altinn.Authorization.Host.Pipeline.Services;

namespace Altinn.Authorization.Host.Pipeline.Builders;

internal class PipelineSegmentBuilder<TIn>(
    PipelineGroup descriptor
    ) : ISegmentBuilder<TIn>
{
    internal string Name { get; private set; }
    
    internal object Func { get; private set; }
    
    internal object? Segment { get; private set; }
    
    internal object? Sink { get; private set; }

    /// <inheritdoc/>
    public ISegmentBuilder<TOut> AddSegment<TOut>(string name, PipelineSegment<TIn, TOut> func)
    {
        var segment = new PipelineSegmentBuilder<TOut>(descriptor);

        Name = name;
        Func = func;
        Segment = segment;
        return segment;
    }

    /// <inheritdoc/>
    public ISinkBuilder<TIn> AddSink(string name, PipelineSink<TIn> func)
    {
        var sink = new PipelineSinkBuilder<TIn>(descriptor);

        Name = name;
        Func = func;
        Sink = sink;
        return sink;
    }
}

/// <summary>
/// Builder for configuring pipeline segments (transformations) and sinks.
/// Segments transform messages: Source → Segment₁ → ... → Segmentₙ → Sink.
/// </summary>
/// <typeparam name="TIn">The input message type.</typeparam>
public interface ISegmentBuilder<TIn>
{
    /// <summary>
    /// Adds a transformation segment.
    /// Segments transform messages and are retried up to 3 times on failure.
    /// </summary>
    /// <typeparam name="TOut">The output message type.</typeparam>
    /// <param name="name">Name of the next segment.</param>
    /// <param name="segment">Transform function: receives <typeparamref name="TIn"/>, returns <typeparamref name="TOut"/>.</param>
    /// <returns>Builder for the next stage.</returns>
    ISegmentBuilder<TOut> AddSegment<TOut>(string name, PipelineSegment<TIn, TOut> segment);

    /// <summary>
    /// Adds the terminal sink stage.
    /// Sinks perform final processing (e.g., database writes, API calls) and are retried up to 3 times on failure.
    /// </summary>
    /// <param name="name">Name of the sink.</param>
    /// <param name="sink">Sink function: consumes <typeparamref name="TIn"/> and performs final processing.</param>
    /// <returns>Builder to complete pipeline configuration.</returns>
    ISinkBuilder<TIn> AddSink(string name, PipelineSink<TIn> sink);
}
