using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest.Services;

/// <summary>
/// Ingest RolePackages from Json files
/// </summary>
public class RolePackageJsonIngestService : BaseJsonIngestService<RolePackage, IRolePackageService>, IIngestService<RolePackage, IRolePackageService>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RolePackageJsonIngestService"/> class.
    /// </summary>
    /// <param name="service">Db repo from RolePackage</param>
    /// <param name="config">JsonIngestConfig</param>
    public RolePackageJsonIngestService(IRolePackageService service, IOptions<JsonIngestConfig> config, JsonIngestMeters meters) : base(service, config, meters) { }
}
