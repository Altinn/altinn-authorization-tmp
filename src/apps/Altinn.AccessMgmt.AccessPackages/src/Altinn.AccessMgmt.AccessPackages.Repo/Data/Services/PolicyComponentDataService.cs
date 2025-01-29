using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for PolicyComponent
/// </summary>
public class PolicyComponentDataService : BaseCrossDataService<Policy, PolicyComponent, Component>, IPolicyComponentService
{
    /// <summary>
    /// Data service for PolicyComponent
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public PolicyComponentDataService(IDbCrossRepo<Policy, PolicyComponent, Component> repo) : base(repo) { }
}
