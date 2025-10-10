using System.Collections.Concurrent;
using System.Diagnostics;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Host.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.Host.Pipeline.Services;

/// <summary>
/// Represents the terminal stage of a processing pipeline.  
/// The sink job consumes messages from an inbound queue, invokes the provided sink delegate,
/// and completes any final processing such as persistence, dispatch, or acknowledgment.
/// <para>
/// Unlike other pipeline stages, the sink does not emit further messages.
/// It is responsible for completing or releasing resources associated with each message,
/// ensuring that all in-flight work is flushed before the pipeline shuts down.
/// </para>
/// </summary>
/// <param name="options"></param>
/// <param name="logger">
/// The <see cref="ILogger"/> used for structured diagnostics and telemetry.
/// </param>
internal partial class PipelineSinkJob(
    IServiceProvider serviceProvider,
    ILogger<PipelineSinkJob> logger
)
{
    public async Task Run<TIn>(
        IPipelineDescriptor descriptor,
        PipelineState state,
        BlockingCollection<PipelineSingleMessage<TIn>> inbound,
        PipelineSink<TIn> fn)
    {
        try
        {
            await EnumerateSink(descriptor, state, inbound, fn);
        }
        catch (InvalidOperationException ex)
        {
            // Should only occur if inbound is closed. However, this shouldn't be throwed as we use the enumerator of inbound queue.
            Log.InboundQueueClosed(logger);
        }
    }

    private async Task EnumerateSink<TIn>(
        IPipelineDescriptor descriptor,
        PipelineState state,
        BlockingCollection<PipelineSingleMessage<TIn>> inbound,
        PipelineSink<TIn> func)
    {
        foreach (var data in inbound.GetConsumingEnumerable())
        {
            try
            {
                using var activity = PipelineTelemetry.ActivitySource.StartActivity($"Pipeline Sink: {descriptor.Name}", ActivityKind.Internal, data.ActivityContext ?? default);
                var ctx = new PipelineSinkContext<TIn>()
                {
                    Lease = state.Lease,
                    Data = data.Data,
                };

                await DispatchSegment(func, ctx);
            }
            catch (InvalidOperationException ex)
            {
                // Should only occur if segment fails to process the same data repiteadly
                data.CancellationTokenSource.Cancel();
            }
        }
    }

    /// <summary>
    /// Handles Task Issues
    /// </summary>
    private async Task DispatchSegment<TIn>(
        PipelineSink<TIn> func,
        PipelineSinkContext<TIn> item)
    {
        const int maxAttempts = 3;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await func(item);
                return;
            }
            catch (Exception ex)
            {
                Log.SegmentAttemptFailed(logger, attempt, maxAttempts, ex);

                if (attempt >= maxAttempts)
                {
                    Log.SinkRetriesExhausted(logger, typeof(TIn).Name);
                    throw new InvalidOperationException($"Segment failed after {maxAttempts} attempts.", ex);
                }

                await Task.Delay(TimeSpan.FromSeconds(attempt));
            }
        }

        throw new InvalidOperationException($"Unreachable code and segment failed after {maxAttempts} attempts.");
    }

    /// <summary>
    /// Provides structured logging for sink operations.
    /// </summary>
    static partial class Log
    {
        [LoggerMessage(0, LogLevel.Debug, "Inbound queue closed; stopping segment consumption.")]
        internal static partial void InboundQueueClosed(ILogger logger);

        [LoggerMessage(1, LogLevel.Warning, "Pipeline segment attempt {Attempt}/{MaxAttempts} failed.")]
        internal static partial void SegmentAttemptFailed(ILogger logger, int Attempt, int MaxAttempts, Exception ex);

        [LoggerMessage(2, LogLevel.Information, "Pipeline sink execution cancelled.")]
        internal static partial void SegmentCancelled(ILogger logger);

        [LoggerMessage(3, LogLevel.Error, "Sink failed after all retry attempts for message type {MessageType}.")]
        internal static partial void SinkRetriesExhausted(ILogger logger, string MessageType);
    }
}

public class PipelineSinkContext<TIn>
{
    public required TIn Data { get; init; }

    public ILease? Lease { get; set; }

    public IServiceScope Services { get; set; }
}

public delegate Task PipelineSink<TIn>(PipelineSinkContext<TIn> context);
