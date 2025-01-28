using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace Altinn.Authorization.Host.Lease;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class LeaseResult<T> : IDisposable, IAsyncDisposable
{
    internal LeaseResult()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    public T Data { get; init; }

    /// <summary>
    /// 
    /// </summary>
    public bool HasLease => string.IsNullOrEmpty(Response?.LeaseId);

    internal string LeaseName { get; init; }

    internal BlobClient BlobClient { get; init; }

    internal BlobLeaseClient LeaseClient { get; init; }

    internal BlobLease Response { get; init; }

    internal DateTime Acquired { get; } = DateTime.Now;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
