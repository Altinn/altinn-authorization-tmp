using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

/// <inheritdoc/>
public interface IEntityService : IDbExtendedDataService<Entity, ExtEntity>
{
    /// <summary>
    /// Get entity based on ref and type
    /// </summary>
    /// <param name="refId">Refrence</param>
    /// <param name="typeId">EntityType</param>
    /// <returns></returns>
    Task<Entity?> GetByRefId(string refId, Guid typeId);
}