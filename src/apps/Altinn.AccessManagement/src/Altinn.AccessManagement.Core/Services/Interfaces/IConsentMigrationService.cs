using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Consent;

namespace Altinn.AccessManagement.Core.Services.Interfaces;

/// <summary>
/// Service for migrating individual consents from old application to new system
/// </summary>
public interface IConsentMigrationService
{
    /// <summary>
    /// Migrates a consent request to the new system
    /// </summary>
    /// <param name="consentRequest">The consent request from the old application</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success, failure, or duplicate</returns>
    Task<ConsentMigrationResult> MigrateConsentRequest(ConsentRequest consentRequest, CancellationToken cancellationToken);
}
