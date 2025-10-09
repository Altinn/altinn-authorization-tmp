using System.Collections.Concurrent;
using Altinn.Authorization.Host.Lease.Telemetry;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Core;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TIn"></typeparam>
/// <typeparam name="TOut"></typeparam>
/// <param name="logger">logger</param>
public partial class PipelineSegment<TIn, TOut>(ILogger<PipelineSegment<TIn, TOut>> logger)
{
    public async Task Run(BlockingCollection<PipelineMessage<TIn>> inbound, BlockingCollection<PipelineMessage<TOut>> outbound, PipelineSegmentDelegate<TIn, TOut> fn)
    {
        try
        {
            foreach (var data in inbound.GetConsumingEnumerable())
            {
                using var activity = PipelineTelemetry.ActivitySource.StartActivity("Processing", System.Diagnostics.ActivityKind.Producer, data.ActivityContext);
                var ctx = new PipelineContext<TIn>()
                {
                    Data = data.Data,
                };

                var result = await DispatchSegment(fn, ctx);

                outbound.Add(new()
                {
                    Data = result,
                    ActivityContext = data.ActivityContext,
                });
            }
        }
        finally
        {
            outbound.CompleteAdding();
        }
    }

    /// <summary>
    /// Handles Task Issues
    /// </summary>
    private async Task<TOut> DispatchSegment(PipelineSegmentDelegate<TIn, TOut> fn, PipelineContext<TIn> item)
    {
        try
        {
            var result = await fn(item, CancellationToken.None);
            return result.Value;
        }
        catch (Exception ex)
        {

        }

        return default;
    }

    static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Parent Channel is closed, shutting down.")]
        internal static partial void ParentChannelIsClosed(ILogger logger);
    }
}
