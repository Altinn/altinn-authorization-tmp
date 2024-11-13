using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for PackageTag
/// </summary>
public class PackageTagDataService : BaseCrossDataService<Package, PackageTag, Tag>, IPackageTagService
{
    /// <summary>
    /// Data service for PackageTag
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public PackageTagDataService(IDbCrossRepo<Package, PackageTag, Tag> repo) : base(repo) { }
}
