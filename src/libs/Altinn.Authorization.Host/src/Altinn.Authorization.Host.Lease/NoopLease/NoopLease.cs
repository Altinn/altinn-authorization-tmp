using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.Host.Lease.Noop;

/// <summary>
/// In-memory implementation of <see cref="IAltinnLease"/>.
/// Provides a thread-safe mechanism for leasing and storing temporary data.
/// There are no cleanup of the keys, creating indefinite keys will result in 
/// memory leak.
/// </summary>
[ExcludeFromCodeCoverage]
public class NoopLease : IAltinnLease
{
    /// <inheritdoc/>
    public Task<T> Get<T>(LeaseResult activeLease, CancellationToken cancellationToken = default)
        where T : class, new()
    {
        return Task.FromResult(default(T));
    }

    /// <inheritdoc/>
    public CancellationToken LinkTokens(LeaseResult activeLease, params CancellationToken[] cancellationTokens)
    {
        return CancellationTokenSource.CreateLinkedTokenSource(cancellationTokens).Token;
    }

    /// <inheritdoc/>
    public Task<LeaseResult> TryAcquireNonBlocking<T>(string leaseName, CancellationToken cancellationToken = default)
    {
        return default;
    }

    /// <inheritdoc/>
    public Task Update<T>(LeaseResult activeLease, T data, CancellationToken cancellationToken = default)
        where T : class, new()
    {
        return default;
    }

    /// <inheritdoc/>
    public Task Update<T>(LeaseResult activeLease, Action<T> data, CancellationToken cancellationToken = default)
        where T : class, new()
    {
        return default;
    }
}
