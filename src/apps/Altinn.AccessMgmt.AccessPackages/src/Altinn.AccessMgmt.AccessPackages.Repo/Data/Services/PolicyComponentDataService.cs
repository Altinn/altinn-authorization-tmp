using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for PolicyElement
/// </summary>
public class PolicyElementDataService : BaseCrossDataService<Policy, PolicyElement, Element>, IPolicyElementService
{
    /// <summary>
    /// Data service for PolicyElement
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public PolicyElementDataService(IDbCrossRepo<Policy, PolicyElement, Element> repo) : base(repo) { }
}
