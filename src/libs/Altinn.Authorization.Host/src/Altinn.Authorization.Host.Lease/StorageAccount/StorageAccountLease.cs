using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Altinn.Authorization.Host.Lease.Telemetry;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.Host.Lease.StorageAccount;

/// <summary>
/// Represents a lease management implementation for a storage account.
/// Provides functionality for acquiring, releasing, and refreshing leases on blobs in a storage account.
/// </summary>
/// <param name="Factory">The factory used to create <see cref="BlobServiceClient"/> instances.</param>
/// <param name="Logger">The logger used to log events related to lease operations.</param>
/// <remarks>
/// https://learn.microsoft.com/en-us/rest/api/storageservices/blob-service-error-codes
/// </remarks>
public partial class StorageAccountLease(ILogger<StorageAccountLease> Logger, IAzureClientFactory<BlobServiceClient> Factory) : IAltinnLease
{
    public static TimeSpan MaxLeaseTime { get; } = TimeSpan.FromSeconds(60);

    [DoesNotReturn]
    public void Unreachable() => throw new UnreachableException();

    /// <inheritdoc/>
    public async Task<LeaseResult> TryAcquireNonBlocking<T>(string leaseName, CancellationToken cancellationToken = default)
    {
        var client = CreateClient(leaseName);
        await CreateEmptyFileIfNotExists<T>(client, cancellationToken);
        var leaseClient = client.GetBlobLeaseClient();

        try
        {
            var lease = await LeaseTelemetry.RecordLeaseAcquire(
                Logger,
                leaseName,
                async () => await leaseClient.AcquireAsync(MaxLeaseTime, default, cancellationToken)
            );

            return new StorageAccountLeaseResult(
                client,
                leaseClient,
                lease,
                this
            );
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
        {
            return new StorageAccountLeaseResult(
                client,
                leaseClient,
                null,
                this
            );
        }
    }

    /// <inheritdoc/>
    public async Task<T> Get<T>(LeaseResult activeLease, CancellationToken cancellationToken = default)
        where T : class, new()
    {
        var lease = AssertLeaseType(activeLease);
        lease.RwLock.EnterReadLock();
        try
        {
            var client = CreateClient(lease.BlobClient.Name);
            var content = await client.DownloadContentAsync(cancellationToken);
            return content.Value.Content.ToObjectFromJson<T>();
        }
        finally
        {
            lease.RwLock.ExitReadLock();
        }
    }

    /// <inheritdoc/>
    public async Task Update<T>(LeaseResult activeLease, Action<T> data, CancellationToken cancellationToken = default)
        where T : class, new()
    {
        if (data is { })
        {
            var content = new T();
            await Update(activeLease, data, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task Update<T>(LeaseResult activeLease, T data, CancellationToken cancellationToken = default)
        where T : class, new()
    {
        var lease = AssertLeaseType(activeLease);
        lease.RwLock.EnterWriteLock();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (lease.BlobLease is null)
            {
                Unreachable();
            }

            var content = JsonSerializer.Serialize(data);
            var options = new BlobUploadOptions()
            {
                Conditions = new()
                {
                    LeaseId = lease.BlobLease.LeaseId,
                },
            };

            await LeaseTelemetry.RecordLeasePut(Logger, lease.BlobClient.Name, async () =>
            {
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                return await lease.BlobClient.UploadAsync(stream, options, cancellationToken);
            });
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.LeaseLost || ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
        {
            lease.Cancel();
        }
        finally
        {
            lease.RwLock.ExitWriteLock();
        }
    }

    public CancellationToken LinkTokens(LeaseResult activeLease, params CancellationToken[] cancellationTokens)
    {
        var lease = AssertLeaseType(activeLease);
        return lease.LinkTokens(cancellationTokens);
    }

    internal async Task Release(LeaseResult activeLease, CancellationToken cancellationToken = default)
    {
        var lease = AssertLeaseType(activeLease);
        lease.RwLock.EnterWriteLock();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (lease.BlobLease is null)
            {
                Unreachable();
            }

            var blobClient = lease.BlobClient.GetBlobLeaseClient(lease.BlobLease.LeaseId);
            var result = await LeaseTelemetry.RecordReleaseLease(Logger, lease.BlobClient.Name, async () => await blobClient.ReleaseAsync(default, cancellationToken));
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.LeaseLost || ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
        {
            lease.Cancel();
        }
        finally
        {
            lease.RwLock.ExitWriteLock();
        }
    }

    internal async Task RefreshLease(LeaseResult activeLease, CancellationToken cancellationToken = default)
    {
        var lease = AssertLeaseType(activeLease);
        lease.RwLock.EnterWriteLock();
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (lease.BlobLease is null)
            {
                Unreachable();
            }

            var result = await LeaseTelemetry.RecordRefreshLease(Logger, lease.BlobClient.Name, async () => await lease.BlobLeaseClient.RenewAsync(default, cancellationToken));
            lease.BlobLease = result;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.LeaseLost || ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
        {
            lease.Cancel();
        }
        finally
        {
            lease.Cancel();
            lease.RwLock.ExitWriteLock();
        }
    }

    internal static StorageAccountLeaseResult AssertLeaseType(LeaseResult lease)
    {
        if (lease is not StorageAccountLeaseResult leaseResult)
        {
            throw new ArgumentException("Underlying implementation and lease is not of same type", nameof(lease));
        }

        return leaseResult;
    }

    internal async Task CreateEmptyFileIfNotExists<T>(BlobClient client, CancellationToken cancellationToken)
    {
        try
        {
            if (!await client.ExistsAsync(cancellationToken))
            {
                var bytes = Encoding.UTF8.GetBytes("{}");
                using var stream = new MemoryStream(bytes);
                await client.UploadAsync(stream, cancellationToken);
            }
        }
        catch (RequestFailedException)
        {
            throw;
        }
    }

    internal BlobClient CreateClient(string blobName) =>
        Factory.CreateClient(AltinnLeaseOptions.StorageAccountLease.Name).GetBlobContainerClient(AltinnLeaseOptions.StorageAccountLease.Container).GetBlobClient(blobName);
}
