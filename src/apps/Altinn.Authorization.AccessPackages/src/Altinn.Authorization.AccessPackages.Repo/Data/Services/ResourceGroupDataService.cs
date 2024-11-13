using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

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
        ExtendedRepo.Join<Provider>();
    }
}
