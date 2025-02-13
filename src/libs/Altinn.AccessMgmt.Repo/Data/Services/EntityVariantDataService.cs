using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

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
        Join<EntityType>(t => t.TypeId, t => t.Id, t => t.Type);
    }
}
