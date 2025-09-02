namespace Altinn.Authorization.Host.Lease;

/// <summary>
/// Represents the result of a lease operation, encapsulating the leased data and its status.
/// This abstract class ensures proper resource management through both synchronous and asynchronous disposal.
/// </summary>
public interface IAltinnLeaseResult : IDisposable, IAsyncDisposable
{
}
