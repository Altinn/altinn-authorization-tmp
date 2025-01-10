using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for PackageDelegation
/// </summary>
public class PackageDelegationDataService : BaseExtendedDataService<PackageDelegation, ExtPackageDelegation>, IPackageDelegationService
{
    /// <summary>
    /// Data service for PackageDelegation
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public PackageDelegationDataService(IDbExtendedRepo<PackageDelegation, ExtPackageDelegation> repo) : base(repo)
    {
        ExtendedRepo.Join<Entity>(t => t.ForId, t => t.Id, t => t.For);
        ExtendedRepo.Join<Entity>(t => t.ToId, t => t.Id, t => t.To);
        ExtendedRepo.Join<Package>(t => t.PackageId, t => t.Id, t => t.Package);
    }
}
