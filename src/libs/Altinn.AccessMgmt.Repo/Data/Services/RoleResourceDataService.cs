using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for RoleResource
/// </summary>
public class RoleResourceDataService : CrossRepository<RoleResource, ExtRoleResource, Role, Resource>, IRoleResourceService
{
    /// <summary>
    /// Data service for RoleResource
    /// </summary>
    /// <param name="repo">Cross repo</param>
    //public RoleResourceDataService(IDbCrossRepo<Role, RoleResource, Resource> repo) : base(repo) { }
    public RoleResourceDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
