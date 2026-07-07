using Altinn.AccessManagement.Core.Models.IdPortenAuthorization;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Interface for the consent service
    /// </summary>
    public interface IIdPortenAuthorizationService
    {
        /// <summary>
        /// Returns a specific concent based on the id
        /// </summary>
        /// <returns></returns>
        Task<Result<List<IdPortenAuthorization>>> GetIdPortenAuthorizations(Guid partyUuid, CancellationToken cancellationToken);

        /// <summary>
        /// Returns a specific concent based on the id. For end user
        /// </summary>
        /// <returns></returns>
        Task<Result<bool>> DeleteIdPortenAuthorization(Guid partyUuid, string id, CancellationToken cancellationToken);
    }
}
