using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for Group
/// </summary>
public class GroupDataService : BaseExtendedDataService<Group, ExtGroup>, IGroupService
{
    /// <summary>
    /// Data service for Group
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public GroupDataService(IDbExtendedRepo<Group, ExtGroup> repo) : base(repo)
    {
        ExtendedRepo.Join<Entity>(t => t.OwnerId, t => t.Id, t => t.Owner);
    }
}
