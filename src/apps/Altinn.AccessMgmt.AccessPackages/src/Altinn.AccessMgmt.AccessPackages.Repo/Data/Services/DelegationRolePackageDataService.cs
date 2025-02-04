using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for DelegationRolePackage
/// </summary>
public class DelegationRolePackageDataService : BaseExtendedDataService<DelegationRolePackage, ExtDelegationRolePackage>, IDelegationRolePackageService
{
    /// <summary>
    /// Data service for DelegationRolePackage
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public DelegationRolePackageDataService(IDbExtendedRepo<DelegationRolePackage, ExtDelegationRolePackage> repo) : base(repo)
    {
        ExtendedRepo.Join<Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation);
        ExtendedRepo.Join<RolePackage>(t => t.RolePackageId, t => t.Id, t => t.RolePackage);
    }
}
