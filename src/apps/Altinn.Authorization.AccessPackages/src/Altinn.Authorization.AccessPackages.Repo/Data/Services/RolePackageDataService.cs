using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for RolePackage
/// </summary>
public class RolePackageDataService : BaseExtendedDataService<RolePackage, ExtRolePackage>, IRolePackageService
{
    /// <summary>
    /// Data service for RolePackage
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public RolePackageDataService(IDbExtendedRepo<RolePackage, ExtRolePackage> repo) : base(repo)
    {
        ExtendedRepo.Join<Role>();
        ExtendedRepo.Join<Package>();
        ExtendedRepo.Join<EntityVariant>(optional: true);
    }
}
