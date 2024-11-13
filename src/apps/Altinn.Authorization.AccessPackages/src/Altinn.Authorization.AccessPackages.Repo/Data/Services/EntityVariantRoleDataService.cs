using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for EntityVariantRole
/// </summary>
public class EntityVariantRoleDataService : BaseCrossDataService<EntityVariant, EntityVariantRole, Role>, IEntityVariantRoleService
{
    /// <summary>
    /// Data service for EntityVariantRole
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public EntityVariantRoleDataService(IDbCrossRepo<EntityVariant, EntityVariantRole, Role> repo) : base(repo) 
    {
        CrossRepo.SetCrossColumns("variantid", "roleid");
    }
}
