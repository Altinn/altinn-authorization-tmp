using System.Net;
using Altinn.AccessManagement.HostedServices.Contracts;
using Altinn.AccessManagement.HostedServices.Services;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.Authorization.AccessManagement.HostedServices;
using Altinn.Authorization.Host.Lease;
using Microsoft.FeatureManagement;

namespace Altinn.AccessManagement.HostedServices
{
    /// <summary>
    /// Hosted service for synchronizing Altinn roles.
    /// </summary>
    public partial class AltinnRoleHostedService(
        IAltinnLease lease,
        IFeatureManager featureManager,
        IStatusService statusService,
        ILogger<AltinnRoleHostedService> logger,
        IAllAltinnRoleSyncService allAltinnRoleSyncService) : IHostedService, IDisposable
    {
        private readonly IAltinnLease _lease = lease;
        private readonly ILogger<AltinnRoleHostedService> _logger = logger;
        private readonly IFeatureManager _featureManager = featureManager;
        private readonly IStatusService statusService = statusService;
        private readonly IAllAltinnRoleSyncService allAltinnRoleSyncService = allAltinnRoleSyncService;
        
        private Timer _timerAltinnRoles = null;
        private readonly CancellationTokenSource _stop = new();

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.StartAltinnRoleSync(_logger);

            _timerAltinnRoles = new Timer(async state => await SyncAllAltinnRoleDispatcher(state), _stop.Token, TimeSpan.Zero, TimeSpan.FromMinutes(2));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Dispatches the Altinn roles synchronization process in a separate task.
        /// </summary>
        /// <param name="state">Cancellation token for stopping execution.</param>
        private async Task SyncAllAltinnRoleDispatcher(object state)
        {
            var cancellationToken = (CancellationToken)state;
            try
            {
                var options = new ChangeRequestOptions()
                {
                    ChangedBy = AuditDefaults.Altinn2RoleImportSystem,
                    ChangedBySystem = AuditDefaults.Altinn2RoleImportSystem
                };

                if (await _featureManager.IsEnabledAsync(AccessManagementFeatureFlags.HostedServicesAllAltinnRoleSync))
                {
                    await using var ls = await _lease.TryAquireNonBlocking<AllAltinnRoleLease>("access_management_allaltinnrole_sync", cancellationToken);
                    if (!ls.HasLease || cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    await SyncAllAltinnRoles(ls, options, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Log.SyncError(_logger, ex);
            }            
        }

        private async Task SyncAllAltinnRoles(LeaseResult<AllAltinnRoleLease> ls, ChangeRequestOptions options, CancellationToken cancellationToken)
        {
            var allAltinnRoleStatus = await statusService.GetOrCreateRecord(Guid.Parse("8B05CEB5-43D4-4FF1-A831-4790F5792B93"), "accessmgmt-sync-sblbridge-altinnrole", options, 5);
            var canRunAllAltinnRoleSync = await statusService.TryToRun(allAltinnRoleStatus, options);

            try
            {
                if (canRunAllAltinnRoleSync)
                {
                    await allAltinnRoleSyncService.SyncAllAltinnRoles(ls, cancellationToken);
                    await statusService.RunSuccess(allAltinnRoleStatus, options);
                }
            }
            catch (Exception ex)
            {
                Log.SyncError(_logger, ex);
                await statusService.RunFailed(allAltinnRoleStatus, ex, options);
            }
            
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                Log.QuitAltinnRoleSync(_logger);
            }
            finally
            {
                _timerAltinnRoles?.Change(Timeout.Infinite, 0);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timerAltinnRoles?.Dispose();
                _stop?.Cancel();
                _stop?.Dispose();
            }
        }
    }
}
