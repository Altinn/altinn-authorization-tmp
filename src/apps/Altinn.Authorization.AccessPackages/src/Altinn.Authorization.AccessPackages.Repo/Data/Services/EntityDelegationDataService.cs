using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for EntityDelegation
/// </summary>
public class EntityDelegationDataService : BaseExtendedDataService<EntityDelegation, ExtEntityDelegation>, IEntityDelegationService
{
    /// <summary>
    /// Data service for EntityDelegation
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public EntityDelegationDataService(IDbExtendedRepo<EntityDelegation, ExtEntityDelegation> repo) : base(repo)
    {
        ExtendedRepo.Join<Assignment>(t => t.AssignmentId, t => t.Id, t => t.Assignment);
        ExtendedRepo.Join<Entity>(t => t.EntityId, t => t.Id, t => t.Entity);
    }
}
