using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for DelegationAssignment
/// </summary>
public class DelegationAssignmentDataService : BaseCrossDataService<Delegation, DelegationAssignment, Assignment>, IDelegationAssignmentService
{
    /// <summary>
    /// Data service for Delegation
    /// </summary>
    /// <param name="repo">Cross repo</param>
    public DelegationAssignmentDataService(IDbCrossRepo<Delegation, DelegationAssignment, Assignment> repo) : base(repo) { }
}
