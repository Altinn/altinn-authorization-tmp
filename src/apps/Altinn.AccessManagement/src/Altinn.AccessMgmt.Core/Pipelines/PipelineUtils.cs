using System.Diagnostics;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Integration.Platform;
using Altinn.Authorization.ProblemDetails;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessMgmt.Core.Pipelines;

internal static class PipelineUtils
{
    internal static async Task<int> Flush<T>(IServiceScope services, List<T> items, IEnumerable<string> matchColumns, CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            return 0;
        }

        var activity = Activity.Current;
        var ingest = services.ServiceProvider.GetRequiredService<IIngestService>();
        var audit = services.ServiceProvider.GetRequiredService<IAuditAccessor>().AuditValues;

        var batchId = Guid.CreateVersion7();
        activity?.AddTag("batch_id", batchId);
        activity?.AddTag("batch_size", items.Count);

        var ingestTemp = await ingest.IngestTempData(items, batchId, cancellationToken);
        activity?.AddTag("ingested_temp", ingestTemp);

        var flushed = await ingest.MergeTempData<T>(batchId, audit, matchColumns, cancellationToken);
        activity?.AddTag("ingested_merge", flushed);

        return flushed;
    }

    internal static void EnsureSuccess<T>(PlatformResponse<PageStream<T>> page)
    {
        var activity = Activity.Current;
        if (page.IsProblem)
        {
            throw new InvalidOperationException(page.ProblemDetails.Detail);
        }

        if (page?.Content?.Data is { } data)
        {
            activity?.AddTag("page_size", data.Count());
        }

        if (page?.Content?.Links is { } link)
        {
            activity?.AddTag("page_next_link", link);
        }

        if (page?.Content?.Stats is { } stats)
        {
            activity?.AddTag("page_stats_end", stats.PageEnd);
            activity?.AddTag("page_stats_start", stats.PageStart);
            activity?.AddTag("page_stats_sequence_max", stats.SequenceMax);
        }
    }
}
