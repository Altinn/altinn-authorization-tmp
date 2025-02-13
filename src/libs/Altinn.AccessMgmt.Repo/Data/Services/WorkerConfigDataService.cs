using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Services;
using Altinn.AccessMgmt.Models;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for EntityGroup
/// </summary>
public class WorkerConfigDataService : BasicRepository<WorkerConfig>, IWorkerConfigService
{
    /// <summary>
    /// Data service for EntityGroup
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public WorkerConfigDataService(IDbBasicRepo<WorkerConfig> repo) : base(repo) { }
    public WorkerConfigDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
