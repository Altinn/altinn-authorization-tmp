using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Configuration;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessMgmt.Core.HostedServices.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Diagnostics.Metrics;
using System.Linq;

namespace AccessMgmt.Tests.Services;

/// <summary>
/// Tests for <see cref="ConsentMigrationSyncService"/>
/// </summary>
public class ConsentMigrationSyncServiceTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IAltinn2ConsentClient> _clientMock;
    private readonly Mock<IConsentMigrationService> _migrationServiceMock;
    private readonly Mock<ILogger<ConsentMigrationSyncService>> _loggerMock;
    private readonly Mock<IMeterFactory> _meterFactoryMock;
    private readonly ConsentMigrationSettings _settings;
    private readonly TimeProvider _timeProvider;

    public ConsentMigrationSyncServiceTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _clientMock = new Mock<IAltinn2ConsentClient>();
        _migrationServiceMock = new Mock<IConsentMigrationService>();
        _loggerMock = new Mock<ILogger<ConsentMigrationSyncService>>();
        _meterFactoryMock = new Mock<IMeterFactory>();
        _timeProvider = TimeProvider.System;

        _settings = new ConsentMigrationSettings
        {
            BatchSize = 10,
            ConsentStatus = 3,
            OnlyExpiredConsents = true,
            NormalDelayMs = 1000,
            EmptyFeedDelayMs = 5000,
            EndDate = DateTimeOffset.UtcNow.AddDays(30).DateTime,
        };

        // Setup meter factory
        var meterMock = new Mock<Meter>("Test", null);
        _meterFactoryMock.Setup(x => x.Create(It.IsAny<MeterOptions>()))
            .Returns(meterMock.Object);

        // Setup service provider and scope
        var scopeServiceProvider = new Mock<IServiceProvider>();
        scopeServiceProvider.Setup(x => x.GetService(typeof(IAltinn2ConsentClient)))
            .Returns(_clientMock.Object);
        scopeServiceProvider.Setup(x => x.GetService(typeof(IConsentMigrationService)))
            .Returns(_migrationServiceMock.Object);

        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(scopeServiceProvider.Object);
        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_serviceScopeFactoryMock.Object);
    }

    [Fact]
    public async Task ProcessBatch_NoConsents_ReturnsZero()
    {
        // Arrange
        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            It.IsAny<int>(),
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid>());

        var service = CreateService();

        // Act
        var result = await service.ProcessBatch(CancellationToken.None);

        // Assert
        Assert.Equal(0, result);
        _migrationServiceMock.Verify(x => x.MigrateConsent(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessBatch_WithConsents_ProcessesAll()
    {
        // Arrange
        var consentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            It.IsAny<int>(),
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(consentIds);

        _migrationServiceMock.Setup(x => x.MigrateConsent(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentMigrationResult.Succeeded());

        var service = CreateService();

        // Act
        var result = await service.ProcessBatch(CancellationToken.None);

        // Assert
        Assert.Equal(3, result);
        _migrationServiceMock.Verify(x => x.MigrateConsent(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task ProcessBatch_NetworkError_ReturnsZero()
    {
        // Arrange
        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            It.IsAny<int>(),
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var service = CreateService();

        // Act
        var result = await service.ProcessBatch(CancellationToken.None);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task ProcessBatch_Timeout_ReturnsZero()
    {
        // Arrange
        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            It.IsAny<int>(),
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        var service = CreateService();

        // Act
        var result = await service.ProcessBatch(CancellationToken.None);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task ProcessBatch_PartialSuccess_ContinuesProcessing()
    {
        // Arrange
        var consentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            It.IsAny<int>(),
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(consentIds);

        // First consent succeeds
        _migrationServiceMock.Setup(x => x.MigrateConsent(consentIds[0], It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentMigrationResult.Succeeded());

        // Second consent fails
        _migrationServiceMock.Setup(x => x.MigrateConsent(consentIds[1], It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentMigrationResult.Failed("Test error"));

        // Third consent succeeds
        _migrationServiceMock.Setup(x => x.MigrateConsent(consentIds[2], It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentMigrationResult.Succeeded());

        var service = CreateService();

        // Act
        var result = await service.ProcessBatch(CancellationToken.None);

        // Assert
        Assert.Equal(3, result);
        var stats = service.GetStatistics();
        Assert.Equal(3, stats.Processed);
        Assert.Equal(2, stats.Migrated);
        Assert.Equal(1, stats.Failed);
    }

    [Fact]
    public async Task ProcessBatch_ConsentThrowsException_ContinuesWithOthers()
    {
        // Arrange
        var consentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            It.IsAny<int>(),
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(consentIds);

        // First throws exception
        _migrationServiceMock.Setup(x => x.MigrateConsent(consentIds[0], It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Second succeeds
        _migrationServiceMock.Setup(x => x.MigrateConsent(consentIds[1], It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentMigrationResult.Succeeded());

        var service = CreateService();

        // Act
        var result = await service.ProcessBatch(CancellationToken.None);

        // Assert
        Assert.Equal(2, result);
        var stats = service.GetStatistics();
        Assert.Equal(2, stats.Processed);
        Assert.Equal(1, stats.Migrated);
        Assert.Equal(1, stats.Failed);
    }

    [Fact]
    public async Task ProcessBatch_Cancelled_ReturnsPartialCount()
    {
        // Arrange
        var consentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            It.IsAny<int>(),
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var service = CreateService();

        // Act
        var result = await service.ProcessBatch(CancellationToken.None);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetStatistics_ReturnsCorrectValues()
    {
        // Arrange
        var service = CreateService();

        // Act
        var stats = service.GetStatistics();

        // Assert
        Assert.Equal(0, stats.Processed);
        Assert.Equal(0, stats.Migrated);
        Assert.Equal(0, stats.Failed);
        Assert.Equal(default, stats.LastRun);
    }

    [Fact]
    public async Task ProcessBatch_MultipleBatches_AccumulatesStatistics()
    {
        // Arrange
        var batch1 = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var batch2 = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var callCount = 0;
        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            It.IsAny<int>(),
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => callCount++ == 0 ? batch1 : batch2);

        _migrationServiceMock.Setup(x => x.MigrateConsent(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentMigrationResult.Succeeded());

        var service = CreateService();

        // Act
        await service.ProcessBatch(CancellationToken.None);
        await service.ProcessBatch(CancellationToken.None);
        var stats = service.GetStatistics();

        // Assert
        Assert.Equal(5, stats.Processed); // 2 + 3
        Assert.Equal(5, stats.Migrated);
        Assert.Equal(0, stats.Failed);
    }

    [Fact]
    public async Task ProcessBatch_AllFailed_UpdatesStatistics()
    {
        // Arrange
        var consentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            It.IsAny<int>(),
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
          .ReturnsAsync(consentIds);

        _migrationServiceMock.Setup(x => x.MigrateConsent(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(ConsentMigrationResult.Failed("Migration failed"));

        var service = CreateService();

        // Act
        var result = await service.ProcessBatch(CancellationToken.None);
        var stats = service.GetStatistics();

        // Assert
        Assert.Equal(2, result);
        Assert.Equal(2, stats.Processed);
        Assert.Equal(0, stats.Migrated);
        Assert.Equal(2, stats.Failed);
    }

    [Fact]
    public async Task ProcessBatch_UpdatesLastRunTime()
    {
        // Arrange
        var consentIds = new List<Guid> { Guid.NewGuid() };
        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            It.IsAny<int>(),
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
          .ReturnsAsync(consentIds);

        _migrationServiceMock.Setup(x => x.MigrateConsent(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(ConsentMigrationResult.Succeeded());

        var service = CreateService();
        var statsBefore = service.GetStatistics();

        // Act
        await Task.Delay(100); // Ensure time difference
        await service.ProcessBatch(CancellationToken.None);
        var statsAfter = service.GetStatistics();

        // Assert
        Assert.True(statsAfter.LastRun > statsBefore.LastRun);
    }

    [Fact]
    public async Task ProcessBatch_EmptyBatch_DoesNotUpdateStatistics()
    {
        // Arrange
        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            It.IsAny<int>(),
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
          .ReturnsAsync(new List<Guid>());

        var service = CreateService();
        var statsBefore = service.GetStatistics();

        // Act
        await service.ProcessBatch(CancellationToken.None);
        var statsAfter = service.GetStatistics();

        // Assert - Statistics unchanged except LastRun
        Assert.Equal(statsBefore.Processed, statsAfter.Processed);
        Assert.Equal(statsBefore.Migrated, statsAfter.Migrated);
        Assert.Equal(statsBefore.Failed, statsAfter.Failed);
        Assert.True(statsAfter.LastRun >= statsBefore.LastRun);
    }

    [Fact]
    public async Task ProcessBatch_UnexpectedException_UpdatesFailedCount()
    {
        // Arrange
        var consentIds = new List<Guid> { Guid.NewGuid() };
        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            It.IsAny<int>(),
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
          .ReturnsAsync(consentIds);

        _migrationServiceMock.Setup(x => x.MigrateConsent(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
          .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        var service = CreateService();

        // Act
        var result = await service.ProcessBatch(CancellationToken.None);
        var stats = service.GetStatistics();

        // Assert
        Assert.Equal(1, result);
        Assert.Equal(1, stats.Processed);
        Assert.Equal(0, stats.Migrated);
        Assert.Equal(1, stats.Failed);
    }

    [Fact]
    public async Task ProcessBatch_MixedExceptionsAndResults_HandlesAll()
    {
        // Arrange
        var consentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            It.IsAny<int>(),
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
          .ReturnsAsync(consentIds);

        // Mix of outcomes
        _migrationServiceMock.Setup(x => x.MigrateConsent(consentIds[0], It.IsAny<CancellationToken>()))
          .ReturnsAsync(ConsentMigrationResult.Succeeded());

        _migrationServiceMock.Setup(x => x.MigrateConsent(consentIds[1], It.IsAny<CancellationToken>()))
          .ThrowsAsync(new HttpRequestException("Network error"));

        _migrationServiceMock.Setup(x => x.MigrateConsent(consentIds[2], It.IsAny<CancellationToken>()))
          .ReturnsAsync(ConsentMigrationResult.Failed("Migration failed"));

        _migrationServiceMock.Setup(x => x.MigrateConsent(consentIds[3], It.IsAny<CancellationToken>()))
          .ReturnsAsync(ConsentMigrationResult.Succeeded());

        var service = CreateService();

        // Act
        var result = await service.ProcessBatch(CancellationToken.None);
        var stats = service.GetStatistics();

        // Assert
        Assert.Equal(4, result);
        Assert.Equal(4, stats.Processed);
        Assert.Equal(2, stats.Migrated);
        Assert.Equal(2, stats.Failed);
    }

    [Fact]
    public async Task ProcessBatch_SuccessRate_CalculatesCorrectly()
    {
        // Arrange
        var consentIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            It.IsAny<int>(),
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
          .ReturnsAsync(consentIds);

        // 3 succeed, 1 fails
        _migrationServiceMock.SetupSequence(x => x.MigrateConsent(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(ConsentMigrationResult.Succeeded())
          .ReturnsAsync(ConsentMigrationResult.Succeeded())
          .ReturnsAsync(ConsentMigrationResult.Failed("Error"))
          .ReturnsAsync(ConsentMigrationResult.Succeeded());

        var service = CreateService();

        // Act
        await service.ProcessBatch(CancellationToken.None);
        var stats = service.GetStatistics();

        // Assert
        Assert.Equal(4, stats.Processed);
        Assert.Equal(3, stats.Migrated);
        Assert.Equal(1, stats.Failed);

        // Success rate = 75%
        var successRate = (double)stats.Migrated / stats.Processed;
        Assert.Equal(0.75, successRate, 2);
    }

    [Fact]
    public async Task ProcessBatch_RespectsConfiguredBatchSize()
    {
        // Arrange
        var largeConsentList = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToList();

        _clientMock.Setup(x => x.GetAltinn2ConsentListForMigration(
            _settings.BatchSize, // Should request exactly BatchSize
            It.IsAny<int?>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(largeConsentList.Take(_settings.BatchSize).ToList());

        _migrationServiceMock.Setup(x => x.MigrateConsent(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ConsentMigrationResult.Succeeded());

        var service = CreateService();

        // Act
        var result = await service.ProcessBatch(CancellationToken.None);

        // Assert
        Assert.Equal(_settings.BatchSize, result);
        _clientMock.Verify(
            x => x.GetAltinn2ConsentListForMigration(
                _settings.BatchSize,
                _settings.ConsentStatus,
                _settings.OnlyExpiredConsents,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private ConsentMigrationSyncService CreateService()
    {
        return new ConsentMigrationSyncService(
            _serviceProviderMock.Object,
            Options.Create(_settings),
            _timeProvider,
            _meterFactoryMock.Object,
            _loggerMock.Object);
    }
}
