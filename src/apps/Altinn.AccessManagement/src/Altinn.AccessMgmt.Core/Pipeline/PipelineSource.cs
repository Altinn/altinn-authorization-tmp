using System.Collections.Concurrent;
using System.Diagnostics;
using Altinn.Authorization.Host.Lease.Telemetry;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Core;

public partial class PipelineSource<TOut>(ILogger<PipelineSource<TOut>> logger)
{
    public delegate IAsyncEnumerable<T> PipelineSourceDelegate<T>(PipelineContext context, CancellationToken cancellationToken);

    public async Task Run(BlockingCollection<PipelineMessage<TOut>> outbound, PipelineSourceDelegate<TOut> fn, CancellationToken cancellationToken)
    {
        try
        {
            var context = new PipelineContext();
            await foreach (var data in DispatchSegment(fn, context, cancellationToken))
            {
                using var activty = PipelineTelemetry.ActivitySource.StartActivity("Processing", ActivityKind.Producer);
                outbound.Add(new(data, activty.Context), CancellationToken.None);
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
