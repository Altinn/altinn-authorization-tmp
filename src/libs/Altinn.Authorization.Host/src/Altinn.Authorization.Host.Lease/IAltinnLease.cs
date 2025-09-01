using System.Diagnostics;

namespace Altinn.Authorization.Host.Lease;

/// <summary>
/// Defines the contract for managing leases, including acquiring, storing data, releasing, and refreshing leases.
/// This interface allows operations to be performed on leases in an asynchronous, non-blocking manner.
/// </summary>
public interface IAltinnLease
{
    /// <summary>
    /// Attempts to acquire a lease without blocking.
    /// This method checks for the availability of a lease and returns the result of the acquisition.
    /// </summary>
    /// <typeparam name="T">The type of data being leased.</typeparam>
    /// <param name="leaseName">The name of the lease to acquire.</param>
    /// <param name="cancellationToken">A token that can be used to observe and handle cancellation requests.</param>
    /// <returns>A task that represents the operation, containing the acquired lease.</returns>
    Task<LeaseResult> TryAcquireNonBlocking<T>(string leaseName, CancellationToken cancellationToken = default);

    Task<T> Get<T>(LeaseResult activeLease, CancellationToken cancellationToken = default)
        where T : class, new();

    Task Update<T>(LeaseResult activeLease, T data, CancellationToken cancellationToken = default)
        where T : class, new();

    Task Update<T>(LeaseResult activeLease, Action<T> data, CancellationToken cancellationToken = default)
        where T : class, new();

    CancellationToken LinkTokens(LeaseResult activeLease, params CancellationToken[] cancellationTokens);
}
