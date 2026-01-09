using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;

namespace Altinn.AccessMgmt.Core.Services
{
    public class ErrorQueueService(AppDbContext db) : IErrorQueueService
    {
        public async Task<bool> AddErrorQueue(ErrorQueue error, AuditValues values, CancellationToken cancellationToken)
        {
            await db.ErrorQueue.AddAsync(error, cancellationToken);
            var affected = await db.SaveChangesAsync(values, cancellationToken);
            return affected != 0;
        }
    }
}
