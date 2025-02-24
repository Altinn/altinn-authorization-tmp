using Altinn.AccessManagement.Core.Models.Consent;

namespace Altinn.AccessManagement.Core.Repositories.Interfaces
{
    /// <summary>
    /// Repository for handling consent
    /// </summary>
    public interface IConsentRepository
    {
        /// <summary>
        /// Creates a consent request
        /// </summary>
        Task<ConsentRequestDetails> CreateRequest(ConsentRequest consentRequest);

        /// <summary>
        /// Deletes a consent request. Can only be performed by the party that created the request. Will be soft deleted.
        /// </summary>
        Task DeleteRequest(Guid id);

        /// <summary>
        /// Returns a specific consent request based on the id
        /// </summary>
        Task<ConsentRequestDetails> GetRequest(Guid id);

        /// <summary>
        /// Approves a consent request
        /// </summary>
        Task ApproveConsentRequest(Guid id);

        /// <summary>
        /// Rejects a consent request
        /// </summary>
        Task RejectConsentRequest(Guid id);

        /// <summary>
        /// Gets a consent
        /// </summary>
        Task<Consent> GetConsent(Guid id);

        /// <summary>
        /// Revokes a consent
        /// </summary>
        Task Revoke(Guid id);

        /// <summary>
        /// Gets all active and historical consents for a party
        /// </summary>
        Task<List<Consent>> GetAllConsents(Guid partyUid);
    }
}
