using System.Diagnostics;
using Altinn.Authorization.Host.Lease.Telemetry;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System.Threading;

namespace Altinn.Authorization.Host.Lease.StorageAccount;

/// <summary>
/// Represents the result of a lease acquisition operation for a blob in the storage account.
/// Contains information about the lease, the associated blob, and the lease client.
/// </summary>
internal sealed class StorageAccountLeaseResult : IAltinnLeaseResult, IDisposable, IAsyncDisposable
{
    internal StorageAccountLeaseResult(
        BlobClient blobClient,
        BlobLeaseClient blobLeaseClient,
        BlobLease blobLease,
        StorageAccountLease implementation)
    {
        BlobClient = blobClient;
        BlobLeaseClient = blobLeaseClient;
        BlobLease = blobLease;
        Implementation = implementation;

        if (blobLease is { })
        {
            Stopwatch = Stopwatch.StartNew();
            RenewalTask = LeaseRefresher(LeaseRefresherCancellation.Token);
        }
    }

    internal BlobLease BlobLease { get; set; }

    /// <summary>
    /// Gets the <see cref="BlobClient"/> instance used to interact with the associated blob.
    /// </summary>
    internal BlobClient BlobClient { get; init; }

    /// <summary>
    /// Gets the <see cref="Azure.Storage.Blobs.Specialized.BlobLeaseClient"/> instance used to manage the lease for the associated blob.
    /// </summary>
    internal BlobLeaseClient BlobLeaseClient { get; init; }

    /// <summary>
    /// Gets the <see cref="IAltinnLease"/> implementation associated with this lease result.
    /// </summary>
    internal StorageAccountLease Implementation { get; init; }

    /// <summary>
    /// Rwlock
    /// </summary>
    internal ReaderWriterLockSlim RwLock { get; init; } = new(LockRecursionPolicy.SupportsRecursion);

    private int _disposed = 0;

    /// <summary>
    /// Stopwatch
    /// </summary>
    private Stopwatch Stopwatch { get; init; }

    private CancellationTokenSource LeaseRefresherCancellation { get; init; } = new();

    private Task? RenewalTask { get; init; }

    /// <summary>
    /// Gets Cancelled if lease is lost.
    /// </summary>
    /// <param name="cancellationTokens">Cancellation Token.</param>
    public CancellationToken LinkTokens(params CancellationToken[] cancellationTokens)
    {
        return CancellationTokenSource.CreateLinkedTokenSource(
            new[] { LeaseRefresherCancellation.Token }.Concat(cancellationTokens).ToArray()
        ).Token;
    }

    internal void Cancel()
    {
        LeaseRefresherCancellation.Cancel();
    }

    /// <summary>
    /// Background renewal loop that runs every 30 seconds (half the lease duration).
    /// </summary>
    private async Task LeaseRefresher(CancellationToken cancellationToken)
    {
        try
        {
            var refreshDelay = StorageAccountLease.MaxLeaseTime / 2;
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(refreshDelay, cancellationToken).ConfigureAwait(false);
                await Implementation.RefreshLease(this, cancellationToken).ConfigureAwait(false);
            }
        }
        catch
        {
        }
        finally
        {
            Stopwatch.Stop();
            LeaseTelemetry.RecordLeaseDuration(BlobClient.Name, Stopwatch.Elapsed);
        }
    }

    /// <summary>
    /// Synchronous dispose — lightweight (just cancels and disposes resources).
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        try
        {
            LeaseRefresherCancellation.Cancel();
            Implementation.Release(this);
        }
        catch
        {
        }

        RwLock.Dispose();
        LeaseRefresherCancellation.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronous dispose — waits for background tasks and releases lease properly.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        try
        {
            await LeaseRefresherCancellation.CancelAsync();
            if (RenewalTask is { })
            {
                try
                {
                    await RenewalTask.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                }
                catch
                {
                }
            }

            await Implementation.Release(this, default).ConfigureAwait(false);
        }
        catch
        {
        }

        RwLock.Dispose();
        LeaseRefresherCancellation.Dispose();
        GC.SuppressFinalize(this);
    }
}
