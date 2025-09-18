using Altinn.AccessMgmt.PersistenceEF.Constants;

namespace Altinn.AccessMgmt.PersistenceEF.Data;

public partial class StaticDataIngest
{
    /// <summary>
    /// Ingest AreaGroup
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task IngestAreaGroup(CancellationToken cancellationToken = default)
    {
        foreach (var d in AreaGroupConstants.AllEntities())
        {
            var obj = db.AreaGroups.FirstOrDefault(t => t.Id == d.Id);
            if (obj == null)
            {
                db.AreaGroups.Add(d);
            }
            else
            {
                if (!obj.Name.Equals(d.Entity.Name) || !obj.Description.Equals(d.Entity.Description))
                {
                    obj.Name = d.Entity.Name;
                    obj.Description = d.Entity.Description;
                }
            }
        }

        foreach (var translation in AreaGroupConstants.AllTranslations())
        {
            await translationService.UpsertTranslationAsync(translation);
        }

        var result = await db.SaveChangesAsync(AuditValues, cancellationToken);
    }
}
