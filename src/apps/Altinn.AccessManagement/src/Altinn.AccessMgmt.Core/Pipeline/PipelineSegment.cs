using System.Collections.Concurrent;
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
    public async Task Run(BlockingCollection<TIn> inbound, BlockingCollection<TOut> outbound, PipelineSegmentDelegate<TIn, TOut> fn)
    {
        try
        {
            foreach (var item in inbound.GetConsumingEnumerable())
            {
                var ctx = new PipelineContext<TIn>()
                {
                    Data = item,
                };

                var result = await DispatchSegment(fn, ctx);
                outbound.Add(result);
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
