using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Data;

public partial class StaticDataIngest
{
    /// <summary>
    /// Ingest EntityVariant
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestEntityVariant(CancellationToken cancellationToken = default)
    {
        foreach (var entity in EntityVariantConstants.AllEntities())
        {
            var obj = await db.EntityVariants.FirstOrDefaultAsync(t => t.Id == entity.Id, cancellationToken);
            if (obj is { })
            {
                obj.Name = entity.Entity.Name;
            }
            else
            {
                db.EntityVariants.Add(entity);
            }
        }

        foreach (var translation in EntityVariantConstants.AllTranslations())
        {
            await translationService.UpsertTranslationAsync(translation);
        }

        var result = await db.SaveChangesAsync(AuditValues, cancellationToken);
    }
}
