using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;

/// <inheritdoc/>
public interface IDelegationPackageService : IDbCrossDataService<Delegation, DelegationPackage, Package> { }
