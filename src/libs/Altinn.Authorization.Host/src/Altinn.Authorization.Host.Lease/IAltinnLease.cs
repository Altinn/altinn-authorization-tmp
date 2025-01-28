namespace Altinn.Authorization.Host.Lease;

/// <summary>
/// a
/// </summary>
public interface IAltinnLease
{
    /// <summary>
    /// Try Acquire lease 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<LeaseResult<T>> TryAquireNonBlocking<T>(string leaseName, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Add data
    /// </summary>
    Task<LeaseResult<T>> Put<T>(LeaseResult<T> lease, T data, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Releases lease
    /// </summary>
    /// <typeparam name="T"></typeparam>
    Task Release<T>(LeaseResult<T> lease, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// asd
    /// </summary>
    Task<LeaseResult<T>> RefreshLease<T>(LeaseResult<T> lease, CancellationToken cancellationToken = default)
        where T : class;
}
