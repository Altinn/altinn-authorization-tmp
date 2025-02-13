using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for DelegationAssignmentPackage
/// </summary>
public class DelegationAssignmentPackageDataService : BaseExtendedDataService<DelegationAssignmentPackage, ExtDelegationAssignmentPackage>, IDelegationAssignmentPackageService
{
    /// <summary>
    /// Data service for DelegationAssignmentPackage
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public DelegationAssignmentPackageDataService(IDbExtendedRepo<DelegationAssignmentPackage, ExtDelegationAssignmentPackage> repo) : base(repo)
    {
        Join<Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation);
        Join<AssignmentPackage>(t => t.AssignmentPackageId, t => t.Id, t => t.AssignmentPackage);
    }
}
