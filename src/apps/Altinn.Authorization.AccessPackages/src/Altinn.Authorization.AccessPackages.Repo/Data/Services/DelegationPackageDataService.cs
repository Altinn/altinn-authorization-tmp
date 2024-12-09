using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for DelegationPackage
/// </summary>
public class DelegationPackageDataService : BaseCrossDataService<Delegation, DelegationPackage, Package>, IDelegationPackageService
{
    /// <summary>
    /// Data service for Delegation
    /// </summary>
    /// <param name="repo">Cross repo</param>
    public DelegationPackageDataService(IDbCrossRepo<Delegation, DelegationPackage, Package> repo) : base(repo) { }
}
