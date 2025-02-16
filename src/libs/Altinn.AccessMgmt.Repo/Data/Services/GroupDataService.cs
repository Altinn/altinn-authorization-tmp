using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.Repo.Data.Contracts;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for EntityGroup
/// </summary>
public class GroupDataService : ExtendedRepository<EntityGroup, ExtEntityGroup>, IGroupService
{
    /// <inheritdoc/>
    public GroupDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
