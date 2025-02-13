using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for RoleMap
/// </summary>
public class RoleMapDataService : ExtendedRepository<RoleMap, ExtRoleMap>, IRoleMapService
{
    /// <summary>
    /// Data service for RoleMap
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public RoleMapDataService(IDbExtendedRepo<RoleMap, ExtRoleMap> repo) : base(repo)
    //{
    //    Join<Role>(t => t.HasRoleId, t => t.Id, t => t.HasRole);
    //    Join<Role>(t => t.GetRoleId, t => t.Id, t => t.GetRole);
    //}
    public RoleMapDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
