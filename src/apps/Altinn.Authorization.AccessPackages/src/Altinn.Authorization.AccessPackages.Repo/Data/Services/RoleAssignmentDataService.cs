using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for RoleAssignment
/// </summary>
public class RoleAssignmentDataService : BaseExtendedDataService<RoleAssignment, ExtRoleAssignment>, IRoleAssignmentService
{
    /// <summary>
    /// Data service for RoleAssignment
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public RoleAssignmentDataService(IDbExtendedRepo<RoleAssignment, ExtRoleAssignment> repo) : base(repo)
    {
        ExtendedRepo.Join<Role>(t => t.RoleId, t => t.Id, t => t.Role);
        ExtendedRepo.Join<Entity>(t => t.ForId, t => t.Id, t => t.For);
        ExtendedRepo.Join<Entity>(t => t.ToId, t => t.Id, t => t.To);
    }
}
