using System.Threading.Channels;
using Altinn.Authorization.ProblemDetails;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessManagement.Core;

public static class ServiceCollectionExtensions
{
    public static void AddPipeline(this IServiceCollection collection, Action<ISourceBuilder> builder)
    {

    }
}

public class PipelineSource : ISourceBuilder
{
    public ISegmentBuilder<TOut> AddSource<TOut>(PipelineSourceDelegate<TOut> source)
    {
        throw new NotImplementedException();
    }
}

public class PipelineSegment<TIn> : ISegmentBuilder<TIn>
{
    public ISegmentBuilder<TOut> AddSegment<TOut>(PipelineSegmentDelegate<TIn, TOut> segment)
    {
        throw new NotImplementedException();
    }

    public ISinkBuilder<TIn> AddSink(PipelineSinkDelegate<TIn> sink)
    {
        throw new NotImplementedException();
    }
}

public class PipelineSink<TIn> : ISinkBuilder<TIn>
{
    public void Build()
    {
        throw new NotImplementedException();
    }
}

public class PipelineContext<TData>
{
    public required TData Data { get; init; }
}

public class PipelineContext
{
}

public delegate Task<IAsyncEnumerable<TOut>> PipelineSourceDelegate<TOut>(PipelineContext context, CancellationToken cancellationToken);

public delegate Task<Result<TOut>> PipelineSegmentDelegate<TIn, TOut>(PipelineContext<TIn> context, CancellationToken cancellationToken);

public delegate Task PipelineSinkDelegate<TIn>(PipelineContext<TIn> context);

public interface ISourceBuilder
{
    ISegmentBuilder<TOut> AddSource<TOut>(PipelineSourceDelegate<TOut> source);
}

public interface ISegmentBuilder<TIn>
{
    ISegmentBuilder<TOut> AddSegment<TOut>(PipelineSegmentDelegate<TIn, TOut> segment);

    ISinkBuilder<TIn> AddSink(PipelineSinkDelegate<TIn> sink);
}

public interface ISinkBuilder<TIn>
{
    void Build();
}
