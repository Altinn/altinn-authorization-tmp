﻿using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for Assignment
/// </summary>
public class AssignmentDataService : BaseExtendedDataService<Assignment, ExtAssignment>, IAssignmentService
{
    /// <summary>
    /// Data service for Assignment
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public AssignmentDataService(IDbExtendedRepo<Assignment, ExtAssignment> repo) : base(repo)
    {
        ExtendedRepo.Join<Role>(t => t.RoleId, t => t.Id, t => t.Role);
        ExtendedRepo.Join<Entity>(t => t.FromId, t => t.Id, t => t.From);
        ExtendedRepo.Join<Entity>(x => x.ToId, y => y.Id, z => z.To);
    }
}