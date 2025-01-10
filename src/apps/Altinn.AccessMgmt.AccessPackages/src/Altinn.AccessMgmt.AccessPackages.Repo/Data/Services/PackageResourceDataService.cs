using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for PackageResource
/// </summary>
public class PackageResourceDataService : BaseExtendedDataService<PackageResource, ExtPackageResource>, IPackageResourceService
{
    /// <summary>
    /// Data service for PackageResource
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public PackageResourceDataService(IDbExtendedRepo<PackageResource, ExtPackageResource> repo) : base(repo)
    {
        ExtendedRepo.Join<Package>(t => t.PackageId, t => t.Id, t => t.Package);
        ExtendedRepo.Join<Resource>(t => t.ResourceId, t => t.Id, t => t.Resource);
    }
}
