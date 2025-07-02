using Altinn.Authorization.AccessManagement.HostedServices;
using Altinn.Authorization.Host.Lease;

namespace Altinn.AccessManagement.HostedServices.Contracts
{
    /// <summary>
    /// Service for syncronizing altinn roles
    /// </summary>
    public interface IAllAltinnRoleSyncService
    {
        /// <summary>
        /// Sync roles
        /// </summary>
        Task SyncAllAltinnRoles(LeaseResult<AllAltinnRoleLease> ls, CancellationToken cancellationToken);
    }
}
