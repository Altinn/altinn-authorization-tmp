namespace Altinn.Authorization.Host.Lease;

/// <summary>
/// Represents the result of a lease operation, encapsulating the leased data and its status.
/// This abstract class ensures proper resource management through both synchronous and asynchronous disposal.
/// </summary>
public abstract class LeaseResult : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Releases the lease synchronously, if applicable.
    /// This method should be implemented in derived classes to ensure proper resource cleanup.
    /// </summary>
    public abstract void Dispose();

    /// <summary>
    /// Releases the lease asynchronously.
    /// This method should be implemented in derived classes to support proper async cleanup.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous disposal operation.</returns>
    public abstract ValueTask DisposeAsync();
}
