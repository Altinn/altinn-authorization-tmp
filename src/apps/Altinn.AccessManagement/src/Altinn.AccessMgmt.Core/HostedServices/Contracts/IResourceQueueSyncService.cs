using Altinn.Authorization.Host.Lease;

namespace Altinn.AccessMgmt.Core.HostedServices.Contracts
{
    /// <summary>
    /// Service for synchronizing resources based on entries in the resource queue.
    /// </summary>
    public interface IResourceQueueSyncService
    {
        /// <summary>
        /// Synchronizes resources from the resource registry into the Access Management database.
        /// </summary>
        Task SyncResources(ILease lease, CancellationToken cancellationToken);
    }
}
