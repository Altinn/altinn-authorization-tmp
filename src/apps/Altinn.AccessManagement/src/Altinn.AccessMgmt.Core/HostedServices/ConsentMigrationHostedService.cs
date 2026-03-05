using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Altinn.Authorization.Host.Lease;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace Altinn.AccessMgmt.Core.HostedServices;

/// <summary>
/// Hosted service for migrating consents from old application to new system
/// </summary>
public partial class ConsentMigrationHostedService : IHostedService, IDisposable
{
    private readonly IConsentMigrationSyncService _syncService;
    private readonly ILeaseService _leaseService;
    private readonly IFeatureManager _featureManager;
    private readonly IOptionsMonitor<ConsentMigrationSettings> _settings;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ConsentMigrationHostedService> _logger;
    private Timer _timer = null;
    private readonly CancellationTokenSource _stop = new();
    private int _isRunning = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsentMigrationHostedService"/> class
    /// </summary>
    public ConsentMigrationHostedService(
        IConsentMigrationSyncService syncService,
        ILeaseService leaseService,
        IFeatureManager featureManager,
        IOptionsMonitor<ConsentMigrationSettings> settings,
        TimeProvider timeProvider,
        ILogger<ConsentMigrationHostedService> logger)
    {
        _syncService = syncService;
        _leaseService = leaseService;
        _featureManager = featureManager;
        _settings = settings;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.StartConsentMigration(_logger);

        _timer = new Timer(async state => await ConsentMigrationDispatcher(state), _stop.Token, TimeSpan.Zero, TimeSpan.FromMilliseconds(_settings.CurrentValue.EmptyFeedDelayMs));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Dispatches the consent migration process in a separate task.
    /// </summary>
    /// <param name="state">Cancellation token for stopping execution.</param>
    private async Task ConsentMigrationDispatcher(object state)
    {
        if (Interlocked.Exchange(ref _isRunning, 1) == 1)
        {
            return;
        }

        try
        {
            var cancellationToken = (CancellationToken)state;
            var migrationSettings = _settings.CurrentValue;

            bool isEnabled = await _featureManager.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration);
            if (!isEnabled)
            {
                Log.FeatureDisabled(_logger);
                return;
            }

            await using var lease = await _leaseService.TryAcquireNonBlocking("access_management_consent_migration", cancellationToken);
            if (lease is null || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                int processedCount = await _syncService.ProcessBatch(cancellationToken);

                if (processedCount == 0)
                {
                    await Task.Delay(migrationSettings.EmptyFeedDelayMs, cancellationToken);
                    break;
                }
                else
                {
                    await Task.Delay(migrationSettings.NormalDelayMs, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException) when (((CancellationToken)state).IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.SyncError(_logger, ex);
            await Task.Delay(TimeSpan.FromMinutes(1), (CancellationToken)state);
        }
        finally
        {
            _isRunning = 0;
        }
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            Log.QuitConsentMigration(_logger);
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

    static partial class Log
    {
        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Consent Migration Service starting")]
        internal static partial void StartConsentMigration(ILogger logger);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Consent Migration Service stopping")]
        internal static partial void QuitConsentMigration(ILogger logger);

        [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Unexpected error in consent migration loop")]
        internal static partial void SyncError(ILogger logger, Exception ex);

        [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Consent migration feature flag is disabled. Waiting before rechecking.")]
        internal static partial void FeatureDisabled(ILogger logger);
    }
}
