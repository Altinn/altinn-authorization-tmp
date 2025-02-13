using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for DelegationAssignmentResource
/// </summary>
public class DelegationAssignmentResourceDataService : BaseExtendedDataService<DelegationAssignmentResource, ExtDelegationAssignmentResource>, IDelegationAssignmentResourceService
{
    /// <summary>
    /// Data service for DelegationAssignmentResource
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public DelegationAssignmentResourceDataService(IDbExtendedRepo<DelegationAssignmentResource, ExtDelegationAssignmentResource> repo) : base(repo)
    {
        Join<Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation);
        Join<AssignmentResource>(t => t.AssignmentResourceId, t => t.Id, t => t.AssignmentResource);
    }
}
