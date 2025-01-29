using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for Element
/// </summary>
public class ElementDataService : BaseExtendedDataService<Element, ExtElement>, IElementService
{
    /// <summary>
    /// Data service for Element
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public ElementDataService(IDbExtendedRepo<Element, ExtElement> repo) : base(repo)
    {
        ExtendedRepo.Join<Resource>(t => t.ResourceId, t => t.Id, t => t.Resource);
        ExtendedRepo.Join<ElementType>(t => t.TypeId, t => t.Id, t => t.Type);
    }
}
