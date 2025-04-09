using System.Net;
using Altinn.AccessManagement;
using Altinn.AccessManagement.HostedServices.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.Authorization.Host.Lease;
using Microsoft.FeatureManagement;

namespace Altinn.Authorization.AccessManagement;

/// <summary>
/// A hosted service responsible for synchronizing register data using leases.
/// </summary>
/// <param name="lease">Lease provider for distributed locking.</param>
/// <param name="logger">Logger for logging service activities.</param>
/// <param name="featureManager">for reading feature flags</param>
/// <param name="statusService">Status service</param>
/// <param name="resourceSyncService">Service for syncing resources</param>
/// <param name="partySyncService">Service for syncing parties</param>
/// <param name="roleSyncService">Service for syncing roles</param>
public partial class RegisterHostedService(
    IAltinnLease lease,
    ILogger<RegisterHostedService> logger,
    IFeatureManager featureManager,
    IStatusService statusService,
    IResourceSyncService resourceSyncService,
    IPartySyncService partySyncService,
    IRoleSyncService roleSyncService
    ) : IHostedService, IDisposable
{
    private readonly IAltinnLease _lease = lease;
    private readonly ILogger<RegisterHostedService> _logger = logger;
    private readonly IFeatureManager _featureManager = featureManager;
    private readonly IStatusService statusService = statusService;
    private readonly IResourceSyncService resourceSyncService = resourceSyncService;
    private readonly IPartySyncService partySyncService = partySyncService;
    private readonly IRoleSyncService roleSyncService = roleSyncService;
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
        if (!await _featureManager.IsEnabledAsync(AccessManagementFeatureFlags.HostedServicesRegisterSync))
        {
            return;
        }

        var cancellationToken = (CancellationToken)state;
        await using var ls = await _lease.TryAquireNonBlocking<LeaseContent>("access_management_register_sync", cancellationToken);
        if (!ls.HasLease || cancellationToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            var options = new ChangeRequestOptions()
            {
                ChangedBy = AuditDefaults.RegisterImportSystem,
                ChangedBySystem = AuditDefaults.RegisterImportSystem
            };

            var partyStatus = await statusService.GetOrCreateRecord(Guid.Parse("C18B67F6-B07E-482C-AB11-7FE12CD1F48D"), "accessmgmt-sync-register-party", options, 5);
            var roleStatus = await statusService.GetOrCreateRecord(Guid.Parse("84E9726D-E61B-4DFF-91D7-9E17C8BB41A6"), "accessmgmt-sync-register-role", options, 5);
            var resourceStatus = await statusService.GetOrCreateRecord(Guid.Parse("BEF7E6C8-2928-423E-9927-225488A5B08B"), "accessmgmt-sync-register-resource", options, 5);

            bool canRunPartySync = await statusService.TryToRun(partyStatus, options);
            bool canRunRoleSync = await statusService.TryToRun(roleStatus, options);
            bool canRunResourceSync = await statusService.TryToRun(resourceStatus, options);

            if (!canRunPartySync && !canRunRoleSync && !canRunResourceSync)
            {
                return;
            }

            try
            {
                if (canRunResourceSync)
                {
                    await resourceSyncService.SyncResourceOwners(cancellationToken);
                    await resourceSyncService.SyncResources(ls, cancellationToken);
                    await statusService.RunSuccess(resourceStatus, options);
                }
            }
            catch (Exception ex)
            {
                Log.SyncError(_logger, ex);
                await statusService.RunFailed(resourceStatus, ex, options);
            }

            try
            {
                if (canRunPartySync)
                {
                    await partySyncService.SyncParty(ls, cancellationToken);
                    await statusService.RunSuccess(partyStatus, options);
                }
            }
            catch (Exception ex)
            {
                Log.SyncError(_logger, ex);
                await statusService.RunFailed(partyStatus, ex, options);
            }

            try
            {
                if (canRunPartySync)
                {
                    await roleSyncService.SyncRoles(ls, cancellationToken);
                    await statusService.RunSuccess(roleStatus, options);
                }
            }
            catch (Exception ex)
            {
                Log.SyncError(_logger, ex);
                await statusService.RunFailed(partyStatus, ex, options);
            }

            _logger.LogInformation("Register sync completed!");
        }
        catch (Exception ex)
        {
            Log.SyncError(_logger, ex);
        }
        finally
        {
            await _lease.Release(ls, default);
        }
    }

    #region Base

    private async Task UpdateLease(LeaseResult<LeaseContent> ls, Action<LeaseContent> configureLeaseContent, CancellationToken cancellationToken)
    {
        configureLeaseContent(ls.Data);
        await _lease.Put(ls, ls.Data, cancellationToken);
        await _lease.RefreshLease(ls, cancellationToken);
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

    /// <summary>
    /// Represents lease content, including pagination link.
    /// </summary>
    public class LeaseContent()
    {
        /// <summary>
        /// The URL of the next page of Party data.
        /// </summary>
        public string PartyStreamNextPageLink { get; set; }

        /// <summary>
        /// The URL of the next page of AssignmentSuccess data.
        /// </summary>
        public string RoleStreamNextPageLink { get; set; }

        /// <summary>
        /// The URL of the next page of updates resourcs.
        /// </summary>
        public string ResourcesNextPageLink { get; set; }
    }

    public static partial class Log
    {
        [LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "Failed to retrieve updated resources from resource register, got {statusCode}")]
        internal static partial void UpdatedResourceError(ILogger logger, HttpStatusCode statusCode);

        [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "Failed to retrieve service owners from resource register, got {statusCode}")]
        internal static partial void ServiceOwnerError(ILogger logger, HttpStatusCode statusCode);

        [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Error occured while fetching data from register, got {statusCode}")]
        internal static partial void ResponseError(ILogger logger, HttpStatusCode statusCode);

        [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Processing party with uuid {partyUuid} from register. RetryCount {count}")]
        internal static partial void Party(ILogger logger, string partyUuid, int count);

        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "An error occured while streaming data from register")]
        internal static partial void SyncError(ILogger logger, Exception ex);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Starting register hosted service")]
        internal static partial void StartRegisterSync(ILogger logger);

        [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Quit register hosted service")]
        internal static partial void QuitRegisterSync(ILogger logger);

        [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Assignment {action} from '{from}' to '{to}' with role '{role}'")]
        internal static partial void AssignmentSuccess(ILogger logger, string action, string from, string to, string role);

        [LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "Failed to {action} assingment from '{from}' to '{to}' with role '{role}'")]
        internal static partial void AssignmentFailed(ILogger logger, string action, string from, string to, string role);
    }

    #endregion
}
