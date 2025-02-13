using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Services;
using Altinn.AccessMgmt.Models;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for ResourceType
/// </summary>
public class ResourceTypeDataService : BasicRepository<ResourceType>, IResourceTypeService
{
    /// <summary>
    /// Data service for ResourceType
    /// </summary>
    /// <param name="repo">Repo</param>
    //public ResourceTypeDataService(IDbBasicRepo<ResourceType> repo) : base(repo) { }
    public ResourceTypeDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
