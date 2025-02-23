using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.Repo.Contracts;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessMgmt.Repo.Services;

/// <summary>
/// Data service for EntityLookup
/// </summary>
public class EntityLookupDataService : ExtendedRepository<EntityLookup, ExtEntityLookup>, IEntityLookupService
{
    /// <inheritdoc/>
    public EntityLookupDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
