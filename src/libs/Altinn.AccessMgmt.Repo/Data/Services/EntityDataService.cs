using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Models;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for Entity
/// </summary>
public class EntityDataService : BaseExtendedDataService<Entity, ExtEntity>, IEntityService
{
    /// <summary>
    /// Data service for Entity
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public EntityDataService(IDbExtendedRepo<Entity, ExtEntity> repo) : base(repo)
    {
        Join<EntityType>(t => t.TypeId, t => t.Id, t => t.Type);
        Join<EntityVariant>(t => t.VariantId, t => t.Id, t => t.Variant);
    }
}
