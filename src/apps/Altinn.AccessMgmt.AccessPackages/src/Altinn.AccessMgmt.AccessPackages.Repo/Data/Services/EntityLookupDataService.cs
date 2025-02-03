using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for EntityLookup
/// </summary>
public class EntityLookupDataService : BaseExtendedDataService<EntityLookup, ExtEntityLookup>, IEntityLookupService
{
    /// <summary>
    /// Data service for EntityLookup
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public EntityLookupDataService(IDbExtendedRepo<EntityLookup, ExtEntityLookup> repo) : base(repo)
    {
        ExtendedRepo.Join<EntityLookup>(t => t.EntityId, t => t.Id, t => t.Entity);
    }
}
