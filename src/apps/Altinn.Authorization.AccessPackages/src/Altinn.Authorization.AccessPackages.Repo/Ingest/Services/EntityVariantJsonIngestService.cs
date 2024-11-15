using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest.Services;

/// <summary>
/// Ingest EntityVariants from Json files
/// </summary>
public class EntityVariantJsonIngestService : BaseJsonIngestService<EntityVariant, IEntityVariantService>, IIngestService<EntityVariant, IEntityVariantService>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityVariantJsonIngestService"/> class.
    /// </summary>
    /// <param name="service">Db repo from EntityVariant</param>
    /// <param name="config">JsonIngestConfig</param>
    public EntityVariantJsonIngestService(IEntityVariantService service, IOptions<JsonIngestConfig> config, JsonIngestMeters meters) : base(service, config, meters)
    {
        LoadTranslations = true;
    }
}
