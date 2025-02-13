using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for DelegationAssignmentPackage
/// </summary>
public class DelegationAssignmentPackageDataService : ExtendedRepository<DelegationAssignmentPackage, ExtDelegationAssignmentPackage>, IDelegationAssignmentPackageService
{
    /// <summary>
    /// Data service for DelegationAssignmentPackage
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public DelegationAssignmentPackageDataService(IDbExtendedRepo<DelegationAssignmentPackage, ExtDelegationAssignmentPackage> repo) : base(repo)
    //{
    //    Join<Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation);
    //    Join<AssignmentPackage>(t => t.AssignmentPackageId, t => t.Id, t => t.AssignmentPackage);
    //}
    public DelegationAssignmentPackageDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
