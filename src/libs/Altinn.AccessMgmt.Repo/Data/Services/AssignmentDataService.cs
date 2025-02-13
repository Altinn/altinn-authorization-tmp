using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for Assignment
/// </summary>
public class AssignmentDataService : ExtendedRepository<Assignment, ExtAssignment>, IAssignmentService
{
    /// <summary>
    /// Data service for Assignment
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public AssignmentDataService(IDbExtendedRepo<Assignment, ExtAssignment> repo) : base(repo)
    //{
    //    Join<Role>(t => t.RoleId, t => t.Id, t => t.Role);
    //    Join<Entity>(t => t.FromId, t => t.Id, t => t.From);
    //    Join<Entity>(x => x.ToId, y => y.Id, z => z.To);
    //}
    public AssignmentDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
