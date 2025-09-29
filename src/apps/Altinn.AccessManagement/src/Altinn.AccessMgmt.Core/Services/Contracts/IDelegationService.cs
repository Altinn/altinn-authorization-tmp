﻿using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;

namespace Altinn.AccessMgmt.Core.Services.Contracts;

/// <summary>
/// Delegation service
/// </summary>
public interface IDelegationService
{
    Task<Delegation> GetDelegation(Guid id, CancellationToken cancellationToken = default);

    Task<Delegation> GetDelegation(Guid fromId, Guid toId, Guid roleId, Guid viaRoleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new delegation betweeen two assignments
    /// </summary>
    /// <param name="userId">User</param>
    /// <param name="fromAssignmentId">From</param>
    /// <param name="toAssignmentId">To</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    Task<Delegation> CreateDelegation(Guid userId, Guid fromAssignmentId, Guid toAssignmentId, CancellationToken cancellationToken);

    Task<DelegationPackage> GetOrAddPackage(Guid partyId, Guid fromId, Guid toId, Guid roleId, Guid viaId, Guid viaRoleId, Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a package to the delegation
    /// </summary>
    /// <returns></returns>
    Task<bool> AddPackageToDelegation(Guid userId, Guid delegationId, Guid packageId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a resource to the delegation
    /// </summary>
    /// <returns></returns>
    Task<bool> AddResourceToDelegation(Guid userId, Guid delegationId, Guid resourceId, CancellationToken cancellationToken);

    /// <summary>
    /// Create a delegation and required assignments for system agent flow
    /// </summary>
    Task<IEnumerable<Delegation>> CreateClientDelegation(CreateSystemDelegationRequestDto request, Guid facilitatorPartyId, CancellationToken cancellationToken);
}
