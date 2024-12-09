using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for Delegation
/// </summary>
public class DelegationDataService : BaseExtendedDataService<Delegation, ExtDelegation>, IDelegationService
{
    /// <summary>
    /// Data service for Delegation
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public DelegationDataService(IDbExtendedRepo<Delegation, ExtDelegation> repo) : base(repo)
    {
        ExtendedRepo.Join<Assignment>(t => t.AssignmentId, t => t.Id, t => t.Assignment);
    }
}
