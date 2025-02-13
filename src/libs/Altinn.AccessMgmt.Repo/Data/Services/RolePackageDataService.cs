using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

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
        Join<Role>(t => t.RoleId, t => t.Id, t => t.Role);
        Join<Package>(t => t.PackageId, t => t.Id, t => t.Package);
        Join<EntityVariant>(t => t.EntityVariantId, t => t.Id, t => t.EntityVariant, optional: true);
    }
}
