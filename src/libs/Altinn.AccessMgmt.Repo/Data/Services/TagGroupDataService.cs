using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Services;
using Altinn.AccessMgmt.Models;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for TagGroup
/// </summary>
public class TagGroupDataService : BasicRepository<TagGroup>, ITagGroupService
{
    /// <summary>
    /// Data service for TagGroup
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public TagGroupDataService(IDbBasicRepo<TagGroup> repo) : base(repo) { }
    public TagGroupDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
