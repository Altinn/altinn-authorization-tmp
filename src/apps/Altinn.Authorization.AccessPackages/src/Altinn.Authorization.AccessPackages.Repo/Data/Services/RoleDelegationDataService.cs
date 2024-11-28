using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for RoleDelegation
/// </summary>
public class RoleDelegationDataService : BaseExtendedDataService<RoleDelegation, ExtRoleDelegation>, IRoleDelegationService
{
    /// <summary>
    /// Data service for RoleDelegation
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public RoleDelegationDataService(IDbExtendedRepo<RoleDelegation, ExtRoleDelegation> repo) : base(repo)
    {
        ExtendedRepo.Join<Assignment>(t => t.AssignmentId, t => t.Id, t => t.Assignment);
        ExtendedRepo.Join<Role>(t => t.RoleId, t => t.Id, t => t.Role);
    }
}
