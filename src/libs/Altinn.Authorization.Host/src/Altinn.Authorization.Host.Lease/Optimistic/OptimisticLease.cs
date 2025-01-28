using System.Collections.Concurrent;

namespace Altinn.Authorization.Host.Lease.Memory;

/// <inheritdoc/>
public class OptimisticLease : IAltinnLease
{
    private static ConcurrentDictionary<string, object> _state = new();

    /// <inheritdoc/>
    public Task<LeaseResult<T>> Put<T>(LeaseResult<T> lease, T data, CancellationToken cancellationToken = default)
        where T : class
    {
        if (lease.HasLease)
        {
            _state[lease.LeaseName] = data;
        }

        return Task.FromResult(new LeaseResult<T>()
        {
            LeaseName = lease.LeaseName,
            Data = data,
        });
    }

    /// <inheritdoc/>
    public Task<LeaseResult<T>> RefreshLease<T>(LeaseResult<T> lease, CancellationToken cancellationToken = default)
        where T : class
    {
        return Task.FromResult(lease);
    }

    /// <inheritdoc/>
    public Task Release<T>(LeaseResult<T> lease, CancellationToken cancellationToken = default)
        where T : class
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<LeaseResult<T>> TryAquireNonBlocking<T>(string leaseName, CancellationToken cancellationToken = default)
        where T : class
    {
        if (_state.TryGetValue(leaseName, out var value))
        {
            return Task.FromResult(new LeaseResult<T>()
            {
                Data = (T)_state[leaseName],
                LeaseName = leaseName,
            });
        }

        return Task.FromResult(new LeaseResult<T>()
        {
            Data = default,
            LeaseName = leaseName,
        });
    }
}
