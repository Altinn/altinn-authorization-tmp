using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for EntityLookup
/// </summary>
public class EntityLookupDataService : ExtendedRepository<EntityLookup, ExtEntityLookup>, IEntityLookupService
{
    /// <summary>
    /// Data service for EntityLookup
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public EntityLookupDataService(IDbExtendedRepo<EntityLookup, ExtEntityLookup> repo) : base(repo)
    //{
    //    Join<EntityLookup>(t => t.EntityId, t => t.Id, t => t.Entity);
    //}
    public EntityLookupDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
