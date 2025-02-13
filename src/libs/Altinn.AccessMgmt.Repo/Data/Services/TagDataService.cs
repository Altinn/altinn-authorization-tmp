using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for Tag
/// </summary>
public class TagDataService : ExtendedRepository<Tag, ExtTag>, ITagService
{
    /// <summary>
    /// Data service for Tag
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public TagDataService(IDbExtendedRepo<Tag, ExtTag> repo) : base(repo)
    //{
    //    Join<TagGroup>(t => t.GroupId, t => t.Id, t => t.Group, optional: true);
    //    Join<Tag>(t => t.ParentId, t => t.Id, t => t.Parent, optional: true);
    //}
    public TagDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
