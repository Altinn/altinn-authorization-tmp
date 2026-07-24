using Altinn.AccessManagement.Core.Models;

namespace Altinn.AccessManagement.Core.Clients.Interfaces
{
    /// <summary>
    /// Interface for client for getting Altinn rights from AltinnII SBL Bridge
    /// </summary>
    public interface IAltinn2RightsClient
    {
        /// <summary>
        /// Operation to clear a recipients cached rights from a given reportee/from party, and the recipients authorized parties/reportees
        /// </summary>
        /// <param name="fromPartyId">The party id of the from party</param>
        /// <param name="toPartyId">The party id of the to party</param>
        /// <param name="toUserId">The user id of the to party (if the recipient is a user)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>HttpResponse</returns>
        Task<HttpResponseMessage> ClearReporteeRights(int fromPartyId, int toPartyId, int toUserId = 0, CancellationToken cancellationToken = default);
    }
}
