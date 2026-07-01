using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services
{
    public class ResourceQueueService(AppDbContext db) : IResourceQueueService
    {
        /// <inheritdoc />
        public async Task<List<ResourceQueue>> RetrieveItemsForProcessing(long retriveFrom, CancellationToken cancellationToken)
        {
            var items = await db.ResourceQueue.AsNoTracking()
            .Where(t => t.Id >= retriveFrom)
            .OrderBy(t => t.Id)
            .Take(100)
            .ToListAsync(cancellationToken);

            return items;
        }
    }
}
