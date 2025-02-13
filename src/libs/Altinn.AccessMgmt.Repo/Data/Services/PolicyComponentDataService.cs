using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Services;
using Altinn.AccessMgmt.Models;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for PolicyElement
/// </summary>
public class PolicyElementDataService : CrossRepository<PolicyElement, ExtPolicyElement, Policy, Element>, IPolicyElementService
{
    /// <summary>
    /// Data service for PolicyElement
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public PolicyElementDataService(IDbCrossRepo<Policy, PolicyElement, Element> repo) : base(repo) { }
    public PolicyElementDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
