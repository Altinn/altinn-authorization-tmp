using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Microsoft.Extensions.Logging;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for EntityGroup
/// </summary>
public class AreaGroupDataService : ExtendedRepository<AreaGroup, ExtAreaGroup>, IAreaGroupService
{
    /// <summary>
    /// Data service for EntityGroup
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public AreaGroupDataService(IDbExtendedRepo<AreaGroup, ExtAreaGroup> repo) : base(repo) 
    //{
    //    Join<EntityType>(t => t.EntityTypeId, t => t.Id, t => t.EntityType);
    //}
    public AreaGroupDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
