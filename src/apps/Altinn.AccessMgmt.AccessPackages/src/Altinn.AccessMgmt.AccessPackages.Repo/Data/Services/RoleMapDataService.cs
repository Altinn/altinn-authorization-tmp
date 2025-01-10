using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for RoleMap
/// </summary>
public class RoleMapDataService : BaseExtendedDataService<RoleMap, ExtRoleMap>, IRoleMapService
{
    /// <summary>
    /// Data service for RoleMap
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public RoleMapDataService(IDbExtendedRepo<RoleMap, ExtRoleMap> repo) : base(repo)
    {
        ExtendedRepo.Join<Role>(t => t.HasRoleId, t => t.Id, t => t.HasRole);
        ExtendedRepo.Join<Role>(t => t.GetRoleId, t => t.Id, t => t.GetRole);
    }
}
