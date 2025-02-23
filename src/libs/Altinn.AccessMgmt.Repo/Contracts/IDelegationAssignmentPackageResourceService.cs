using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Contracts;

/// <inheritdoc/>
public interface IDelegationAssignmentPackageResourceService : IDbExtendedRepository<DelegationAssignmentPackageResource, ExtDelegationAssignmentPackageResource> { }
