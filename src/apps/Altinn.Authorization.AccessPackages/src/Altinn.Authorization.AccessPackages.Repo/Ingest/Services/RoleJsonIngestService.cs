using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest.Services;

/// <summary>
/// Ingest Roles from Json files
/// </summary>
public class RoleJsonIngestService : BaseJsonIngestService<Role, IRoleService>, IIngestService<Role, IRoleService>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoleJsonIngestService"/> class.
    /// </summary>
    /// <param name="service">Db repo from Role</param>
    /// <param name="config">JsonIngestConfig</param>
    public RoleJsonIngestService(IRoleService service, IOptions<JsonIngestConfig> config) : base(service, config)
    {
        LoadTranslations = true;
    }
}
