using Altinn.AccessManagement.Core.Models.IdPortenAuthorization;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Core.Clients.Interfaces
{
    /// <summary>
    /// Interface for the consent service
    /// </summary>
    public interface IIdPortenAuthorizationClient
    {
        /// <summary>
        /// Returns a specific concent based on the id
        /// </summary>
        /// <returns></returns>
        Task<List<IdPortenAuthorization>> GetIdPortenAuthorizations(string ssn, CancellationToken cancellationToken);

        /// <summary>
        /// Returns a specific concent based on the id. For end user
        /// </summary>
        /// <returns></returns>
        Task<bool> DeleteIdPortenAuthorization(string ssn, string id, CancellationToken cancellationToken);
    }
}
