using Altinn.Authorization.AccessManagement;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.AltinnRole;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.Win32;
using System.Net;

namespace Altinn.AccessManagement.HostedServices
{
    /// <summary>
    /// Hosted service for synchronizing Altinn roles.
    /// </summary>
    public partial class AltinnRoleHostedServices(
        IAltinnLease lease,
        IAltinnRole role,
        IFeatureManager featureManager,
        ILogger<AltinnRoleHostedServices> logger) : IHostedService, IDisposable
    {
        private readonly IAltinnLease _lease = lease;
        private readonly ILogger<AltinnRoleHostedServices> _logger = logger;
        private readonly IFeatureManager _featureManager = featureManager;
        private readonly IAltinnRole _role = role;

        private int _executionCount = 0;
        private Timer _timer = null;
        private readonly CancellationTokenSource _stop = new();

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Log.StartAltinnRoleSync(_logger);

            _timer = new Timer(async state => await SyncAltinnRoleDispatcher(state), _stop.Token, TimeSpan.Zero, TimeSpan.FromMinutes(2));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Dispatches the register synchronization process in a separate task.
        /// </summary>
        /// <param name="state">Cancellation token for stopping execution.</param>
        private async Task SyncAltinnRoleDispatcher(object state)
        {
            var cancellationToken = (CancellationToken)state;

            await using var ls = await _lease.TryAquireNonBlocking<LeaseContent>("access_management_altinnrole_sync", cancellationToken);
            if (!ls.HasLease || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                if (await _featureManager.IsEnabledAsync(AccessManagementFeatureFlags.HostedServicesAltinnRoleSync))
                {
                    //await PrepareSync(); // do db setup
                    await SyncAllRoles(ls, cancellationToken);

                }
            }
            catch (Exception ex)
            {
                Log.SyncError(_logger, ex);
                return;
            }
            finally
            {
                await _lease.Release(ls, default);
            }
        }

        /// <summary>
        /// Synchronizes altinn role data by first acquiring a remote lease and streaming altinn role entries.
        /// Returns if lease is already taken.
        /// </summary>
        /// <param name="ls">The lease result containing the lease data and status.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        private async Task SyncAllRoles(LeaseResult<LeaseContent> ls, CancellationToken cancellationToken)
        {
            var test = await _role.StreamRoles("3", ls.Data?.AltinnRoleStreamNextPageLink, cancellationToken);
            
            await foreach (var page in test)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (!page.IsSuccessful)
                {
                    Log.ResponseError(_logger, page.StatusCode);
                }

                foreach (var item in page.Content.Data)
                {
                    // TODO: one for party, one for role
                    Interlocked.Increment(ref _executionCount);
                    //Log.Role(_logger, item.FromParty, item.ToParty, item.RoleIdentifier);
                    //await WriteRolesToDb(item);
                }

                if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
                {
                    return;
                }

                await _lease.Put(ls, new() { AltinnRoleStreamNextPageLink = page.Content.Links.Next }, cancellationToken);
                await _lease.RefreshLease(ls, cancellationToken);
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
                _timer?.Change(Timeout.Infinite, 0);
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
            }
        }

        private static partial class Log
        {
            [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Error occured while fetching data from sbl bridge, got {statusCode}")]
            internal static partial void ResponseError(ILogger logger, HttpStatusCode statusCode);

            [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Starting AltinnRole hosted service")]
            internal static partial void StartAltinnRoleSync(ILogger logger);

            [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "An error occured while streaming data from sbl bridge")]
            internal static partial void SyncError(ILogger logger, Exception ex);

            [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Quit AltinnRole hosted service")]
            internal static partial void QuitAltinnRoleSync(ILogger logger);
        }

        /// <summary>
        /// Represents lease content, including pagination link.
        /// </summary>
        public class LeaseContent()
        {
            /// <summary>
            /// The URL of the next page of Party data.
            /// </summary>
            public string AltinnRoleStreamNextPageLink { get; set; }            
        }
    }
}
