using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for DelegationRolePackage
/// </summary>
public class DelegationRolePackageDataService : ExtendedRepository<DelegationRolePackage, ExtDelegationRolePackage>, IDelegationRolePackageService
{
    /// <summary>
    /// Data service for DelegationRolePackage
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public DelegationRolePackageDataService(IDbExtendedRepo<DelegationRolePackage, ExtDelegationRolePackage> repo) : base(repo)
    //{
    //    Join<Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation);
    //    Join<RolePackage>(t => t.RolePackageId, t => t.Id, t => t.RolePackage);
    //}
    public DelegationRolePackageDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
