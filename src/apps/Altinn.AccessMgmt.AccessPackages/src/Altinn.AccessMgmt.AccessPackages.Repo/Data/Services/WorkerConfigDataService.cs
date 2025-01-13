using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for EntityGroup
/// </summary>
public class WorkerConfigDataService : BaseDataService<WorkerConfig>, IWorkerConfigService
{
    /// <summary>
    /// Data service for EntityGroup
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public WorkerConfigDataService(IDbBasicRepo<WorkerConfig> repo) : base(repo) { }
}
