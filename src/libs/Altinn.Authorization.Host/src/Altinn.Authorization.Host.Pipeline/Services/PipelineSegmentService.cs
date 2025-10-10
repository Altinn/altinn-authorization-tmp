using System.Collections.Concurrent;
using System.Diagnostics;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Host.Pipeline.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.Host.Pipeline.Services;

/// <summary>
/// Executes pipeline segment transformations with retry logic.
/// </summary>
internal partial class PipelineSegmentService(ILogger<PipelineSegmentService> logger, IServiceProvider serviceProvider)
{
    /// <summary>
    /// Runs the pipeline segment, transforming messages from inbound to outbound queue.
    /// </summary>
    public async Task Run<TIn, TOut>(
        IPipelineDescriptor descriptor,
        PipelineState state,
        PipelineSegment<TIn, TOut> func,
        BlockingCollection<PipelineMessage<TIn>> inbound,
        BlockingCollection<PipelineMessage<TOut>> outbound)
    {
        try
        {
            await EnumerateSegment(descriptor, state, inbound, outbound, func);
        }
        catch (InvalidOperationException ex)
        {
            // Should only occur if inbound is closed. However, this shouldn't be throwed as we use the enumerator of inbound queue.
            Log.InboundQueueClosed(logger, ex);
        }
        finally
        {
            outbound.CompleteAdding();
        }
    }

    private async Task EnumerateSegment<TIn, TOut>(
        IPipelineDescriptor descriptor,
        PipelineState state,
        BlockingCollection<PipelineMessage<TIn>> inbound,
        BlockingCollection<PipelineMessage<TOut>> outbound,
        PipelineSegment<TIn, TOut> func)
    {
        using var serviceScope = descriptor.ServiceScope?.Invoke(serviceProvider) ?? serviceProvider.CreateScope();
        foreach (var data in inbound.GetConsumingEnumerable())
        {
            using var activity = PipelineTelemetry.ActivitySource.StartActivity($"Pipeline Segment: {descriptor.Name}", ActivityKind.Internal, data.ActivityContext ?? default);
            try
            {
                var ctx = new PipelineSegmentContext<TIn>()
                {
                    Data = data.Data,
                    Lease = state.Lease,
                    Services = serviceScope
                };

                var result = await DispatchSegment(func, ctx);
                outbound.Add(new(result, data.ActivityContext, data.CancellationTokenSource));
            }
            catch (InvalidOperationException ex)
            {
                // Should only occur if segment fails to process the same data repeatedly.
                activity?.AddException(ex);
                data.CancellationTokenSource.Cancel();
            }
        }
    }

    private async Task<TOut> DispatchSegment<TIn, TOut>(
        PipelineSegment<TIn, TOut> func,
        PipelineSegmentContext<TIn> item)
    {
        const int maxAttempts = 3;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var result = await func(item);
                return result;
            }
            catch (Exception ex)
            {
                Log.SegmentAttemptFailed(logger, attempt, maxAttempts, ex);

                if (attempt >= maxAttempts)
                {
                    Log.SegmentRetriesExhausted(logger, typeof(TIn).Name);
                    throw new InvalidOperationException($"Segment failed after {maxAttempts} attempts.", ex);
                }

                await Task.Delay(TimeSpan.FromSeconds(attempt));
            }
        }

        throw new InvalidOperationException($"Unreachable code and segment failed after {maxAttempts} attempts.");
    }

    static partial class Log
    {
        [LoggerMessage(0, LogLevel.Debug, "Inbound queue closed; stopping segment consumption.")]
        internal static partial void InboundQueueClosed(ILogger logger, Exception ex);

        [LoggerMessage(1, LogLevel.Warning, "Pipeline segment attempt {Attempt}/{MaxAttempts} failed.")]
        internal static partial void SegmentAttemptFailed(ILogger logger, int Attempt, int MaxAttempts, Exception ex);

        [LoggerMessage(2, LogLevel.Information, "Pipeline segment execution cancelled.")]
        internal static partial void SegmentCancelled(ILogger logger);

        [LoggerMessage(3, LogLevel.Error, "Segment failed after all retry attempts for message type {MessageType}.")]
        internal static partial void SegmentRetriesExhausted(ILogger logger, string MessageType);
    }
}

/// <summary>
/// Context provided to pipeline segment functions.
/// </summary>
/// <typeparam name="TData">The message data type.</typeparam>
public class PipelineSegmentContext<TData>
{
    /// <summary>
    /// The message data being processed.
    /// </summary>
    public required TData Data { get; init; }
    
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
/// Delegate for pipeline segment functions that transform messages.
/// </summary>
/// <typeparam name="TIn">The input message type.</typeparam>
/// <typeparam name="TOut">The output message type.</typeparam>
public delegate Task<TOut> PipelineSegment<TIn, TOut>(PipelineSegmentContext<TIn> context);
