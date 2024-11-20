using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

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
