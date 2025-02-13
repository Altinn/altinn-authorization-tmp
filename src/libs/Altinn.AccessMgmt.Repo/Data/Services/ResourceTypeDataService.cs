using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for ResourceType
/// </summary>
public class ResourceTypeDataService : BaseDataService<ResourceType>, IResourceTypeService
{
    /// <summary>
    /// Data service for ResourceType
    /// </summary>
    /// <param name="repo">Repo</param>
    public ResourceTypeDataService(IDbBasicRepo<ResourceType> repo) : base(repo) { }
}
