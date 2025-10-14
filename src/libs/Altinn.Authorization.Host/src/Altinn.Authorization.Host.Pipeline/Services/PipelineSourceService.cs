using System.Collections.Concurrent;
using System.Diagnostics;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Host.Pipeline.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.Host.Pipeline.Services;

/// <summary>
/// Executes pipeline source functions and streams messages to the outbound queue.
/// </summary>
internal partial class PipelineSourceService(ILogger<PipelineSourceService> logger, IServiceProvider serviceProvider)
{
    private static ulong _sequence = 1;

    /// <summary>
    /// Runs the pipeline source, enumerating messages and forwarding them to the outbound queue.
    /// </summary>
    public async Task Run<TOut>(
        PipelineArgs args,
        PipelineSource<TOut> func,
        BlockingCollection<PipelineMessage<TOut>> outbound,
        CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            await EnumerateSource(args, outbound, func, cancellationTokenSource);
        }
        finally
        {
            outbound.CompleteAdding();
        }
    }

    private async Task EnumerateSource<TOut>(
        PipelineArgs args,
        BlockingCollection<PipelineMessage<TOut>> outbound,
        PipelineSource<TOut> func,
        CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            using var serviceScope = args.Descriptor.ServiceScope?.Invoke(serviceProvider) ?? serviceProvider.CreateScope();
            var context = new PipelineSourceContext()
            {
                Lease = args.Lease,
                Services = serviceScope,
            };

            await EnumerateSource(args, outbound, func, context, cancellationTokenSource);
        }
        catch (InvalidOperationException ex)
        {
            // Possible Outbound Queue is closed. Should not happen as Soruce is the one to close it.
            Log.OutboundQueueClosed(logger, args.Descriptor.Name, args.Name, ex);
            throw;
        }
        catch (OperationCanceledException ex)
        {
            // Someone issued termination of the pipeline.
            Log.PipelineCancelled(logger, args.Descriptor.Name, args.Name);
            Activity.Current?.AddException(ex);
            PipelineTelemetry.RecordSourceFailure(args);
            Log.OutboundQueueClosed(logger, args.Descriptor.Name, args.Name, ex);
        }
        catch (Exception ex)
        {
            // Unhandled exception
            PipelineTelemetry.RecordSourceFailure(args);
            Log.StreamingFailed(logger, args.Descriptor.Name, args.Name, ex);
        }
    }

    private async Task EnumerateSource<TOut>(
        PipelineArgs args,
        BlockingCollection<PipelineMessage<TOut>> outbound,
        PipelineSource<TOut> func,
        PipelineSourceContext context,
        CancellationTokenSource cancellationTokenSource)
    {
        await using var enumerator = func(context, cancellationTokenSource.Token).GetAsyncEnumerator();

        Log.SourceStart(logger, args.Descriptor.Name, args.Name);

        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            using var parentActivity = PipelineTelemetry.ActivitySource.StartActivity($"Pipeline: {args.Descriptor.Name}", ActivityKind.Internal);
            using var sourceActivity = PipelineTelemetry.ActivitySource.StartActivity($"Pipeline Source: {args.Name}", ActivityKind.Internal);
            sourceActivity.SetSequence(_sequence);

            Log.SourceMessageStart(logger, args.Descriptor.Name, args.Name, _sequence);

            if (!await enumerator.MoveNextAsync())
            {
                Log.SourceEnumerationCompleted(logger, args.Descriptor.Name, args.Name);
                break;
            }

            var data = enumerator.Current;

            PipelineTelemetry.RecordSourceSuccess(args);
            outbound.Add(
                new(data, parentActivity?.Context, cancellationTokenSource)
                {
                    Sequence = Interlocked.Increment(ref _sequence),
                },
                cancellationTokenSource.Token);

            Log.SourceMessageEmitted(logger, args.Descriptor.Name, args.Name, _sequence);
        }

        Log.SourceCompleted(logger, args.Descriptor.Name, args.Name);
    }

    static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Pipeline '{pipeline}' source '{source}' started enumeration.")]
        internal static partial void SourceStart(ILogger logger, string pipeline, string source);

        [LoggerMessage(1, LogLevel.Information, "Pipeline '{pipeline}' source '{source}' started producing message with sequence #{sequence}.")]
        internal static partial void SourceMessageStart(ILogger logger, string pipeline, string source, ulong sequence);

        [LoggerMessage(2, LogLevel.Information, "Pipeline '{pipeline}' source '{source}' emitted message with sequence #{sequence}.")]
        internal static partial void SourceMessageEmitted(ILogger logger, string pipeline, string source, ulong sequence);

        [LoggerMessage(3, LogLevel.Information, "Pipeline '{pipeline}' source '{source}' completed enumeration.")]
        internal static partial void SourceEnumerationCompleted(ILogger logger, string pipeline, string source);

        [LoggerMessage(4, LogLevel.Warning, "Outbound queue closed before source '{source}' in pipeline '{pipeline}' completed.")]
        internal static partial void OutboundQueueClosed(ILogger logger, string pipeline, string source, Exception ex);

        [LoggerMessage(5, LogLevel.Warning, "Pipeline '{pipeline}' source '{source}' execution cancelled as requested.")]
        internal static partial void PipelineCancelled(ILogger logger, string pipeline, string source);

        [LoggerMessage(6, LogLevel.Information, "Pipeline '{pipeline}' source '{source}' finished enumeration.")]
        internal static partial void SourceCompleted(ILogger logger, string pipeline, string source);

        [LoggerMessage(7, LogLevel.Error, "Unhandled exception occurred while streaming from pipeline '{pipeline}' source '{source}'.")]
        internal static partial void StreamingFailed(ILogger logger, string pipeline, string source, Exception ex);

        [LoggerMessage(8, LogLevel.Error, "Unhandled exception while processing pipeline '{pipeline}' source '{source}' message with sequence #{sequence}.")]
        internal static partial void SinkUnhandledError(ILogger logger, string pipeline, string source, ulong sequence, Exception ex);
    }
}

/// <summary>
/// Context provided to pipeline source functions.
/// </summary>
public class PipelineSourceContext
{
    /// <summary>
    /// The distributed lease associated with this pipeline, if configured.
    /// </summary>
    public ILease? Lease { get; set; }

    /// <summary>
    /// The scoped service provider for resolving dependencies.
    /// </summary>
    public IServiceScope Services { get; set; }
}

/// <summary>
/// Delegate for pipeline source functions that produce messages asynchronously.
/// </summary>
/// <typeparam name="TOut">The output message type.</typeparam>
public delegate IAsyncEnumerable<TOut> PipelineSource<TOut>(PipelineSourceContext context, CancellationToken cancellationToken);
