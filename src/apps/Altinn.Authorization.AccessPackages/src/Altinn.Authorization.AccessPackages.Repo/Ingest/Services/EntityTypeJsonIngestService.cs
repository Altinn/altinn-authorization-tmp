using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest.Services;

/// <summary>
/// Ingest EntityTypes from Json files
/// </summary>
public class EntityTypeJsonIngestService : BaseJsonIngestService<EntityType, IEntityTypeService>, IIngestService<EntityType, IEntityTypeService>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityTypeJsonIngestService"/> class.
    /// </summary>
    /// <param name="service">Db repo from EntityType</param>
    /// <param name="config">JsonIngestConfig</param>
    public EntityTypeJsonIngestService(IEntityTypeService service, IOptions<JsonIngestConfig> config, JsonIngestMeters meters) : base(service, config, meters)
    {
        LoadTranslations = true;
    }
}
