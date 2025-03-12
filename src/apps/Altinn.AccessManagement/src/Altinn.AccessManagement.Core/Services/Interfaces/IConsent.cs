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
        Task<Consent> GetConcent(Guid id, ConsentPartyUrn from, ConsentPartyUrn to, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a specific concent request
        /// </summary>
        Task<ConsentRequestDetails> GetRequest(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a concent requests and return info about the created one.
        /// </summary>
        Task<Result<ConsentRequestDetails>> CreateRequest(ConsentRequest consentRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a concent request
        /// </summary>
        Task DenyRequest(Guid id, Guid performedByParty, CancellationToken cancellationToken = default);

        /// <summary>
        /// Approves a concent request. The request needs to be a valid request. 
        /// </summary>
        Task ApproveRequest(Guid id, Guid approvedByParty, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes a concent. The concent needs to be valid.
        /// </summary>
        Task RevokeConsent(Guid id, Guid performedByParty, CancellationToken cancellationToken = default);
    }
}
