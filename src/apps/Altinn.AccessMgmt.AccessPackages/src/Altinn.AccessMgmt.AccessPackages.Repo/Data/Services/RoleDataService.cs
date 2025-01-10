using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for Role
/// </summary>
public class RoleDataService : BaseExtendedDataService<Role, ExtRole>, IRoleService
{
    /// <summary>
    /// Data service for Role
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public RoleDataService(IDbExtendedRepo<Role, ExtRole> repo) : base(repo)
    {
        ExtendedRepo.Join<Provider>(t => t.ProviderId, t => t.Id, t => t.Provider);
        ExtendedRepo.Join<EntityType>(t => t.EntityTypeId, t => t.Id, t => t.EntityType);
    }
}
