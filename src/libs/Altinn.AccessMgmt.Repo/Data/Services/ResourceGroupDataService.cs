using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for ResourceGroup
/// </summary>
public class ResourceGroupDataService : ExtendedRepository<ResourceGroup, ExtResourceGroup>, IResourceGroupService
{
    /// <summary>
    /// Data service for ResourceGroup
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public ResourceGroupDataService(IDbExtendedRepo<ResourceGroup, ExtResourceGroup> repo) : base(repo)
    //{
    //    Join<Provider>(t => t.ProviderId, t => t.Id, t => t.Provider);
    //}
    public ResourceGroupDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
