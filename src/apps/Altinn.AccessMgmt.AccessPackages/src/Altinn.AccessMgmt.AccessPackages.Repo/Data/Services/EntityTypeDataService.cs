using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for EntityType
/// </summary>
public class EntityTypeDataService : BaseExtendedDataService<EntityType, ExtEntityType>, IEntityTypeService
{
    /// <summary>
    /// Data service for EntityType
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public EntityTypeDataService(IDbExtendedRepo<EntityType, ExtEntityType> repo) : base(repo)
    {
        ExtendedRepo.Join<Provider>(t => t.ProviderId, t => t.Id, t => t.Provider);
    }
}
