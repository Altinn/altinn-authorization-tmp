using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.Platform.Register.Models;

namespace Altinn.AccessManagement.Core.Clients.Interfaces;

/// <summary>
/// Interface for a client wrapper for integration with SBL bridge delegation request API
/// </summary>
public interface IAltinn2ConsentClient
{
    /// <summary>
    /// Returns Consent
    /// </summary>
    /// <param name="consentGuid">The consent GUID to lookup</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Consent information</returns>
    Task<ConsentRequestDetails> GetConsent(Guid consentGuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates status of guid
    /// </summary>
    /// <param name="consentGuid">The consent GUID to update</param>
    /// <param name="status">The status to set</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Boolean</returns>
    Task<bool> UpdateConsentStatus(Guid consentGuid, int status, CancellationToken cancellationToken = default);
}
