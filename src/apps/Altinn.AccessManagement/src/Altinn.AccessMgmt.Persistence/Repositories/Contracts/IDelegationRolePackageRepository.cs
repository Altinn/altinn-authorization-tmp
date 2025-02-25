using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;

namespace Altinn.AccessMgmt.Persistence.Repositories.Contracts;

/// <inheritdoc/>
public interface IDelegationRolePackageRepository : IDbExtendedRepository<DelegationRolePackage, ExtDelegationRolePackage> { }
