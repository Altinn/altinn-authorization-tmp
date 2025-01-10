using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

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
