using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for AssignmentResource
/// </summary>
public class AssignmentResourceDataService : ExtendedRepository<AssignmentResource, ExtAssignmentResource>, IAssignmentResourceService
{
    /// <summary>
    /// Data service for AssignmentResource
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public AssignmentResourceDataService(IDbExtendedRepo<AssignmentResource, ExtAssignmentResource> repo) : base(repo)
    //{
    //    Join<Assignment>(t => t.AssignmentId, t => t.Id, t => t.Assignment);
    //    Join<Resource>(t => t.ResourceId, t => t.Id, t => t.Resource);
    //}
    public AssignmentResourceDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
