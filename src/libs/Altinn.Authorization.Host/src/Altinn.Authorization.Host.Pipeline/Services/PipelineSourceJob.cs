using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using Altinn.Authorization.Host.Telemetry;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.Host.Pipeline.Services;

/// <summary>
/// Represents a job that runs a <see cref="PipelineSource{TOut}"/> delegate
/// and streams its output messages into a downstream <see cref="BlockingCollection{T}"/>.
/// </summary>
/// <typeparam name="TOut">The type of elements produced by the pipeline source.</typeparam>
internal partial class PipelineSourceJob(ILogger<PipelineSourceJob> logger)
{
    public async Task Run<TOut>(
        IPipelineDescriptor descriptor,
        PipelineState state,
        PipelineSource<TOut> func,
        BlockingCollection<PipelineSingleMessage<TOut>> outbound,
        CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            await EnumerateSource(descriptor, state, outbound, func, cancellationTokenSource);
        }
        finally
        {
            cancellationTokenSource.Cancel();
            outbound.CompleteAdding();
        }
    }

    private async Task EnumerateSource<TOut>(
        IPipelineDescriptor descriptor,
        PipelineState state,
        BlockingCollection<PipelineSingleMessage<TOut>> outbound,
        PipelineSource<TOut> func,
        CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            var context = new PipelineSourceContext();
            await foreach (var data in func(context, cancellationTokenSource.Token))
            {
                using var activity = PipelineTelemetry.ActivitySource.StartActivity($"Pipeline Source: {descriptor.Name}:", ActivityKind.Internal);
                outbound.Add(new(data, activity?.Context, cancellationTokenSource));
            }
        }
        catch (InvalidOperationException)
        {
            Log.OutboundQueueClosed(logger);
        }
        catch (OperationCanceledException)
        {
            Log.PipelineCancelled(logger);
        }
        catch (Exception ex)
        {
            Log.StreamingFailed(logger, ex);
        }
    }

    static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Outbound queue closed before source completed. Stopping source enumeration.")]
        internal static partial void OutboundQueueClosed(ILogger logger);

        [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Pipeline execution cancelled.")]
        internal static partial void PipelineCancelled(ILogger logger);

        [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Unhandled exception occurred while streaming from source.")]
        internal static partial void StreamingFailed(ILogger logger, Exception ex);
    }
}

public class PipelineSourceContext
{
}

public delegate IAsyncEnumerable<TOut> PipelineSource<TOut>(PipelineSourceContext context, CancellationToken cancellationToken);
