using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

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