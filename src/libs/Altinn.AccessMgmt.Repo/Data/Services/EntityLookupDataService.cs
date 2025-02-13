using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

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
        Join<EntityLookup>(t => t.EntityId, t => t.Id, t => t.Entity);
    }
}
