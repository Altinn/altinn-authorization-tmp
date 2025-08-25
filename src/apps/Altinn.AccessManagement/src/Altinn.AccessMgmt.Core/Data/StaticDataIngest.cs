using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Utils;

namespace Altinn.AccessMgmt.Core.Data;

/// <summary>
/// Ingest static data into the database
/// </summary>
/// <param name="db">AppDbContext</param>
/// <param name="translationService">TranslationService</param>
public partial class StaticDataIngest(AppDbContext db, ITranslationService translationService, AuditValues auditValues, IIngestService ingestService)
{
    public async Task IngestAll(CancellationToken cancellationToken = default)
    {
        await IngestProviderType(cancellationToken);
    }
}
