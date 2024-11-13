using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

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
        ExtendedRepo.Join<EntityType>(alias:"type", baseJoinProperty:"typeid");
        ExtendedRepo.Join<EntityVariant>(alias:"variant", baseJoinProperty: "variantid");
    }

    /// <inheritdoc/>
    public async Task<Entity?> GetByRefId(string refId, Guid typeId)
    {
        var res = await Get(new Dictionary<string, object>() { { "RefId", refId }, { "TypeId", typeId } });
        return res.FirstOrDefault();
    }
}