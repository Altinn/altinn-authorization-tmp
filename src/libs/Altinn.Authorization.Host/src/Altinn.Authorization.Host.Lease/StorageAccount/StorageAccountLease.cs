using System.Text;
using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Client;

namespace Altinn.Authorization.Host.Lease.StorageAccount;

/// <summary>
/// Storage Account Lease
/// </summary>
/// <param name="factory">storage account factory</param>
public class StorageAccountLease(IAzureClientFactory<BlobServiceClient> factory) : IAltinnLease
{
    /// <summary>
    /// factory
    /// </summary>
    public IAzureClientFactory<BlobServiceClient> Factory { get; } = factory;

    /// <inheritdoc/>
    public async Task<LeaseResult<T>> TryAquireNonBlocking<T>(string leaseName, CancellationToken cancellationToken = default)
        where T : class
    {
        var client = CreateClient(leaseName);
        if (!await client.ExistsAsync(cancellationToken))
        {
            await client.UploadAsync(new MemoryStream(Encoding.UTF8.GetBytes("{ }")), cancellationToken);
        }

        var leaseClient = client.GetBlobLeaseClient();
        try
        {
            var lease = await leaseClient.AcquireAsync(TimeSpan.FromMinutes(1), default, cancellationToken);
            var stream = await DownloadStreamAsync(client, cancellationToken);
            return new()
            {
                BlobClient = client,
                Data = JsonSerializer.Deserialize<T>(stream),
                Response = lease.Value,
                LeaseClient = leaseClient,
            };
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseAlreadyPresent")
        {
            return new LeaseResult<T>()
            {
                BlobClient = client,
                Data = default,
                Response = default,
                LeaseClient = leaseClient,
            };
        }
    }

    /// <summary>
    /// Add data
    /// </summary>
    public async Task<LeaseResult<T>> Put<T>(LeaseResult<T> lease, T data, CancellationToken cancellationToken = default)
        where T : class
    {
        if (lease.HasLease)
        {
            var content = JsonSerializer.Serialize(data);
            await lease.BlobClient.UploadAsync(
            content,
            new BlobUploadOptions()
            {
                Conditions = new()
                {
                    LeaseId = lease.Response.LeaseId,
                },
            },
            cancellationToken);

            return new()
            {
                BlobClient = lease.BlobClient,
                Data = data,
                LeaseClient = lease.LeaseClient,
                Response = lease.Response,
            };
        }

        return lease;
    }

    /// <inheritdoc/>
    public async Task Release<T>(LeaseResult<T> lease, CancellationToken cancellationToken = default)
          where T : class
    {
        if (lease.HasLease)
        {
            try
            {
                await lease.BlobClient.GetBlobLeaseClient(lease.Response.LeaseId).ReleaseAsync(default, cancellationToken);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseNotPresentWithBlobOperation" || ex.ErrorCode == "LeaseLost" || ex.ErrorCode == "LeaseIdMissing")
            {
                return;
            }
        }
    }

    private static async Task<Stream> DownloadStreamAsync(BlobClient client, CancellationToken cancellationToken = default)
    {
        var result = new MemoryStream();
        if (await client.ExistsAsync(cancellationToken))
        {
            await client.DownloadToAsync(result, cancellationToken);
            result.Position = 0;
            return result;
        }

        return result;
    }

    public Task<LeaseResult<T>> RefreshLease<T>(LeaseResult<T> lease, CancellationToken cancellationToken = default)
        where T : class
    {
        if (lease.HasLease)
        {
            var expires = lease.Acquired.AddSeconds(Convert.ToDouble(lease.Response.LeaseTime));
        }

        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a blob client
    /// </summary>
    private BlobClient CreateClient(string blobName) =>
        Factory.CreateClient(AltinnLeaseOptions.StorageAccountLease.Name).GetBlobContainerClient(AltinnLeaseOptions.StorageAccountLease.Container).GetBlobClient(blobName);
}
