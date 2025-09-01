using System.Diagnostics;
using Altinn.Authorization.Host.Lease.Telemetry;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Altinn.Authorization.Host.Lease.StorageAccount;

/// <summary>
/// Represents the result of a lease acquisition operation for a blob in the storage account.
/// Contains information about the lease, the associated blob, and the lease client.
/// </summary>
internal class StorageAccountLeaseResult : LeaseResult
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

    private bool _disposed = false;

    /// <summary>
    /// Stopwatch
    /// </summary>
    private Stopwatch Stopwatch { get; init; }

    private CancellationTokenSource LeaseRefresherCancellation { get; init; } = new CancellationTokenSource();

    private Task RenewalTask { get; init; }

    /// <summary>
    /// Gets Cancelled if lease is lost.
    /// </summary>
    /// <param name="cancellationTokens">Cancellation Token.</param>
    /// <returns></returns>
    public CancellationToken LinkTokens(params CancellationToken[] cancellationTokens)
    {
        return CancellationTokenSource.CreateLinkedTokenSource([LeaseRefresherCancellation.Token, .. cancellationTokens]).Token;
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
                await Task.Delay(refreshDelay, cancellationToken);
                await Implementation.RefreshLease(this, cancellationToken);
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            Stopwatch.Stop();
            LeaseTelemetry.RecordLeaseDuration(BlobClient.Name, Stopwatch.Elapsed);
        }
    }

    /// <summary>
    /// Releases the lease synchronously when the object is disposed.
    /// </summary>
    public override void Dispose()
    {
        if (!_disposed)
        {
            LeaseRefresherCancellation?.Cancel();
            try
            {
                RenewalTask?.Wait(TimeSpan.FromSeconds(5));
            }
            catch (Exception)
            {
            }

            try
            {
                Implementation.Release(this, default).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch
            {
            }

            LeaseRefresherCancellation?.Dispose();
        }

        _disposed = true;
    }

    /// <summary>
    /// Releases the lease asynchronously when the object is disposed.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public override async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            LeaseRefresherCancellation?.Cancel();
            if (RenewalTask is { })
            {
                try
                {
                    await RenewalTask.WaitAsync(TimeSpan.FromSeconds(5));
                }
                catch (Exception)
                {
                }
            }

            try
            {
                await Implementation.Release(this, default).ConfigureAwait(false);
            }
            catch
            {
            }

            LeaseRefresherCancellation?.Dispose();
        }

        _disposed = true;
    }
}
