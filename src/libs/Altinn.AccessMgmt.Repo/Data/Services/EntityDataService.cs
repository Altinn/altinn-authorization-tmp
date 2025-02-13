using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for Entity
/// </summary>
public class EntityDataService : ExtendedRepository<Entity, ExtEntity>, IEntityService
{
    /// <summary>
    /// Data service for Entity
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public EntityDataService(IDbExtendedRepo<Entity, ExtEntity> repo) : base(repo)
    //{
    //    Join<EntityType>(t => t.TypeId, t => t.Id, t => t.Type);
    //    Join<EntityVariant>(t => t.VariantId, t => t.Id, t => t.Variant);
    //}
    public EntityDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
