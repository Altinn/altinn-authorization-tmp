using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for GroupMember
/// </summary>
public class GroupMemberDataService : ExtendedRepository<GroupMember, ExtGroupMember>, IGroupMemberService
{
    /// <summary>
    /// Data service for GroupMember
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public GroupMemberDataService(IDbExtendedRepo<GroupMember, ExtGroupMember> repo) : base(repo)
    //{
    //    Join<EntityGroup>(t => t.GroupId, t => t.Id, t => t.Group);
    //    Join<Entity>(t => t.MemberId, t => t.Id, t => t.Member);
    //}
    public GroupMemberDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
