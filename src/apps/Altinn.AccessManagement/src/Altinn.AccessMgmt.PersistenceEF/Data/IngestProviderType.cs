using Altinn.AccessMgmt.PersistenceEF.Constants;

namespace Altinn.AccessMgmt.PersistenceEF.Data;

public partial class StaticDataIngest
{
    /// <summary>
    /// Ingest ProviderTypes
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestProviderType(CancellationToken cancellationToken = default)
    {
        foreach (var entity in ProviderTypeConstants.AllEntities())
        {
            var obj = db.ProviderTypes.FirstOrDefault(t => t.Id == entity.Id);
            if (obj == null)
            {
                db.ProviderTypes.Add(entity);
            }
            else
            {
                obj.Name = entity.Entity.Name;
            }
        }

        foreach (var translation in ProviderTypeConstants.AllTranslations())
        {
            await translationService.UpsertTranslationAsync(translation);
        }

        var result = await db.SaveChangesAsync(AuditValues, cancellationToken);
    }
}
