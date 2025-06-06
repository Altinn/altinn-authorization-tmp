﻿using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;
using Altinn.AccessMgmt.Persistence.Core.Services;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;

namespace Altinn.AccessMgmt.Persistence.Repositories;

/// <summary>
/// Data service for DelegationPackage
/// </summary>
public class DelegationPackageRepository : CrossRepository<DelegationPackage, ExtDelegationPackage, Delegation, Package>, IDelegationPackageRepository
{
    /// <inheritdoc/>
    public DelegationPackageRepository(DbDefinitionRegistry dbDefinitionRegistry, IDbExecutor executor) : base(dbDefinitionRegistry, executor)
    {
    }
}
