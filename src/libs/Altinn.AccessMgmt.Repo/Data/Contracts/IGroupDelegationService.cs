using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Contracts;

/// <inheritdoc/>
public interface IGroupDelegationService : IDbExtendedRepository<GroupDelegation, ExtGroupDelegation> { }
