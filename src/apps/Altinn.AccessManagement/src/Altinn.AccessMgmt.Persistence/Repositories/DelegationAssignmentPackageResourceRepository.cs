using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Definitions;
using Altinn.AccessMgmt.Persistence.Core.Executors;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Services;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.AccessMgmt.Persistence.Repositories;

/// <summary>
/// Data service for DelegationAssignmentPackageResource
/// </summary>
public class DelegationAssignmentPackageResourceRepository : ExtendedRepository<DelegationAssignmentPackageResource, ExtDelegationAssignmentPackageResource>, IDelegationAssignmentPackageResourceRepository
{
    /// <inheritdoc/>
    public DelegationAssignmentPackageResourceRepository(IOptions<DbAccessConfig> options, DbDefinitionRegistry dbDefinitionRegistry, IDbExecutor executor) : base(options, dbDefinitionRegistry, executor)
    {
    }
}
