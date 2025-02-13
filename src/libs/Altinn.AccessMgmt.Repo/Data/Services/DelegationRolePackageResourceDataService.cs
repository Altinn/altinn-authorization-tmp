using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for DelegationRolePackageResource
/// </summary>
public class DelegationRolePackageResourceDataService : ExtendedRepository<DelegationRolePackageResource, ExtDelegationRolePackageResource>, IDelegationRolePackageResourceService
{
    /// <summary>
    /// Data service for DelegationRolePackageResource
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public DelegationRolePackageResourceDataService(IDbExtendedRepo<DelegationRolePackageResource, ExtDelegationRolePackageResource> repo) : base(repo)
    //{
    //    Join<Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation);
    //    Join<RolePackage>(t => t.RolePackageId, t => t.Id, t => t.PackageResource);
    //    Join<PackageResource>(t => t.PackageResourceId, t => t.Id, t => t.PackageResource);
    //}
    public DelegationRolePackageResourceDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
