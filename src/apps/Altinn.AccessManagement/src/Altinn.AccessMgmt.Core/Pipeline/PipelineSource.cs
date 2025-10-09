using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Core;

public partial class PipelineSource<TOut>(ILogger<PipelineSource<TOut>> logger)
{
    public delegate IAsyncEnumerable<T> PipelineSourceDelegate<T>(PipelineContext context, CancellationToken cancellationToken);

    public async Task Run(BlockingCollection<TOut> outbound, PipelineSourceDelegate<TOut> fn, CancellationToken cancellationToken)
    {
        try
        {
            var context = new PipelineContext();
            await foreach (var item in DispatchSegment(fn, context, cancellationToken))
            {
                outbound.Add(item, CancellationToken.None);
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
    private IAsyncEnumerable<TOut> DispatchSegment(PipelineSourceDelegate<TOut> fn, PipelineContext item, CancellationToken cancellationToken)
    {
        return fn(item, cancellationToken);
    }

    static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Parent Channel is closed, shutting down.")]
        internal static partial void ParentChannelIsClosed(ILogger logger);
    }
}
