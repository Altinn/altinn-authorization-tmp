using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for Assignment
/// </summary>
public class AssignmentDataService : BaseExtendedDataService<Assignment, ExtAssignment>, IAssignmentService
{
    /// <summary>
    /// Data service for Assignment
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public AssignmentDataService(IDbExtendedRepo<Assignment, ExtAssignment> repo) : base(repo)
    {
        Join<Role>(t => t.RoleId, t => t.Id, t => t.Role);
        Join<Entity>(t => t.FromId, t => t.Id, t => t.From);
        Join<Entity>(x => x.ToId, y => y.Id, z => z.To);
    }
}
