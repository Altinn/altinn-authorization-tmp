using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Interface for the concent service
    /// </summary>
    public interface IConsent
    {
        /// <summary>
        /// Returns a specific concent based on the id
        /// </summary>
        /// <returns></returns>
        Task<Result<Consent>> GetConsent(Guid id, ConsentPartyUrn from, ConsentPartyUrn to, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a specific concent request. Requires the userId for the user that is requesting the concent.
        /// </summary>
        Task<ConsentRequestDetails> GetRequest(Guid id, Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a concent requests and return info about the created one.
        /// </summary>
        Task<Result<ConsentRequestDetails>> CreateRequest(ConsentRequest consentRequest, ConsentPartyUrn performedByParty,  CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a concent request
        /// </summary>
        Task<Result<ConsentRequestDetails>> RejectRequest(Guid id, Guid performedByParty, CancellationToken cancellationToken = default);

        /// <summary>
        /// Approves a concent request. The request needs to be a valid request. 
        /// </summary>
        Task<Result<ConsentRequestDetails>> AcceptRequest(Guid id, Guid performedByParty, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes a concent. The concent needs to be valid.
        /// </summary>
        Task<Result<ConsentRequestDetails>> RevokeConsent(Guid id, Guid performedByParty, CancellationToken cancellationToken = default);
    }
}
