using System.Collections.Concurrent;
using System.Diagnostics;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Host.Pipeline.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.Host.Pipeline.Services;

/// <summary>
/// Executes pipeline sink functions with retry logic and scoped services.
/// </summary>
internal partial class PipelineSinkService(ILogger<PipelineSinkService> logger, IServiceProvider serviceProvider)
{
    /// <summary>
    /// Runs the pipeline sink, consuming messages from the inbound queue.
    /// </summary>
    public async Task Run<TIn>(
        IPipelineDescriptor descriptor,
        PipelineState state,
        BlockingCollection<PipelineMessage<TIn>> inbound,
        PipelineSink<TIn> fn)
    {
        try
        {
            await EnumerateSink(descriptor, state, inbound, fn);
        }
        catch (InvalidOperationException ex)
        {
            // Should only occur if inbound is closed. However, this shouldn't be throwed as we use the enumerator of inbound queue.
            Log.InboundQueueClosed(logger, ex);
        }
    }

    private async Task EnumerateSink<TIn>(
        IPipelineDescriptor descriptor,
        PipelineState state,
        BlockingCollection<PipelineMessage<TIn>> inbound,
        PipelineSink<TIn> func)
    {
        using var serviceScope = descriptor.ServiceScope?.Invoke(serviceProvider) ?? serviceProvider.CreateScope();
        foreach (var data in inbound.GetConsumingEnumerable())
        {
            try
            {
                using var activity = PipelineTelemetry.ActivitySource.StartActivity($"Pipeline Sink: {descriptor.Name}", ActivityKind.Internal, data.ActivityContext ?? default);
                var ctx = new PipelineSinkContext<TIn>()
                {
                    Lease = state.Lease,
                    Data = data.Data,
                    Services = serviceScope,
                };

                await DispatchSegment(func, ctx);
            }
            catch (InvalidOperationException)
            {
                // Should only occur if segment fails to process the same data repeatedly
                data.CancellationTokenSource.Cancel();
            }
        }
    }

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
    }

    static partial class Log
    {
        [LoggerMessage(0, LogLevel.Debug, "Inbound queue closed; stopping segment consumption.")]
        internal static partial void InboundQueueClosed(ILogger logger, Exception ex);

        [LoggerMessage(1, LogLevel.Warning, "Pipeline segment attempt {Attempt}/{MaxAttempts} failed.")]
        internal static partial void SegmentAttemptFailed(ILogger logger, int Attempt, int MaxAttempts, Exception ex);

        [LoggerMessage(2, LogLevel.Information, "Pipeline sink execution cancelled.")]
        internal static partial void SegmentCancelled(ILogger logger);

        [LoggerMessage(3, LogLevel.Error, "Sink failed after all retry attempts for message type {MessageType}.")]
        internal static partial void SinkRetriesExhausted(ILogger logger, string MessageType);
    }
}

/// <summary>
/// Context provided to pipeline sink functions.
/// </summary>
/// <typeparam name="TIn">The message data type.</typeparam>
public class PipelineSinkContext<TIn>
{
    /// <summary>
    /// The message data being processed.
    /// </summary>
    public required TIn Data { get; init; }

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
/// Delegate for pipeline sink functions that perform final message processing.
/// </summary>
/// <typeparam name="TIn">The input message type.</typeparam>
public delegate Task PipelineSink<TIn>(PipelineSinkContext<TIn> context);
