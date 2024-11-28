using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for AssignmentDelegation
/// </summary>
public class AssignmentDelegationDataService : BaseExtendedDataService<AssignmentDelegation, ExtAssignmentDelegation>, IAssignmentDelegationService
{
    /// <summary>
    /// Data service for AssignmentDelegation
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public AssignmentDelegationDataService(IDbExtendedRepo<AssignmentDelegation, ExtAssignmentDelegation> repo) : base(repo)
    {
        ExtendedRepo.Join<Entity>(t => t.FromAssignmentId, t => t.Id, t => t.FromAssignment);
        ExtendedRepo.Join<Entity>(t => t.ToAssignmentId, t => t.Id, t => t.ToAssignment);
    }
}
