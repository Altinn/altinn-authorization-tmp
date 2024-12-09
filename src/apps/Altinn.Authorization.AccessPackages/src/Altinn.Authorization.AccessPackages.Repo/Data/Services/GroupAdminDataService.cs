using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for GroupAdmin
/// </summary>
public class GroupAdminDataService : BaseExtendedDataService<GroupAdmin, ExtGroupAdmin>, IGroupAdminService
{
    /// <summary>
    /// Data service for GroupAdmin
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public GroupAdminDataService(IDbExtendedRepo<GroupAdmin, ExtGroupAdmin> repo) : base(repo)
    {
        ExtendedRepo.Join<EntityGroup>(t => t.GroupId, t => t.Id, t => t.Group);
        ExtendedRepo.Join<Entity>(t => t.MemberId, t => t.Id, t => t.Member);
    }
}
