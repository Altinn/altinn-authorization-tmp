using Altinn.Authorization.Core.Models.Consent;

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
        Task<ConsentRequestDetails> CreateRequest(ConsentRequest consentRequest, ConsentPartyUrn performedBy, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a consent request. Can only be performed by the party that created the request. Will be soft deleted.
        /// </summary>
        Task DeleteRequest(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a specific consent request based on the id
        /// </summary>
        Task<ConsentRequestDetails> GetRequest(Guid consentRequestId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Approves a consent request
        /// </summary>
        Task AcceptConsentRequest(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken = default);

        /// <summary>
        /// Rejects a consent request
        /// </summary>
        Task RejectConsentRequest(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes a consent
        /// </summary>
        Task Revoke(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all active and historical consents for a party
        /// </summary>
        Task<List<Consent>> GetAllConsents(Guid partyUid, CancellationToken cancellationToken = default);
    }
}
