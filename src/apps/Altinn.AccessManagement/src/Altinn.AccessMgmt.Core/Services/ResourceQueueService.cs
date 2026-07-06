using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services
{
    public class ResourceQueueService(AppDbContext db) : IResourceQueueService
    {
        /// <inheritdoc />
        public async Task<List<ResourceQueue>> RetrieveItemsForProcessing(long retrieveFrom, CancellationToken cancellationToken)
        {
            var items = await db.ResourceQueue.AsNoTracking()
                .Where(t => t.Id >= retrieveFrom)
                .OrderBy(t => t.Id)
                .Take(100)
                .ToListAsync(cancellationToken);

            return items;
        }
    }
}
