using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for DelegationAssignmentPackageResource
/// </summary>
public class DelegationAssignmentPackageResourceDataService : BaseExtendedDataService<DelegationAssignmentPackageResource, ExtDelegationAssignmentPackageResource>, IDelegationAssignmentPackageResourceService
{
    /// <summary>
    /// Data service for DelegationAssignmentPackageResource
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public DelegationAssignmentPackageResourceDataService(IDbExtendedRepo<DelegationAssignmentPackageResource, ExtDelegationAssignmentPackageResource> repo) : base(repo)
    {
        ExtendedRepo.Join<Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation);
        ExtendedRepo.Join<AssignmentPackage>(t => t.AssignmentPackageId, t => t.Id, t => t.PackageResource);
        ExtendedRepo.Join<PackageResource>(t => t.PackageResourceId, t => t.Id, t => t.PackageResource);
    }
}
