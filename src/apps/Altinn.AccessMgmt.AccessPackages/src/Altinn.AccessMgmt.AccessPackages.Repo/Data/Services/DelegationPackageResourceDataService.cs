using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for DelegationPackageResource
/// </summary>
public class DelegationPackageResourceDataService : BaseCrossDataService<Delegation, DelegationPackageResource, PackageResource>, IDelegationPackageResourceService
{
    /// <summary>
    /// Data service for Delegation
    /// </summary>
    /// <param name="repo">Cross repo</param>
    public DelegationPackageResourceDataService(IDbCrossRepo<Delegation, DelegationPackageResource, PackageResource> repo) : base(repo) { }
}
