using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Contracts;

/// <inheritdoc/>
public interface IPolicyElementService : IDbCrossRepository<PolicyElement, ExtPolicyElement, Policy, Element> { }
