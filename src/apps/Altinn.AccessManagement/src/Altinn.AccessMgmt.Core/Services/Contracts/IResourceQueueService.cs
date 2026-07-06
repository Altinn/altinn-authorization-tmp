using Altinn.AccessMgmt.PersistenceEF.Models;

namespace Altinn.AccessMgmt.Core.Services.Contracts
{
    public interface IResourceQueueService
    {
        /// <summary>
        /// Retrieves queued resources for processing, starting from the specified id (inclusive).
        /// </summary>
        /// <param name="retrieveFrom">The id to start retrieving from (inclusive), used for paging.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The next page of queued resources ordered by id.</returns>
        Task<List<ResourceQueue>> RetrieveItemsForProcessing(long retrieveFrom = 1, CancellationToken cancellationToken = default);        
    }
}
