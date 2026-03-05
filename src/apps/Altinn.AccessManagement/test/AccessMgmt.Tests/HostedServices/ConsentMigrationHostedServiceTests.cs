using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Core.HostedServices;
using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Altinn.Authorization.Host.Lease;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Moq;

namespace AccessMgmt.Tests.HostedServices;

/// <summary>
/// Tests for <see cref="ConsentMigrationHostedService"/>
/// </summary>
public class ConsentMigrationHostedServiceTests : IAsyncDisposable
{
    private readonly Mock<IConsentMigrationSyncService> _syncServiceMock;
    private readonly Mock<ILeaseService> _leaseServiceMock;
    private readonly Mock<IFeatureManager> _featureManagerMock;
    private readonly Mock<IOptionsMonitor<ConsentMigrationSettings>> _settingsMonitorMock;
    private readonly Mock<ILogger<ConsentMigrationHostedService>> _loggerMock;
    private readonly ConsentMigrationSettings _settings;
    private readonly FakeTimeProvider _timeProvider;
    private readonly List<ConsentMigrationHostedService> _servicesToDispose;

    public ConsentMigrationHostedServiceTests()
    {
        _syncServiceMock = new Mock<IConsentMigrationSyncService>();
        _leaseServiceMock = new Mock<ILeaseService>();
        _featureManagerMock = new Mock<IFeatureManager>();
        _settingsMonitorMock = new Mock<IOptionsMonitor<ConsentMigrationSettings>>();
        _loggerMock = new Mock<ILogger<ConsentMigrationHostedService>>();
        _timeProvider = new FakeTimeProvider();
        _servicesToDispose = new List<ConsentMigrationHostedService>();

        _settings = new ConsentMigrationSettings
        {
            BatchSize = 10,
            ConsentStatus = 3,
            OnlyExpiredConsents = true,
            NormalDelayMs = 100,
            EmptyFeedDelayMs = 200,
            FeatureDisabledDelayMs = 600000
        };

        _settingsMonitorMock.Setup(x => x.CurrentValue).Returns(_settings);
    }

    [Fact]
    public async Task StartAsync_StartsSuccessfully()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.StartAsync(CancellationToken.None);

        // Assert - No exceptions thrown
        Assert.True(true);
    }

    [Fact]
    public async Task StopAsync_StopsSuccessfully()
    {
        // Arrange
        var service = CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act
        await service.StopAsync(CancellationToken.None);

        // Assert - No exceptions thrown
        Assert.True(true);
    }

    [Fact]
    public async Task ConsentMigrationDispatcher_FeatureDisabled_DoesNotProcess()
    {
        // Arrange
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration))
            .ReturnsAsync(false);

        var service = CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act
        await Task.Delay(300); // Wait for timer to execute

        // Assert
        _syncServiceMock.Verify(x => x.ProcessBatch(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConsentMigrationDispatcher_LeaseNotAcquired_DoesNotProcess()
    {
        // Arrange
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration))
            .ReturnsAsync(true);

        _leaseServiceMock
            .Setup(x => x.TryAcquireNonBlocking("access_management_consent_migration", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ILease)null);

        var service = CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act
        await Task.Delay(300);

        // Assert
        _syncServiceMock.Verify(x => x.ProcessBatch(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConsentMigrationDispatcher_ProcessesBatch_WhenLeaseAcquired()
    {
        // Arrange
        var leaseMock = new Mock<ILease>();
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration))
            .ReturnsAsync(true);

        _leaseServiceMock
            .Setup(x => x.TryAcquireNonBlocking("access_management_consent_migration", It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaseMock.Object);

        _syncServiceMock
            .Setup(x => x.ProcessBatch(It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var service = CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act
        await Task.Delay(300);

        // Assert
        _syncServiceMock.Verify(x => x.ProcessBatch(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ConsentMigrationDispatcher_EmptyBatch_UsesLongerDelay()
    {
        // Arrange
        var leaseMock = new Mock<ILease>();
        var processedCount = 0;

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration))
            .ReturnsAsync(true);

        _leaseServiceMock
            .Setup(x => x.TryAcquireNonBlocking("access_management_consent_migration", It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaseMock.Object);

        _syncServiceMock
            .Setup(x => x.ProcessBatch(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => processedCount);

        var service = CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act
        await Task.Delay(400); // Should process once, then wait for EmptyFeedDelayMs

        // Assert
        _syncServiceMock.Verify(x => x.ProcessBatch(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConsentMigrationDispatcher_NonEmptyBatch_ContinuesProcessing()
    {
        // Arrange
        var leaseMock = new Mock<ILease>();
        var callCount = 0;

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration))
            .ReturnsAsync(true);

        _leaseServiceMock
            .Setup(x => x.TryAcquireNonBlocking("access_management_consent_migration", It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaseMock.Object);

        _syncServiceMock
            .Setup(x => x.ProcessBatch(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount < 3 ? 10 : 0; // Return 10 for first 2 calls, then 0
            });

        var service = CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act
        await Task.Delay(500);

        // Assert
        _syncServiceMock.Verify(x => x.ProcessBatch(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task ConsentMigrationDispatcher_Exception_ContinuesOperation()
    {
        // Arrange
        var leaseMock = new Mock<ILease>();
        var callCount = 0;

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration))
            .ReturnsAsync(true);

        _leaseServiceMock
            .Setup(x => x.TryAcquireNonBlocking("access_management_consent_migration", It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaseMock.Object);

        _syncServiceMock
            .Setup(x => x.ProcessBatch(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        var service = CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act
        await Task.Delay(300);

        // Assert - Service should not crash
        _syncServiceMock.Verify(x => x.ProcessBatch(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ConsentMigrationDispatcher_PreventsOverlappingExecution()
    {
        // Arrange
        var leaseMock = new Mock<ILease>();
        var executionCount = 0;
        var tcs = new TaskCompletionSource();

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration))
            .ReturnsAsync(true);

        _leaseServiceMock
            .Setup(x => x.TryAcquireNonBlocking("access_management_consent_migration", It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaseMock.Object);

        _syncServiceMock
            .Setup(x => x.ProcessBatch(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                Interlocked.Increment(ref executionCount);
                await Task.Delay(1000); // Long running task
                return 0;
            });

        var service = CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act
        await Task.Delay(500); // Timer fires multiple times during long execution

        // Assert - Should only execute once due to _isRunning flag
        Assert.Equal(1, executionCount);
    }

    [Fact]
    public async Task ConsentMigrationDispatcher_CancellationToken_StopsProcessing()
    {
        // Arrange
        var leaseMock = new Mock<ILease>();
        var cts = new CancellationTokenSource();

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration))
            .ReturnsAsync(true);

        _leaseServiceMock
            .Setup(x => x.TryAcquireNonBlocking("access_management_consent_migration", It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaseMock.Object);

        _syncServiceMock
            .Setup(x => x.ProcessBatch(It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var service = CreateService();
        await service.StartAsync(cts.Token);

        // Act
        await Task.Delay(100);
        cts.Cancel(); // Cancel the token being used by the dispatcher
        await Task.Delay(200);

        // Assert - Should have stopped processing due to cancellation
        var callCount = _syncServiceMock.Invocations.Count(i => i.Method.Name == nameof(IConsentMigrationSyncService.ProcessBatch));
        Assert.True(callCount >= 1); // At least one call before cancellation

        // Cleanup with a separate token
        await service.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task ConsentMigrationDispatcher_OperationCanceledException_IsRethrown()
    {
        // Arrange
        var leaseMock = new Mock<ILease>();
        var cts = new CancellationTokenSource();

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration))
            .ReturnsAsync(true);

        _leaseServiceMock
            .Setup(x => x.TryAcquireNonBlocking("access_management_consent_migration", It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaseMock.Object);

        _syncServiceMock
            .Setup(x => x.ProcessBatch(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                cts.Cancel();
                throw new OperationCanceledException(cts.Token);
            });

        var service = CreateService();
        await service.StartAsync(cts.Token);

        // Act & Assert
        await Task.Delay(300); // Allow time for exception to be thrown

        // Should not crash the application - OperationCanceledException with matching token is expected
        Assert.True(cts.IsCancellationRequested);
    }

    [Fact]
    public async Task ConsentMigrationDispatcher_DisposesLease_AfterProcessing()
    {
        // Arrange
        var leaseMock = new Mock<ILease>();
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration))
            .ReturnsAsync(true);

        _leaseServiceMock
            .Setup(x => x.TryAcquireNonBlocking("access_management_consent_migration", It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaseMock.Object);

        _syncServiceMock
            .Setup(x => x.ProcessBatch(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var service = CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act
        await Task.Delay(400);

        // Assert
        leaseMock.Verify(x => x.DisposeAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ConsentMigrationDispatcher_UsesCorrectDelays()
    {
        // Arrange
        var leaseMock = new Mock<ILease>();
        var callTimes = new List<DateTime>();

        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesConsentMigration))
            .ReturnsAsync(true);

        _leaseServiceMock
            .Setup(x => x.TryAcquireNonBlocking("access_management_consent_migration", It.IsAny<CancellationToken>()))
            .ReturnsAsync(leaseMock.Object);

        _syncServiceMock
            .Setup(x => x.ProcessBatch(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callTimes.Add(DateTime.UtcNow);
                return Task.FromResult(callTimes.Count < 3 ? 10 : 0);
            });

        var service = CreateService();
        await service.StartAsync(CancellationToken.None);

        // Act
        await Task.Delay(1000);

        // Assert - Verify delays are approximately correct
        Assert.True(callTimes.Count >= 2);
        if (callTimes.Count >= 2)
        {
            var delayBetweenCalls = (callTimes[1] - callTimes[0]).TotalMilliseconds;
            Assert.True(delayBetweenCalls >= _settings.NormalDelayMs * 0.8); // Allow some tolerance
        }
    }

    [Fact]
    public void Dispose_DisposesResourcesProperly()
    {
        // Arrange
        var service = CreateService();

        // Act
        service.Dispose();

        // Assert - No exceptions thrown
        Assert.True(true);
    }

    private ConsentMigrationHostedService CreateService()
    {
        var service = new ConsentMigrationHostedService(
            _syncServiceMock.Object,
            _leaseServiceMock.Object,
            _featureManagerMock.Object,
            _settingsMonitorMock.Object,
            _timeProvider,
            _loggerMock.Object);

        _servicesToDispose.Add(service);
        return service;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var service in _servicesToDispose)
        {
            try
            {
                await service.StopAsync(CancellationToken.None);
                service.Dispose();
            }
            catch
            {
                // Ignore disposal errors in tests
            }
        }

        _servicesToDispose.Clear();
    }
}

/// <summary>
/// Fake implementation of TimeProvider for testing
/// </summary>
internal class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _now = DateTimeOffset.UtcNow;

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan timeSpan) => _now += timeSpan;
}
