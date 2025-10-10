using System.Collections.Concurrent;
using System.Diagnostics;
using Altinn.Authorization.Host.Telemetry;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.Host.Pipeline.Services;

/// <summary>
/// Executes a single segment within a processing pipeline by consuming input messages from an inbound queue,
/// invoking the specified segment function to transform each message, and publishing the processed results
/// to an outbound queue.
/// <para>
/// If a segment repeatedly fails to process an item beyond the configured retry limit,
/// a cancellation request is issued for the entire pipeline to ensure consistent shutdown behavior.
/// </para>
/// <typeparam name="TIn">The type of data received from the previous pipeline stage.</typeparam>
/// <typeparam name="TOut">The type of data emitted to the next pipeline stage.</typeparam>
/// <param name="logger">The logger used for structured diagnostics and telemetry events.</param>
internal partial class PipelineSegmentJob(ILogger<PipelineSegmentJob> logger)
{
    /// <summary>
    /// Begins consuming input messages, invoking the pipeline segment delegate for each,
    /// and emitting corresponding output messages into the outbound collection.
    /// </summary>
    /// <param name="inbound">The inbound queue containing messages from the previous stage.</param>
    /// <param name="outbound">The outbound queue to which processed messages will be added.</param>
    /// <param name="func">The pipeline segment delegate that transforms input into output.</param>
    public async Task Run<TIn, TOut>(
        IPipelineDescriptor descriptor,
        PipelineState state,
        PipelineSegment<TIn, TOut> func,
        BlockingCollection<PipelineSingleMessage<TIn>> inbound,
        BlockingCollection<PipelineSingleMessage<TOut>> outbound)
    {
        try
        {
            await EnumerateSegment(descriptor, state, inbound, outbound, func);
        }
        catch (InvalidOperationException ex)
        {
            // Should only occur if inbound is closed. However, this shouldn't be throwed as we use the enumerator of inbound queue.
            Log.InboundQueueClosed(logger);
        }
        finally
        {
            outbound.CompleteAdding();
        }
    }

    private async Task EnumerateSegment<TIn, TOut>(
        IPipelineDescriptor descriptor,
        PipelineState state,
        BlockingCollection<PipelineSingleMessage<TIn>> inbound,
        BlockingCollection<PipelineSingleMessage<TOut>> outbound,
        PipelineSegment<TIn, TOut> func)
    {
        foreach (var data in inbound.GetConsumingEnumerable())
        {
            using var activity = PipelineTelemetry.ActivitySource.StartActivity($"Pipeline Segment: {descriptor.Name}", ActivityKind.Internal, data.ActivityContext ?? default);
            try
            {
                var ctx = new PipelineSegmentContext<TIn>()
                {
                    Data = data.Data,
                };

                var result = await DispatchSegment(func, ctx);
                outbound.Add(new(result, data.ActivityContext, data.CancellationTokenSource));
            }
            catch (InvalidOperationException ex)
            {
                // Should only occur if segment fails to process the same data repiteadly.
                activity?.AddException(ex);
                data.CancellationTokenSource.Cancel();
            }
        }
    }

    /// <summary>
    /// Handles Task Issues
    /// </summary>
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
                // Any exception can be raised from func. Must handle
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
        internal static partial void InboundQueueClosed(ILogger logger);

        [LoggerMessage(1, LogLevel.Warning, "Pipeline segment attempt {Attempt}/{MaxAttempts} failed.")]
        internal static partial void SegmentAttemptFailed(ILogger logger, int Attempt, int MaxAttempts, Exception ex);

        [LoggerMessage(2, LogLevel.Information, "Pipeline segment execution cancelled.")]
        internal static partial void SegmentCancelled(ILogger logger);

        [LoggerMessage(3, LogLevel.Error, "Segment failed after all retry attempts for message type {MessageType}.")]
        internal static partial void SegmentRetriesExhausted(ILogger logger, string MessageType);
    }
}

public class PipelineSegmentContext<TData>
{
    public required TData Data { get; init; }
}

public delegate Task<TOut> PipelineSegment<TIn, TOut>(PipelineSegmentContext<TIn> context);
