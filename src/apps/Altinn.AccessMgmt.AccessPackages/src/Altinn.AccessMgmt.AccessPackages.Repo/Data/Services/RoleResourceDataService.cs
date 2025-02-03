using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for RoleResource
/// </summary>
public class RoleResourceDataService : BaseCrossDataService<Role, RoleResource, Resource>, IRoleResourceService
{
    /// <summary>
    /// Data service for RoleResource
    /// </summary>
    /// <param name="repo">Cross repo</param>
    public RoleResourceDataService(IDbCrossRepo<Role, RoleResource, Resource> repo) : base(repo) { }
}
