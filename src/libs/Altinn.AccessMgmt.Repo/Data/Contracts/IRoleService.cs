using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Contracts;

/// <inheritdoc/>
public interface IRoleService : IDbExtendedDataService<Role, ExtRole> { }
