using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

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
