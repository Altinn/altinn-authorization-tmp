using Altinn.AccessMgmt.PersistenceEF.Models;

namespace Altinn.AccessMgmt.Core.Services.Contracts
{
    public interface IResourceQueueService
    {
        /// <summary>
        /// Retrieves all errors marked for ReProcessing of the defined type.
        /// </summary>
        /// <param name="retriveFrom">value to retrive from used to fetching a page at a time</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
        /// <returns></returns>
        Task<List<ResourceQueue>> RetrieveItemsForProcessing(long retriveFrom = 1, CancellationToken cancellationToken = default);        
    }
}
