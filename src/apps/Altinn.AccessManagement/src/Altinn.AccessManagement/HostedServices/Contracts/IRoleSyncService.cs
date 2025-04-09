using Altinn.Authorization.Host.Lease;
using static Altinn.Authorization.AccessManagement.RegisterHostedService;

namespace Altinn.AccessManagement.HostedServices.Contracts;

/// <summary>
/// Service for syncronizing roles
/// </summary>
public interface IRoleSyncService
{
    /// <summary>
    /// Sync roles
    /// </summary>
    Task SyncRoles(LeaseResult<LeaseContent> ls, CancellationToken cancellationToken);
}
