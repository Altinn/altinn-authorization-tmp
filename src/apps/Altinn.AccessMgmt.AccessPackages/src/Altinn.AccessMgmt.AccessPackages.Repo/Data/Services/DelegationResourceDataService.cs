using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for DelegationPackageResource
/// </summary>
public class DelegationResourceDataService : BaseCrossDataService<Delegation, DelegationResource, Resource>, IDelegationResourceService
{
    /// <summary>
    /// Data service for Delegation
    /// </summary>
    /// <param name="repo">Cross repo</param>
    public DelegationResourceDataService(IDbCrossRepo<Delegation, DelegationResource, Resource> repo) : base(repo) { }
}
