using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for EntityVariant
/// </summary>
public class EntityVariantDataService : BaseExtendedDataService<EntityVariant, ExtEntityVariant>, IEntityVariantService
{
    /// <summary>
    /// Data service for EntityVariant
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public EntityVariantDataService(IDbExtendedRepo<EntityVariant, ExtEntityVariant> repo) : base(repo)
    {
        ExtendedRepo.Join<EntityType>("Type");
    }
}
