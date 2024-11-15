using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest.Services;

/// <summary>
/// Ingest RoleMaps from Json files
/// </summary>
public class RoleMapJsonIngestService : BaseJsonIngestService<RoleMap, IRoleMapService>, IIngestService<RoleMap, IRoleMapService>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoleMapJsonIngestService"/> class.
    /// </summary>
    /// <param name="service">Db repo from RoleMap</param>
    /// <param name="config">JsonIngestConfig</param>
    public RoleMapJsonIngestService(IRoleMapService service, IOptions<JsonIngestConfig> config, JsonIngestMeters meters) : base(service, config, meters) { }
}
