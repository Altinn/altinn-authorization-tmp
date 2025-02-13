using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for AssignmentResource
/// </summary>
public class AssignmentResourceDataService : BaseExtendedDataService<AssignmentResource, ExtAssignmentResource>, IAssignmentResourceService
{
    /// <summary>
    /// Data service for AssignmentResource
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public AssignmentResourceDataService(IDbExtendedRepo<AssignmentResource, ExtAssignmentResource> repo) : base(repo)
    {
        Join<Assignment>(t => t.AssignmentId, t => t.Id, t => t.Assignment);
        Join<Resource>(t => t.ResourceId, t => t.Id, t => t.Resource);
    }
}
