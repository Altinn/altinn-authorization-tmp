using Altinn.Authorization.Host.Lease;

namespace Altinn.AccessMgmt.Core.HostedServices.Contracts
{
    /// <summary>
    /// Service for syncronizing altinn roles
    /// </summary>
    public interface IResourceQueueSyncService
    {
        /// <summary>
        /// Sync roles
        /// </summary>
        Task SyncResources(ILease lease, CancellationToken cancellationToken);
    }
}
