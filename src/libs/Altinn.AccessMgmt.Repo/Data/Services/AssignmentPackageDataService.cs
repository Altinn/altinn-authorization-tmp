using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for AssignmentPackage
/// </summary>
public class AssignmentPackageDataService : ExtendedRepository<AssignmentPackage, ExtAssignmentPackage>, IAssignmentPackageService
{
    /// <summary>
    /// Data service for AssignmentPackage
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public AssignmentPackageDataService(IDbExtendedRepo<AssignmentPackage, ExtAssignmentPackage> repo) : base(repo)
    //{
    //    Join<Assignment>(t => t.AssignmentId, t => t.Id, t => t.Assignment);
    //    Join<Package>(t => t.PackageId, t => t.Id, t => t.Package);
    //}
    public AssignmentPackageDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
