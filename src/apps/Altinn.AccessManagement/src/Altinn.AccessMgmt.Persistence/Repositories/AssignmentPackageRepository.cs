﻿using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Definitions;
using Altinn.AccessMgmt.Persistence.Core.Executors;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Services;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.AccessMgmt.Persistence.Repositories;

/// <summary>
/// Data service for AssignmentPackage
/// </summary>
public class AssignmentPackageRepository : CrossRepository<AssignmentPackage, ExtAssignmentPackage, Assignment, Package>, IAssignmentPackageRepository
{
    /// <inheritdoc/>
    public AssignmentPackageRepository(DbDefinitionRegistry dbDefinitionRegistry, IDbExecutor executor) : base(dbDefinitionRegistry, executor)
    {
    }
}

/// <summary>
/// Data service for AssignmentPackage
/// </summary>
public class ConnectionPackageRepository : CrossRepository<ConnectionPackage, ExtConnectionPackage, Connection, Package>, IConnectionPackageRepository
{
    /// <inheritdoc/>
    public ConnectionPackageRepository(DbDefinitionRegistry dbDefinitionRegistry, IDbExecutor executor) : base(dbDefinitionRegistry, executor)
    {
    }
}
