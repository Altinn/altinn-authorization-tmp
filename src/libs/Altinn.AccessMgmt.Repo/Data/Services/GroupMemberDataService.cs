using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

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
        Join<EntityGroup>(t => t.GroupId, t => t.Id, t => t.Group);
        Join<Entity>(t => t.MemberId, t => t.Id, t => t.Member);
    }
}
