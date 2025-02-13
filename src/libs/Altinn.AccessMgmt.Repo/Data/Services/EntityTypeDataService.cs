using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for EntityType
/// </summary>
public class EntityTypeDataService : ExtendedRepository<EntityType, ExtEntityType>, IEntityTypeService
{
    /// <summary>
    /// Data service for EntityType
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public EntityTypeDataService(IDbExtendedRepo<EntityType, ExtEntityType> repo) : base(repo)
    //{
    //    Join<Provider>(t => t.ProviderId, t => t.Id, t => t.Provider);
    //}
    public EntityTypeDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
