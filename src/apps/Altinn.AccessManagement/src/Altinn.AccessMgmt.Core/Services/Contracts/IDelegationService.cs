using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessMgmt.Core.Services.Contracts;

/// <summary>
/// Delegation service
/// </summary>
public interface IDelegationService
{
    /// <summary>
    /// Gets a delegation by id
    /// </summary>
    /// <returns></returns>
    Task<Delegation> GetDelegation(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get delegation
    /// </summary>
    /// <param name="fromId">From assignments from (e.g. Client)</param>
    /// <param name="toId">To assignments to (e.g. Agent)</param>
    /// <param name="viaId">Via (from assignments to and to assignments from)</param>
    /// <param name="fromRoleId">From assignment role</param>
    /// <param name="toRoleId">To assignment role</param>
    /// <param name="includePackages">Include packages</param>
    /// <param name="includePossibleDelegations">Include possible delegations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    Task<IEnumerable<RelationDto>> Get(
        Guid? fromId,
        Guid? toId,
        Guid? viaId,
        Guid? fromRoleId,
        Guid? toRoleId,
        bool includePackages = false,
        bool includePossibleDelegations = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Create a new delegation betweeen two assignments
    /// </summary>
    /// <param name="userId">User</param>
    /// <param name="fromAssignmentId">From</param>
    /// <param name="toAssignmentId">To</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    Task<Delegation> CreateDelgation(Guid userId, Guid fromAssignmentId, Guid toAssignmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a package to the delegation
    /// </summary>
    /// <returns></returns>
    Task<bool> AddPackageToDelegation(Guid userId, Guid delegationId, Guid packageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a resource to the delegation
    /// </summary>
    /// <returns></returns>
    Task<bool> AddResourceToDelegation(Guid userId, Guid delegationId, Guid resourceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a delegation and required assignments for system agent flow
    /// </summary>
    Task<IEnumerable<CreateDelegationResponseDto>> CreateClientDelegation(CreateSystemDelegationRequestDto request, Guid facilitatorPartyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a delegation and required assignments for imported clint roles from A2
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<CreateDelegationResponseDto>> ImportClientDelegation(ImportClientDelegationRequestDto request, AuditValues audit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a client delegation
    /// </summary>
    /// <returns></returns>
    Task<int> RevokeImportedClientDelegation(ImportClientDelegationRequestDto request, AuditValues audit, bool onlyRemoveA2 = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a delegation of the given id if found.
    /// </summary>
    Task<ProblemInstance> DeleteDelegation(Guid delegationId, CancellationToken cancellationToken = default);
}
