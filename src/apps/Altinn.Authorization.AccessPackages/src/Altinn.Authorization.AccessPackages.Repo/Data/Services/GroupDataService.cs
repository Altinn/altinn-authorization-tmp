using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for EntityGroup
/// </summary>
public class GroupDataService : BaseExtendedDataService<EntityGroup, ExtEntityGroup>, IGroupService
{
    /// <summary>
    /// Data service for EntityGroup
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public GroupDataService(IDbExtendedRepo<EntityGroup, ExtEntityGroup> repo) : base(repo)
    {
        ExtendedRepo.Join<Entity>(t => t.OwnerId, t => t.Id, t => t.Owner);
    }
}
