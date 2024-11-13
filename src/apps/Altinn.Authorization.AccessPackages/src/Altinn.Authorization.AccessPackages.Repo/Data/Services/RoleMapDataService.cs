using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

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
        ExtendedRepo.Join<Role>("HasRole");
        ExtendedRepo.Join<Role>("GetRole");
    }
}