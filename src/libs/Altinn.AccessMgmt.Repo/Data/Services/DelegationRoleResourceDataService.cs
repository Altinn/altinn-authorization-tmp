using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for DelegationRoleResource
/// </summary>
public class DelegationRoleResourceDataService : BaseExtendedDataService<DelegationRoleResource, ExtDelegationRoleResource>, IDelegationRoleResourceService
{
    /// <summary>
    /// Data service for DelegationRoleResource
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public DelegationRoleResourceDataService(IDbExtendedRepo<DelegationRoleResource, ExtDelegationRoleResource> repo) : base(repo)
    {
        Join<Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation);
        Join<RoleResource>(t => t.RoleResourceId, t => t.Id, t => t.RoleResource);
    }
}
