using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Altinn.AccessManagement.Core.Services;

/// <summary>
/// Service for migrating consents from old application to new system
/// </summary>
public class ConsentMigrationService : IConsentMigrationService
{
    private readonly IConsent _consentService;
    private readonly ILogger<ConsentMigrationService> _logger;
    private readonly object _lockObject = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsentMigrationService"/> class
    /// </summary>
    public ConsentMigrationService(
        IConsent consentService,
        ILogger<ConsentMigrationService> logger)
    {
        _consentService = consentService;
        _logger = logger;
    }

    /// <summary>
    /// Migrates a consent request to the new system
    /// </summary>
    /// <param name="consentRequest">The consent request from the old application</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success, failure, or duplicate</returns>
    public async Task<ConsentMigrationResult> MigrateConsentRequest(ConsentRequest consentRequest, CancellationToken cancellationToken)
    {
        try
        {
            if (consentRequest == null)
            {
                return ConsentMigrationResult.Failed("Consent request is null");
            }

            // Use from party as performedByParty (the consent is being created on behalf of the from party)
            ConsentPartyUrn performedByParty = consentRequest.From;

            // TODO: Create consent in new system via ConsentService
            // var result = await _consentService.CreateRequest(consentRequest, performedByParty, cancellationToken);

            // Write to file instead of calling service
            await WriteMigratedConsentToFile(consentRequest, cancellationToken);

            // TODO : uncomment and implement actual service call and result handling when ready to integrate with real service
            //if (result.IsProblem)
            //{
            //    return ConsentMigrationResult.Failed($"Failed to create consent: {result.Problem}");
            //}

            //// Check if consent already existed (duplicate)
            //if (result.Value.AlreadyExisted)
            //{
            //    _logger.LogInformation("Consent {ConsentId} already exists (duplicate), marking as migrated", consentRequest.Id);
            //    return ConsentMigrationResult.Duplicate();
            //}

            _logger.LogInformation("Successfully migrated consent {ConsentId}", consentRequest.Id);
            return ConsentMigrationResult.Succeeded();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while migrating consent {ConsentId}", consentRequest?.Id);
            return ConsentMigrationResult.Failed($"Exception: {ex.Message}");
        }
    }

    private async Task WriteMigratedConsentToFile(ConsentRequest consentRequest, CancellationToken cancellationToken)
    {
        lock (_lockObject)
        {
            string assemblyDirectory = Path.GetDirectoryName(typeof(ConsentMigrationService).Assembly.Location);
            string outputDirectory = Path.Combine(assemblyDirectory, "Clients", "Testdata", "Consent", "Migrated");

            // Ensure directory exists
            Directory.CreateDirectory(outputDirectory);

            string fileName = $"migrated_consent_{consentRequest.Id}.json";
            string filePath = Path.Combine(outputDirectory, fileName);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            string jsonContent = JsonSerializer.Serialize(consentRequest, options);
            File.WriteAllText(filePath, jsonContent);

            _logger.LogInformation("Mock: Wrote migrated consent to {Path}", filePath);
        }
    }
}
