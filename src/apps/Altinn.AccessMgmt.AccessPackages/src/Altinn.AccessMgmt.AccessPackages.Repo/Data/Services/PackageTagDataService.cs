using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

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
