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
    /// <summary>
    /// Runs the pipeline source, enumerating messages and forwarding them to the outbound queue.
    /// </summary>
    public async Task Run<TOut>(
        IPipelineDescriptor descriptor,
        PipelineState state,
        PipelineSource<TOut> func,
        BlockingCollection<PipelineMessage<TOut>> outbound,
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
        BlockingCollection<PipelineMessage<TOut>> outbound,
        PipelineSource<TOut> func,
        CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            using var serviceScope = descriptor.ServiceScope?.Invoke(serviceProvider) ?? serviceProvider.CreateScope();
            var context = new PipelineSourceContext()
            {
                Lease = state.Lease,
                Services = serviceScope,
            };

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
