using Altinn.AccessMgmt.PersistenceEF.Constants;

namespace Altinn.AccessMgmt.PersistenceEF.Data;

public partial class StaticDataIngest
{
    /// <summary>
    /// Ingest EntityType
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestEntityType(CancellationToken cancellationToken = default)
    {
        foreach (var entity in EntityTypeConstants.AllEntities())
        {
            var obj = db.EntityTypes.FirstOrDefault(t => t.Id == entity.Id);
            if (obj == null)
            {
                db.EntityTypes.Add(entity);
            }
            else
            {
                obj.Name = entity.Entity.Name;
            }
        }

        foreach (var translation in EntityTypeConstants.AllTranslations())
        {
            await translationService.UpsertTranslationAsync(translation);
        }

        var result = await db.SaveChangesAsync(AuditValues, cancellationToken);
    }
}
