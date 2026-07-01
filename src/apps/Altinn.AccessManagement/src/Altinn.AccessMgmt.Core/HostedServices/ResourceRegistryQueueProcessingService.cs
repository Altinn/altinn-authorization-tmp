using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.HostedServices.Services;
using Altinn.Authorization.Host.Lease;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Timer = System.Threading.Timer;

namespace Altinn.AccessMgmt.Core.HostedServices
{
    /// <summary>
    /// Hosted service for synchronizing resources from ResourceRegistry.
    /// </summary>
    public partial class ResourceRegistryQueueProcessingService(
        ILeaseService leaseService,
        IFeatureManager featureManager,
        IResourceQueueSyncService resourceQueue,
        ILogger<ResourceRegistryQueueProcessingService> logger) : IHostedService, IDisposable
    {
        private readonly ILogger<ResourceRegistryQueueProcessingService> _logger = logger;
        private readonly ILeaseService _leaseService = leaseService;
        private readonly IFeatureManager _featureManager = featureManager;
        private readonly IResourceQueueSyncService _resourceQueue = resourceQueue;
        private Timer _timer = null;
        private int _isRunning = 0;
        private readonly CancellationTokenSource _stop = new();

        /// <summary>
        /// Dispatches the Altinn roles synchronization process in a separate task.
        /// </summary>
        /// <param name="state">Cancellation token for stopping execution.</param>
        private async Task SyncResourcesDispatcher(object state)
        {
            if (Interlocked.Exchange(ref _isRunning, 1) == 1)
            {
                return;
            }

            try
            {
                var cancellationToken = (CancellationToken)state;
                if (await _featureManager.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesResourceSync))
                {
                    await using var lease = await _leaseService.TryAcquireNonBlocking("access_management_resource_queue_sync", cancellationToken);
                    if (lease is not null && !cancellationToken.IsCancellationRequested)
                    {
                        await SyncResources(lease, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.SyncError(_logger, ex);
            }
            finally
            {
                _isRunning = 0;
            }
        }

        private async Task SyncResources(ILease lease, CancellationToken cancellationToken)
        {
            try
            {
                await _resourceQueue.SyncResources(lease, cancellationToken);
            }
            catch (Exception ex)
            {
                Log.SyncError(_logger, ex);
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.StartRegisterSync(_logger);

            _timer = new Timer(async state => await SyncResourcesDispatcher(state), _stop.Token, TimeSpan.Zero, TimeSpan.FromMinutes(2));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                Log.QuitRegisterSync(_logger);
            }
            finally
            {
                _timer?.Change(Timeout.Infinite, 0);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Dispose();
                _stop?.Cancel();
                _stop?.Dispose();
            }
        }
    }
}
