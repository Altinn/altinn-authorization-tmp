using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for Resource
/// </summary>
public class ResourceDataService : BaseExtendedDataService<Resource, ExtResource>, IResourceService
{
    /// <summary>
    /// Data service for Resource
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public ResourceDataService(IDbExtendedRepo<Resource, ExtResource> repo) : base(repo)
    {
        ExtendedRepo.Join<Provider>(t => t.ProviderId, t => t.Id, t => t.Provider);
        ExtendedRepo.Join<ResourceType>(t => t.TypeId, t => t.Id, t => t.Type);
        ExtendedRepo.Join<ResourceGroup>(t => t.GroupId, t => t.Id, t => t.Group);
    }
}
