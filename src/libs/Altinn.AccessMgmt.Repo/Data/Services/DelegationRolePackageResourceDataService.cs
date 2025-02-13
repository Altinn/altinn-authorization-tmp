using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for DelegationRolePackageResource
/// </summary>
public class DelegationRolePackageResourceDataService : BaseExtendedDataService<DelegationRolePackageResource, ExtDelegationRolePackageResource>, IDelegationRolePackageResourceService
{
    /// <summary>
    /// Data service for DelegationRolePackageResource
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public DelegationRolePackageResourceDataService(IDbExtendedRepo<DelegationRolePackageResource, ExtDelegationRolePackageResource> repo) : base(repo)
    {
        Join<Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation);
        Join<RolePackage>(t => t.RolePackageId, t => t.Id, t => t.PackageResource);
        Join<PackageResource>(t => t.PackageResourceId, t => t.Id, t => t.PackageResource);
    }
}
