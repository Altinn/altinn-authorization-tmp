using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for Policy
/// </summary>
public class PolicyDataService : ExtendedRepository<Policy, ExtPolicy>, IPolicyService
{
    /// <summary>
    /// Data service for Policy
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public PolicyDataService(IDbExtendedRepo<Policy, ExtPolicy> repo) : base(repo)
    //{
    //    Join<Resource>(t => t.ResourceId, t => t.Id, t => t.Resource);
    //}
    public PolicyDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
