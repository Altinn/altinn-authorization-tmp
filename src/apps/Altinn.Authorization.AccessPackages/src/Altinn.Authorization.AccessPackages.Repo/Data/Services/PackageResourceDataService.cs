using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

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
        ExtendedRepo.Join<Package>();
        ExtendedRepo.Join<Resource>();
    }
}