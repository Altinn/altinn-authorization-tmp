using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for DelegationGroup
/// </summary>
public class DelegationGroupDataService : BaseCrossDataService<Delegation, DelegationGroup, EntityGroup>, IDelegationGroupService
{
    /// <summary>
    /// Data service for Delegation
    /// </summary>
    /// <param name="repo">Cross repo</param>
    public DelegationGroupDataService(IDbCrossRepo<Delegation, DelegationGroup, EntityGroup> repo) : base(repo) { }
}
