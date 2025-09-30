using Altinn.AccessMgmt.PersistenceEF.Constants;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Data;

public partial class StaticDataIngest
{
    /// <summary>
    /// Ingest Packages
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestPackage(CancellationToken cancellationToken = default)
    {
        var packages = await db.Packages
            .AsTracking()
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        foreach (var seed in PackageConstants.AllEntities())
        {
            if (packages.TryGetValue(seed, out var entity))
            {
                entity.ProviderId = seed.Entity.ProviderId;
                entity.EntityTypeId = seed.Entity.EntityTypeId;
                entity.AreaId = seed.Entity.AreaId;
                entity.Urn = seed.Entity.Urn;
                entity.Name = seed.Entity.Name;
                entity.Description = seed.Entity.Description;
                entity.IsDelegable = seed.Entity.IsDelegable;
                entity.HasResources = seed.Entity.HasResources;
                entity.IsAssignable = seed.Entity.IsAssignable;
            }
            else
            {
                db.Packages.Add(seed);
            }
        }

        await db.SaveChangesAsync(AuditValues, cancellationToken);
    }
}
