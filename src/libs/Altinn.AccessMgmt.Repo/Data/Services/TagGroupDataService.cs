using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for TagGroup
/// </summary>
public class TagGroupDataService : BaseDataService<TagGroup>, ITagGroupService
{
    /// <summary>
    /// Data service for TagGroup
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public TagGroupDataService(IDbBasicRepo<TagGroup> repo) : base(repo) { }
}
