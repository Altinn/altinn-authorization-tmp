using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for Role
/// </summary>
public class RoleDataService : ExtendedRepository<Role, ExtRole>, IRoleService
{
    /// <summary>
    /// Data service for Role
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public RoleDataService(IDbExtendedRepo<Role, ExtRole> repo) : base(repo)
    //{
    //    Join<Provider>(t => t.ProviderId, t => t.Id, t => t.Provider);
    //    Join<EntityType>(t => t.EntityTypeId, t => t.Id, t => t.EntityType);
    //}
    public RoleDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
