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
        ExtendedRepo.Join<Provider>(t => t.ProviderId, t => t.Id, t => t.Provider);
        ExtendedRepo.Join<Area>(t => t.AreaId, t => t.Id, t => t.Area);
        ExtendedRepo.Join<EntityType>(t => t.EntityTypeId, t => t.Id, t => t.EntityType);
    }
}
