using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Interface for the consent service
    /// </summary>
    public interface IConsent
    {
        /// <summary>
        /// Returns a specific concent based on the id
        /// </summary>
        /// <returns></returns>
        Task<Result<Consent>> GetConsent(Guid consentRequestId, ConsentPartyUrn from, ConsentPartyUrn to, CancellationToken cancellationToken);

        /// <summary>
        /// Get a specific consent request. Requires the userId for the user that is requesting the concent.
        /// </summary>
        Task<Result<ConsentRequestDetails>> GetRequest(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a consent requests and return info about the created one. Available for enteprises. 
        /// </summary>
        Task<Result<ConsentRequestDetails>> CreateRequest(ConsentRequest consentRequest, ConsentPartyUrn performedByParty,  CancellationToken cancellationToken);

        /// <summary>
        /// Rejects a consent request. For end user
        /// </summary>
        Task<Result<ConsentRequestDetails>> RejectRequest(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken);

        /// <summary>
        /// Approves a concent request. The request needs to be a valid request. 
        /// </summary>
        Task<Result<ConsentRequestDetails>> AcceptRequest(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken);

        /// <summary>
        /// Revokes a consent. The consent needs to be in accepted state to be able to be revoked.
        /// </summary>
        Task<Result<ConsentRequestDetails>> RevokeConsent(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken);
    }
}
