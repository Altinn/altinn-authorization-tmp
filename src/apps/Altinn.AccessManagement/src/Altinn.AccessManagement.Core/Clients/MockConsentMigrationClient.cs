using System.Text.Json;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.Consent;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessManagement.Core.Clients;

/// <summary>
/// Mock implementation of consent migration client
/// </summary>
/// <remarks>
/// Replace this with the actual client implementation from the other PR
/// </remarks>
public class MockConsentMigrationClient : IConsentMigrationClient
{
    private readonly ILogger<MockConsentMigrationClient> _logger;
    private readonly object _lockObject = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MockConsentMigrationClient"/> class
    /// </summary>
    public MockConsentMigrationClient(ILogger<MockConsentMigrationClient> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<List<Guid>> GetConsentIdsForMigration(int batchSize, string status, CancellationToken cancellationToken)
    {
        lock (_lockObject)
        {
            _logger.LogInformation("Mock: Fetching up to {BatchSize} consent IDs with status '{Status}'", batchSize, status);

            // Read feed file directly
            var feedIds = ReadFeedFile();

            // Take only the requested batch size
            var batch = feedIds.Take(batchSize).ToList();

            _logger.LogInformation("Mock: Returning {Count} consent IDs from feed (Total available: {Total})", 
                batch.Count, feedIds.Count);

            return Task.FromResult(batch);
        }
    }

    /// <inheritdoc/>
    public Task<ConsentRequest> GetConsentDetails(Guid consentId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Mock: Fetching consent details for {ConsentId}", consentId);

        // Read consent data file and find the specific consent
        var allConsents = ReadConsentDataFile();
        var consent = allConsents.FirstOrDefault(c => c.Id == consentId);

        if (consent == null)
        {
            _logger.LogWarning("Mock: Consent {ConsentId} not found in mock data", consentId);
            return Task.FromResult<ConsentRequest>(null);
        }

        return Task.FromResult(consent);
    }

    /// <inheritdoc/>
    public Task UpdateMigrationStatus(Guid consentId, string status, CancellationToken cancellationToken)
    {
        lock (_lockObject)
        {
            _logger.LogInformation("Mock: Updating migration status for {ConsentId} to '{Status}'", consentId, status);

            // Read current consent data
            var allConsents = ReadConsentDataFile();
            var consent = allConsents.FirstOrDefault(c => c.Id == consentId);

            if (consent == null)
            {
                _logger.LogWarning("Mock: Consent {ConsentId} not found for status update", consentId);
                return Task.CompletedTask;
            }

            // Update status
            consent.MigrationStatus = status;

            // Write back to file
            WriteConsentDataFile(allConsents);

            // Update feed file (remove if migrated)
            UpdateFeedFile(allConsents);

            _logger.LogInformation("Mock: Updated and persisted migration status for {ConsentId}", consentId);

            return Task.CompletedTask;
        }
    }

    private List<Guid> ReadFeedFile()
    {
        string feedPath = FindFile("MockConsentFeed.json");

        if (string.IsNullOrEmpty(feedPath))
        {
            _logger.LogWarning("Mock: Feed file not found. Returning empty list.");
            return [];
        }

        string jsonContent = File.ReadAllText(feedPath);
        var feedIds = JsonSerializer.Deserialize<List<Guid>>(jsonContent);

        return feedIds ?? [];
    }

    private List<ConsentRequest> ReadConsentDataFile()
    {
        string dataPath = FindFile("MockConsentData.json");

        if (string.IsNullOrEmpty(dataPath))
        {
            throw new FileNotFoundException("MockConsentData.json not found.");
        }

        string jsonContent = File.ReadAllText(dataPath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var mockData = JsonSerializer.Deserialize<MockConsentDataFile>(jsonContent, options);

        if (mockData?.Consents == null)
        {
            throw new InvalidDataException("MockConsentData.json contains no consents.");
        }

        return mockData.Consents;
    }

    private void WriteConsentDataFile(List<ConsentRequest> consents)
    {
        string dataPath = FindFile("MockConsentData.json");

        if (string.IsNullOrEmpty(dataPath))
        {
            _logger.LogWarning("Mock: Cannot save consent data - file not found");
            return;
        }

        var mockData = new MockConsentDataFile { Consents = consents };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        string jsonContent = JsonSerializer.Serialize(mockData, options);
        File.WriteAllText(dataPath, jsonContent);

        _logger.LogInformation("Mock: Saved consent data to {Path}", dataPath);
    }

    private void UpdateFeedFile(List<ConsentRequest> allConsents)
    {
        string feedPath = FindFile("MockConsentFeed.json");

        if (string.IsNullOrEmpty(feedPath))
        {
            _logger.LogWarning("Mock: Cannot update feed - file not found");
            return;
        }

        // Feed contains only pending/failed consent GUIDs
        var pendingIds = allConsents
            .Where(c => c.MigrationStatus == "pending" || c.MigrationStatus == "failed")
            .Select(c => c.Id)
            .ToList();

        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonContent = JsonSerializer.Serialize(pendingIds, options);
        File.WriteAllText(feedPath, jsonContent);

        _logger.LogInformation("Mock: Updated feed file with {Count} pending/failed consents", pendingIds.Count);
    }

    private string FindFile(string fileName)
    {
        string assemblyDirectory = Path.GetDirectoryName(typeof(MockConsentMigrationClient).Assembly.Location);

        var possiblePaths = new[]
        {
            Path.Combine(assemblyDirectory, "Clients", "Testdata", "Consent", fileName),
            Path.Combine(assemblyDirectory, "..", "..", "..", "Altinn.AccessManagement.Core", "Clients", "Testdata", "Consent", fileName)
        };

        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                _logger.LogInformation("Mock: Found {FileName} at {Path}", fileName, path);
                return path;
            }
        }

        return null;
    }

    private class MockConsentDataFile
    {
        public List<ConsentRequest> Consents { get; set; }
    }
}
