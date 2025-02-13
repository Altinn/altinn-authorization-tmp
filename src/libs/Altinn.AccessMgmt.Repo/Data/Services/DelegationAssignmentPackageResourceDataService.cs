using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for DelegationAssignmentPackageResource
/// </summary>
public class DelegationAssignmentPackageResourceDataService : ExtendedRepository<DelegationAssignmentPackageResource, ExtDelegationAssignmentPackageResource>, IDelegationAssignmentPackageResourceService
{
    /// <summary>
    /// Data service for DelegationAssignmentPackageResource
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public DelegationAssignmentPackageResourceDataService(IDbExtendedRepo<DelegationAssignmentPackageResource, ExtDelegationAssignmentPackageResource> repo) : base(repo)
    //{
    //    Join<Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation);
    //    Join<AssignmentPackage>(t => t.AssignmentPackageId, t => t.Id, t => t.PackageResource);
    //    Join<PackageResource>(t => t.PackageResourceId, t => t.Id, t => t.PackageResource);
    //}
    public DelegationAssignmentPackageResourceDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
