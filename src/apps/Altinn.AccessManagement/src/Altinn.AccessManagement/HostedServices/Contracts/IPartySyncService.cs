using Altinn.Authorization.Host.Lease;
using static Altinn.Authorization.AccessManagement.RegisterHostedService;

namespace Altinn.AccessManagement.HostedServices.Contracts;

/// <summary>
/// Service for syncronizing parties
/// </summary>
public interface IPartySyncService
{
    /// <summary>
    /// Sync parties
    /// </summary>
    Task SyncParty(LeaseResult<LeaseContent> ls, CancellationToken cancellationToken);
}
