using Altinn.AccessManagement;
using Altinn.AccessManagement.HostedServices.Services;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.Authorization.Host.Lease;
using Microsoft.FeatureManagement;

namespace Altinn.Authorization.AccessManagement;

/// <summary>
/// A hosted service responsible for synchronizing register data using leases.
/// </summary>
/// <param name="leaseService">Lease provider for distributed locking.</param>
/// <param name="logger">Logger for logging service activities.</param>
/// <param name="featureManager">for reading feature flags</param>
public partial class RegisterHostedService(
    ILeaseService leaseService,
    ILogger<RegisterHostedService> logger,
    IFeatureManager featureManager
    ) : IHostedService, IDisposable
{
    private readonly ILeaseService _leaseService = leaseService;
    private readonly ILogger<RegisterHostedService> _logger = logger;
    private readonly IFeatureManager _featureManager = featureManager;
    private Timer _timer = null;
    private readonly CancellationTokenSource _stop = new();

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.StartRegisterSync(_logger);

        _timer = new Timer(async state => await SyncRegisterDispatcher(state), _stop.Token, TimeSpan.Zero, TimeSpan.FromMinutes(2));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Dispatches the register synchronization process in a separate task.
    /// </summary>
    /// <param name="state">Cancellation token for stopping execution.</param>
    private async Task SyncRegisterDispatcher(object state)
    {
        var cancellationToken = (CancellationToken)state;
        try
        {
            var options = new ChangeRequestOptions()
            {
                ChangedBy = AuditDefaults.RegisterImportSystem,
                ChangedBySystem = AuditDefaults.RegisterImportSystem
            };

            if (await _featureManager.IsEnabledAsync(AccessManagementFeatureFlags.HostedServicesResourceRegistrySync, cancellationToken))
            {
                await using var lease = await _leaseService.TryAcquireNonBlocking("access_management_resource_registry_sync", cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }

            if (await _featureManager.IsEnabledAsync(AccessManagementFeatureFlags.HostedServicesRegisterSync))
            {
                await using var lease = await _leaseService.TryAcquireNonBlocking("access_management_register_sync", cancellationToken);
            }

            _logger.LogInformation("Register sync completed!");
        }
        catch (Exception ex)
        {
            Log.SyncError(_logger, ex);
        }
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
            _timer?.Dispose();
            _stop?.Cancel();
            _stop?.Dispose();
        }
    }
}
