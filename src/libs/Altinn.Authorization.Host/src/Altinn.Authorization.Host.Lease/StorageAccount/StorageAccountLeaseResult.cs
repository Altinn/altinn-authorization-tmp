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
/// <typeparam name="T">The type of data associated with the lease.</typeparam>
internal class StorageAccountLeaseResult<T> : LeaseResult<T>
    where T : class
{
    private bool _disposed = false;
    private CancellationTokenSource _renewalCancellation;
    private Task _renewalTask;

    /// <inheritdoc/>
    public override bool HasLease => !string.IsNullOrEmpty(Response?.LeaseId);

    /// <summary>
    /// Stopwatch
    /// </summary>
    private Stopwatch Stopwatch { get; set; } = new Stopwatch();

    /// <summary>
    /// Gets the <see cref="BlobClient"/> instance used to interact with the associated blob.
    /// </summary>
    internal BlobClient BlobClient { get; init; }

    /// <summary>
    /// Gets the <see cref="BlobLeaseClient"/> instance used to manage the lease for the associated blob.
    /// </summary>
    internal BlobLeaseClient LeaseClient { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="BlobLease"/> representing the lease on the associated blob.
    /// </summary>
    internal BlobLease Response { get; set; }

    /// <summary>
    /// Gets the date and time when the lease was acquired.
    /// </summary>
    internal DateTime Acquired { get; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the <see cref="IAltinnLease"/> implementation associated with this lease result.
    /// </summary>
    internal IAltinnLease Implementation { get; init; }

    /// <summary>
    /// Starts automatic lease renewal in the background.
    /// </summary>
    internal void DispachLeaseRefresher()
    {
        if (HasLease && _renewalTask == null)
        {
            _renewalCancellation = new CancellationTokenSource();
            _renewalTask = LeaseRefresher(_renewalCancellation.Token);
        }
    }

    /// <summary>
    /// Background renewal loop that runs every 30 seconds (half the lease duration).
    /// </summary>
    private async Task LeaseRefresher(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && HasLease)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                if (!cancellationToken.IsCancellationRequested && HasLease)
                {
                    await Implementation.RefreshLease(this, cancellationToken);
                }
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            Stopwatch.Stop();
            LeaseTelemetry.RecordLeaseDuration(LeaseName, Stopwatch.Elapsed);
        }
    }

    /// <summary>
    /// Releases the lease synchronously when the object is disposed.
    /// </summary>
    public override void Dispose()
    {
        if (!_disposed)
        {
            _renewalCancellation?.Cancel();
            try
            {
                _renewalTask?.Wait(TimeSpan.FromSeconds(5));
            }
            catch (AggregateException)
            {
            }

            try
            {
                Implementation.Release(this, default).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch
            {
            }

            _renewalCancellation?.Dispose();
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
            _renewalCancellation?.Cancel();
            if (_renewalTask is { })
            {
                try
                {
                    await _renewalTask.WaitAsync(TimeSpan.FromSeconds(5));
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

            _renewalCancellation?.Dispose();
        }

        _disposed = true;
    }
}
