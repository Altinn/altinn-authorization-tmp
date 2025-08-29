using System.Diagnostics;
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
    private static TimeSpan MaxLeaseTime { get; } = TimeSpan.FromSeconds(60);

    /// <inheritdoc/>
    public async Task<LeaseResult<T>> TryAcquireNonBlocking<T>(string leaseName, CancellationToken cancellationToken = default)
        where T : class, new()
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

            var content = await client.DownloadContentAsync(cancellationToken);
            var data = content.Value.Content.ToObjectFromJson<T>();

            var leaseResult = new StorageAccountLeaseResult<T>()
            {
                LeaseName = leaseName,
                BlobClient = client,
                LeaseClient = leaseClient,
                Data = data,
                Response = lease.Value,
                Implementation = this,
            };

            leaseResult.DispachLeaseRefresher();

            return leaseResult;
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
        {
            var content = await client.DownloadContentAsync(cancellationToken);
            var data = content.Value.Content.ToObjectFromJson<T>();

            return new StorageAccountLeaseResult<T>()
            {
                LeaseName = leaseName,
                BlobClient = client,
                LeaseClient = leaseClient,
                Data = data,
                Response = default,
                Implementation = this,
            };
        }
    }

    /// <inheritdoc/>
    public async Task<LeaseResult<T>> Put<T>(LeaseResult<T> lease, T data, CancellationToken cancellationToken = default)
        where T : class
    {
        if (lease.HasLease && lease is StorageAccountLeaseResult<T> castedLease)
        {
            try
            {
                var content = JsonSerializer.Serialize(data);
                var options = new BlobUploadOptions()
                {
                    Conditions = new()
                    {
                        LeaseId = castedLease.Response.LeaseId,
                    },
                };

                await LeaseTelemetry.RecordLeasePut(Logger, lease.LeaseName, async () =>
                {
                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
                    return await castedLease.BlobClient.UploadAsync(stream, options, cancellationToken);
                });

                lease.Data = data;
                return lease;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.LeaseLost || ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
            {
                castedLease.Response = null;
                return castedLease;
            }
        }

        return lease;
    }

    /// <inheritdoc/>
    public async Task Release<T>(LeaseResult<T> lease, CancellationToken cancellationToken = default)
          where T : class
    {
        if (lease.HasLease && lease is StorageAccountLeaseResult<T> castedLease)
        {
            try
            {
                var blobClient = castedLease.BlobClient.GetBlobLeaseClient(castedLease.Response.LeaseId);
                var result = await LeaseTelemetry.RecordReleaseLease(Logger, lease.LeaseName, async () => await blobClient.ReleaseAsync(default, cancellationToken));
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.LeaseLost || ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
            {
            }
            finally
            {
                castedLease.Response = null;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<LeaseResult<T>> RefreshLease<T>(LeaseResult<T> lease, CancellationToken cancellationToken = default)
        where T : class
    {
        if (lease.HasLease && lease is StorageAccountLeaseResult<T> castedLease)
        {
            var leaseExpirationTime = castedLease.Acquired.AddSeconds(Convert.ToDouble(castedLease.Response.LeaseTime));
            if (leaseExpirationTime < DateTime.UtcNow.AddSeconds(20))
            {
                try
                {
                    var result = await LeaseTelemetry.RecordRefreshLease(Logger, lease.LeaseName, async () => await castedLease.LeaseClient.RenewAsync(default, cancellationToken));
                    castedLease.Response = result;
                    return castedLease;
                }
                catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.LeaseLost || ex.ErrorCode == BlobErrorCode.LeaseAlreadyPresent)
                {
                    castedLease.Response = null;
                    return castedLease;
                }
            }
        }

        return lease;
    }

    private async Task CreateEmptyFileIfNotExists<T>(BlobClient client, CancellationToken cancellationToken)
        where T : class, new()
    {
        try
        {
            var content = JsonSerializer.Serialize(new T());
            if (!await client.ExistsAsync(cancellationToken))
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                using var stream = new MemoryStream(bytes);

                await client.UploadAsync(stream, cancellationToken);
            }
        }
        catch (RequestFailedException)
        {
            throw;
        }
    }

    /// <summary>
    /// Creates a blob client
    /// </summary>
    private BlobClient CreateClient(string blobName) =>
        Factory.CreateClient(AltinnLeaseOptions.StorageAccountLease.Name).GetBlobContainerClient(AltinnLeaseOptions.StorageAccountLease.Container).GetBlobClient(blobName);
}
