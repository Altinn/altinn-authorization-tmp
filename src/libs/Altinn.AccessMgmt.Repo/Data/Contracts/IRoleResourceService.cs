using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Contracts;

/// <inheritdoc/>
public interface IRoleResourceService : IDbCrossRepository<RoleResource, ExtRoleResource, Role, Resource> { }
