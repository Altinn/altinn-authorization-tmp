using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for Package
/// </summary>
public class PackageDataService : BaseExtendedDataService<Package, ExtPackage>, IPackageService
{
    /// <summary>
    /// Data service for Package
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public PackageDataService(IDbExtendedRepo<Package, ExtPackage> repo) : base(repo)
    {
        ExtendedRepo.Join<Provider>();
        ExtendedRepo.Join<Area>();
        ExtendedRepo.Join<EntityType>();
    }
}
