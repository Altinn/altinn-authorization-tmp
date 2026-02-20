using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessMgmt.Core.Services
{
    public class RightImportProgressService(AppDbContext db) : IRightImportProgressService
    {
        /// <inheritdoc />
        public async Task<bool> IsImportAlreadyProcessed(long delegationChangeId, string originType, CancellationToken cancellationToken)
        {
            var exists = await db.RightImportProgress.AnyAsync(r => r.DelegationChangeId == delegationChangeId && r.OriginType == originType, cancellationToken);

            return exists;
        }

        /// <inheritdoc />
        public async Task<bool> MarkImportAsProcessed(long delegationChangeId, string originType, AuditValues audit, CancellationToken cancellationToken)
        {
            RightImportProgress processed = new RightImportProgress
            {
                DelegationChangeId = delegationChangeId,
                OriginType = originType
            };

            db.RightImportProgress.Add(processed);
            var res = await db.SaveChangesAsync(audit, cancellationToken);
            return res > 0; 
        }
    }
}
