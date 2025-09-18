using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Data;

public partial class StaticDataIngest
{
    /// <summary>
    /// Ingest ProviderTypes
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestProvider(CancellationToken cancellationToken = default)
    {
        foreach (var d in ProviderConstants.AllEntities())
        {
            // Verify: Compare on Id or Code?
            var obj = db.Providers.FirstOrDefault(t => t.Id == d.Id);
            if (obj == null)
            {
                db.Providers.Add(d);
            }
            else
            {
                obj.Name = d.Entity.Name;
            }
        }
        
        await db.SaveChangesAsync(AuditValues, cancellationToken);
    }
}
