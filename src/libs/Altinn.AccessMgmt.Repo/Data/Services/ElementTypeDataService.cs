using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for ElementType
/// </summary>
public class ElementTypeDataService : BaseDataService<ElementType>, IElementTypeService
{
    /// <summary>
    /// Data service for ElementType
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public ElementTypeDataService(IDbBasicRepo<ElementType> repo) : base(repo) { }
}
