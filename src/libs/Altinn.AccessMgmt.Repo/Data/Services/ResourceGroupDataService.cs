using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for ResourceGroup
/// </summary>
public class ResourceGroupDataService : BaseExtendedDataService<ResourceGroup, ExtResourceGroup>, IResourceGroupService
{
    /// <summary>
    /// Data service for ResourceGroup
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public ResourceGroupDataService(IDbExtendedRepo<ResourceGroup, ExtResourceGroup> repo) : base(repo)
    {
        Join<Provider>(t => t.ProviderId, t => t.Id, t => t.Provider);
    }
}
