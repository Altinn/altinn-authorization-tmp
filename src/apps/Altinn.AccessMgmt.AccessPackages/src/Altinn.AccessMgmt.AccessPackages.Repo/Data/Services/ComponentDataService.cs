using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for Component
/// </summary>
public class ComponentDataService : BaseExtendedDataService<Component, ExtComponent>, IComponentService
{
    /// <summary>
    /// Data service for Component
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public ComponentDataService(IDbExtendedRepo<Component, ExtComponent> repo) : base(repo) 
    {
        ExtendedRepo.Join<Element>(t => t.ElementId, t => t.Id, t => t.Element);
    }
}
