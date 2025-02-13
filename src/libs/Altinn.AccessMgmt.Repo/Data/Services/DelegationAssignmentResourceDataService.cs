using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for DelegationAssignmentResource
/// </summary>
public class DelegationAssignmentResourceDataService : ExtendedRepository<DelegationAssignmentResource, ExtDelegationAssignmentResource>, IDelegationAssignmentResourceService
{
    /// <summary>
    /// Data service for DelegationAssignmentResource
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public DelegationAssignmentResourceDataService(IDbExtendedRepo<DelegationAssignmentResource, ExtDelegationAssignmentResource> repo) : base(repo)
    //{
    //    Join<Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation);
    //    Join<AssignmentResource>(t => t.AssignmentResourceId, t => t.Id, t => t.AssignmentResource);
    //}
    public DelegationAssignmentResourceDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
