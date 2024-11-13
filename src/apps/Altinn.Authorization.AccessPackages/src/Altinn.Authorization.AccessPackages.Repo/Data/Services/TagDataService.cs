using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

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
        ExtendedRepo.Join<TagGroup>("Group", optional: true);
        ExtendedRepo.Join<Tag>("Parent", optional: true);
    }
}
