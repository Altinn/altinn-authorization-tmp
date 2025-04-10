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
        /// Get a specific concent request. Requires the userId for the user that is requesting the concent.
        /// </summary>
        Task<ConsentRequestDetails> GetRequest(Guid consentRequestId, Guid userId, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a concent requests and return info about the created one.
        /// </summary>
        Task<Result<ConsentRequestDetails>> CreateRequest(ConsentRequest consentRequest, ConsentPartyUrn performedByParty,  CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a concent request
        /// </summary>
        Task<Result<ConsentRequestDetails>> RejectRequest(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken);

        /// <summary>
        /// Approves a concent request. The request needs to be a valid request. 
        /// </summary>
        Task<Result<ConsentRequestDetails>> AcceptRequest(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken);

        /// <summary>
        /// Revokes a concent. The concent needs to be valid.
        /// </summary>
        Task<Result<ConsentRequestDetails>> RevokeConsent(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken);
    }
}
