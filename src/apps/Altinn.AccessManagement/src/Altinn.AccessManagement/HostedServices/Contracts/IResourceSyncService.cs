using Altinn.Authorization.AccessManagement;
using Altinn.Authorization.Host.Lease;
using static Altinn.Authorization.AccessManagement.RegisterHostedService;

namespace Altinn.AccessManagement.HostedServices.Contracts;

/// <summary>
/// Service for syncronizing resources
/// </summary>
public interface IResourceSyncService
{
    /// <summary>
    /// Sync resource owners (ServiceOwner)
    /// </summary>
    Task<bool> SyncResourceOwners(CancellationToken cancellationToken);
    
    /// <summary>
    /// Sync resources
    /// </summary>
    Task SyncResources(LeaseResult<LeaseContent> ls, CancellationToken cancellationToken);
}
