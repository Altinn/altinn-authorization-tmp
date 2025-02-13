using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for DelegationRoleResource
/// </summary>
public class DelegationRoleResourceDataService : ExtendedRepository<DelegationRoleResource, ExtDelegationRoleResource>, IDelegationRoleResourceService
{
    /// <summary>
    /// Data service for DelegationRoleResource
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public DelegationRoleResourceDataService(IDbExtendedRepo<DelegationRoleResource, ExtDelegationRoleResource> repo) : base(repo)
    //{
    //    Join<Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation);
    //    Join<RoleResource>(t => t.RoleResourceId, t => t.Id, t => t.RoleResource);
    //}
    public DelegationRoleResourceDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
