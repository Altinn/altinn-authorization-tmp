using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for DelegationGroup
/// </summary>
public class DelegationGroupDataService : BaseCrossDataService<Delegation, DelegationGroup, EntityGroup>, IDelegationGroupService
{
    /// <summary>
    /// Data service for Delegation
    /// </summary>
    /// <param name="repo">Cross repo</param>
    public DelegationGroupDataService(IDbCrossRepo<Delegation, DelegationGroup, EntityGroup> repo) : base(repo)
    {
        CrossRepo.SetCrossColumns("delegationid", "groupid");
    }
}
