using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for AssignmentPackage
/// </summary>
public class AssignmentPackageDataService : BaseExtendedDataService<AssignmentPackage, ExtAssignmentPackage>, IAssignmentPackageService
{
    /// <summary>
    /// Data service for AssignmentPackage
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public AssignmentPackageDataService(IDbExtendedRepo<AssignmentPackage, ExtAssignmentPackage> repo) : base(repo)
    {
        Join<Assignment>(t => t.AssignmentId, t => t.Id, t => t.Assignment);
        Join<Package>(t => t.PackageId, t => t.Id, t => t.Package);
    }
}
