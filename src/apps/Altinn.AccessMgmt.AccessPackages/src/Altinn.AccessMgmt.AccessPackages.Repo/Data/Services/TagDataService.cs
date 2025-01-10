using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for Tag
/// </summary>
public class TagDataService : BaseExtendedDataService<Tag, ExtTag>, ITagService
{
    /// <summary>
    /// Data service for Tag
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public TagDataService(IDbExtendedRepo<Tag, ExtTag> repo) : base(repo)
    {
        ExtendedRepo.Join<TagGroup>(t => t.GroupId, t => t.Id, t => t.Group, optional: true);
        ExtendedRepo.Join<Tag>(t => t.ParentId, t => t.Id, t => t.Parent, optional: true);
    }
}
