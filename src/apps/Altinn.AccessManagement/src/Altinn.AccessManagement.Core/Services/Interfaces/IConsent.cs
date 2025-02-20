using Altinn.AccessManagement.Core.Models.Consent;

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
        Task<Consent> GetConcent(Guid id, string from, string to);

        /// <summary>
        /// Creates a concent requests and return info about the created one.
        /// </summary>
        Task<ConsentRequestDetails> CreateRequest(ConsentRequest consentRequest);

        /// <summary>
        /// Deletes a concent request
        /// </summary>
        Task DeleteRequest(Guid id);

        /// <summary>
        /// Approves a concent request. The request needs to be a valid request. 
        /// </summary>
        Task ApproveRequest(Guid id);
    }
}
