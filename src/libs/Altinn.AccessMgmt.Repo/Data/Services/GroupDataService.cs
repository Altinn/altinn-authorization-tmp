using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for EntityGroup
/// </summary>
public class GroupDataService : ExtendedRepository<EntityGroup, ExtEntityGroup>, IGroupService
{
    /// <summary>
    /// Data service for EntityGroup
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public GroupDataService(IDbExtendedRepo<EntityGroup, ExtEntityGroup> repo) : base(repo)
    //{
    //    Join<Entity>(t => t.OwnerId, t => t.Id, t => t.Owner);
    //    Join<GroupAdmin>(t => t.Id, t => t.GroupId, t => t.Administrators, isList: true);
    //    Join<GroupMember>(t => t.Id, t => t.GroupId, t => t.Members, isList: true);
    //}
    public GroupDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
