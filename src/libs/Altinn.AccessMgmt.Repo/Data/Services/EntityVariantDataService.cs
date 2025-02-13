using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for EntityVariant
/// </summary>
public class EntityVariantDataService : ExtendedRepository<EntityVariant, ExtEntityVariant>, IEntityVariantService
{
    /// <summary>
    /// Data service for EntityVariant
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public EntityVariantDataService(IDbExtendedRepo<EntityVariant, ExtEntityVariant> repo) : base(repo)
    //{
    //    Join<EntityType>(t => t.TypeId, t => t.Id, t => t.Type);
    //}
    public EntityVariantDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
