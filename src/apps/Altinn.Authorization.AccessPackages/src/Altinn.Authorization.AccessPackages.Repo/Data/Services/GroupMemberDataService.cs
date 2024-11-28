using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for GroupMember
/// </summary>
public class GroupMemberDataService : BaseExtendedDataService<GroupMember, ExtGroupMember>, IGroupMemberService
{
    /// <summary>
    /// Data service for GroupMember
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public GroupMemberDataService(IDbExtendedRepo<GroupMember, ExtGroupMember> repo) : base(repo)
    {
        ExtendedRepo.Join<Group>(t => t.GroupId, t => t.Id, t => t.Group);
        ExtendedRepo.Join<Entity>(t => t.MemberId, t => t.Id, t => t.Member);
    }
}
