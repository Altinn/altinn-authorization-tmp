using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

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
