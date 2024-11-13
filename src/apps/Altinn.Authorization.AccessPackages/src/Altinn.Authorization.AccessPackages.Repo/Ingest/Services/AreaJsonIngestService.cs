using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest.Services;

/// <summary>
/// Ingest Areas from Json files
/// </summary>
public class AreaJsonIngestService : BaseJsonIngestService<Area, IAreaService>, IIngestService<Area, IAreaService>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AreaJsonIngestService"/> class.
    /// </summary>
    /// <param name="service">Db repo from Role</param>
    /// <param name="config">JsonIngestConfig</param>
    public AreaJsonIngestService(IAreaService service, IOptions<JsonIngestConfig> config) : base(service, config)
    {
        LoadTranslations = true;
    }
}
