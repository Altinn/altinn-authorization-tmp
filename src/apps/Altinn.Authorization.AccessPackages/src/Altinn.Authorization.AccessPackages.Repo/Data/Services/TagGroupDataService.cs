using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

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
