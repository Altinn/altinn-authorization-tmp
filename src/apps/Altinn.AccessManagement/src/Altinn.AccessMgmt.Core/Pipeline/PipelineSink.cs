using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Core;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TIn"></typeparam>
/// <typeparam name="TOut"></typeparam>
/// <param name="logger">logger</param>
public partial class Sink<TIn>(ILogger<Sink<TIn>> logger)
{
    public async Task Run(BlockingCollection<TIn> inbound, PipelineSinkDelegate<TIn> fn)
    {
        foreach (var item in inbound.GetConsumingEnumerable())
        {
            var ctx = new PipelineContext<TIn>()
            {
                Data = item,
            };

            await DispatchSegment(fn, ctx);
        }
    }

    /// <summary>
    /// Handles Task Issues
    /// </summary>
    private async Task DispatchSegment(PipelineSinkDelegate<TIn> fn, PipelineContext<TIn> item)
    {
        try
        {
            await fn(item);
        }
        catch (Exception ex)
        {

        }
    }

    static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Parent Channel is closed, shutting down.")]
        internal static partial void ParentChannelIsClosed(ILogger logger);
    }
}
