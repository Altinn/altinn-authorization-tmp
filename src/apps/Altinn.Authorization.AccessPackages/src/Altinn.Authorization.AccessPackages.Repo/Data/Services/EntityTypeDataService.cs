using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

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
        ExtendedRepo.Join<Provider>();
    }
}
