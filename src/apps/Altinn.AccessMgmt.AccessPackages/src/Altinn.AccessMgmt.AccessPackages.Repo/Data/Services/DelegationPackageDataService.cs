using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

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
