using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

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
        ExtendedRepo.Join<GroupAdmin>(t => t.Id, t => t.GroupId, t => t.Administrators, isList: true);
        ExtendedRepo.Join<GroupMember>(t => t.Id, t => t.GroupId, t => t.Members, isList: true);
    }
}
