namespace Altinn.Authorization.Host.Lease.InMemory;

/// <summary>
/// Represents the result of an in-memory lease operation.
/// This class ensures that leases are properly released when disposed.
/// </summary>
/// <typeparam name="T">The type of data being leased.</typeparam>
public class InMemoryResult<T> : LeaseResult<T>
    where T : class
{
    private bool _disposed;

    /// <summary>
    /// Gets a value indicating whether the lease is currently held.
    /// </summary>
    public override bool HasLease => SetLease;

    /// <summary>
    /// Indicates whether the lease is set.
    /// </summary>
    internal bool SetLease { set; get; }

    /// <summary>
    /// The lease implementation that manages the lease lifecycle.
    /// </summary>
    internal IAltinnLease Implementation { get; init; }

    /// <summary>
    /// Releases the lease synchronously when the object is disposed.
    /// </summary>
    public override void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    /// Releases the lease asynchronously when the object is disposed.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public override async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
    }

    /// <summary>
    /// Synchronous disposal logic to release the lease.
    /// </summary>
    /// <param name="disposing">Indicates whether the method was called directly or by the garbage collector.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Implementation.Release(this, default).GetAwaiter().GetResult();
        }

        _disposed = true;
    }

    /// <summary>
    /// Asynchronous disposal logic to release the lease.
    /// </summary>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_disposed)
        {
            await Implementation.Release(this, default);
            _disposed = true;
        }
    }
}
