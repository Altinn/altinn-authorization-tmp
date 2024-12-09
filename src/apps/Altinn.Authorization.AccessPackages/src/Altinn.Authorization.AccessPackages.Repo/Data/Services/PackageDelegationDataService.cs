using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

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
