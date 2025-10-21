using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Legacy;

namespace Altinn.AccessMgmt.Core.Services.Legacy;

public interface IDelegationChangesService
{
    /// <summary>
    /// Writes the delegation change metadata to the delegation database
    /// </summary>
    /// <param name="delegationChange">The DelegationChange model describing the delegation, to insert in the database</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The complete DelegationChange record stored in the database</returns>
    Task<DelegationChanges> InsertDelegationAltinnApp(DelegationChanges delegationChange, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the delegation change metadata to the delegation database
    /// </summary>
    /// <param name="delegationChange">The DelegationChange model describing the delegation, to insert in the database</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The complete DelegationChange record stored in the database</returns>
    Task<DelegationChanges> InsertDelegation(DelegationChanges delegationChange, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetch all the latest Instance delegations for a given instance
    /// </summary>
    /// <param name="source">The source to fetch delegations for</param>
    /// <param name="resourceID">The resource to fetch delegations for</param>
    /// <param name="instanceID">The instance to fetch delegations for </param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>All the last InstanceDelegationChange records stored in the database corresponding to the request</returns>
    Task<List<InstanceDelegationChanges>> GetAllLatestInstanceDelegationChanges(InstanceDelegationSource source, string resourceID, string instanceID, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all received instance delegations from db  
    /// </summary>
    /// <param name="toUuid">party uuid that received the delegation</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns></returns>
    Task<List<InstanceDelegationChanges>> GetAllCurrentReceivedInstanceDelegations(List<Guid> toUuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the last change from db to fetch the current policy version and path to policy file
    /// </summary>
    /// <param name="request">The parameters to request the latest change for</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The last InstanceDelegationChange record stored in the database corresponding to the request</returns>
    Task<InstanceDelegationChanges> GetLastInstanceDelegationChange(InstanceDelegationChanges request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes the delegation change metadata to the delegation database
    /// </summary>
    /// <param name="instanceDelegationChange">The InstanceDelegationChange model describing the delegation, to insert in the database</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The complete InstanceDelegationChange record stored in the database</returns>
    Task<InstanceDelegationChanges> InsertInstanceDelegation(InstanceDelegationChanges instanceDelegationChange, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes multiple delegation change metadata to the delegation database
    /// </summary>
    /// <param name="policyWriteOutputs">List of policies changed</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns></returns>
    Task<bool> InsertMultipleInstanceDelegations(List<PolicyWriteOutput> policyWriteOutputs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all the currently active instance delegations existing between the from and to parties
    /// </summary>
    /// <param name="resourceIds">Collection of all resourceIds to lookup</param>
    /// <param name="from">The From party to use for lookup</param>
    /// <param name="to">All To parties to use for lookup</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>The complete InstanceDelegationChange record stored in the database</returns>
    Task<IEnumerable<InstanceDelegationChanges>> GetActiveInstanceDelegations(List<string> resourceIds, Guid from, List<Guid> to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest delegation change matching the filter values
    /// </summary>
    /// <param name="resourceId">The resourceId to look up. Either Altinn app id (org/app) or resource registry id</param>
    /// <param name="offeredByPartyId">The party id of the entity offering the delegated the policy</param>
    /// <param name="coveredByPartyId">The party id of the entity having received the delegated policy, if the entity is an organization</param>
    /// <param name="coveredByUserId">The user id of the entity having received the delegated policy, if the entity is a user</param>
    /// <param name="toUuid">The receiver uuid</param>
    /// <param name="toUuidType">The type of uuid the reciver is</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    Task<DelegationChanges> GetCurrentDelegationChange(string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, Guid? toUuid, UuidType toUuidType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the latest delegation change matching the filter values
    /// </summary>
    /// <param name="resourceId">The resourceId to look up. Either Altinn app id (org/app) or resource registry id</param>
    /// <param name="offeredByPartyId">The party id of the entity offering the delegated the policy</param>
    /// <param name="coveredByPartyId">The party id of the entity having received the delegated policy, if the entity is an organization</param>
    /// <param name="coveredByUserId">The user id of the entity having received the delegated policy, if the entity is a user</param>
    /// <param name="toUuid">The receiver uuid</param>
    /// <param name="toUuidType">The type of uuid the reciver is</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    Task<DelegationChanges> GetCurrentDelegationChangeAltinnApp(string resourceId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, Guid? toUuid, UuidType toUuidType, CancellationToken cancellationToken = default);


    /// <summary>
    /// Gets all the delegation change records matching the filter values for a complete changelog
    /// </summary>
    /// <param name="altinnAppId">The Altinn app id (org/app)</param>
    /// <param name="offeredByPartyId">The party id of the entity offering the delegated the policy</param>
    /// <param name="coveredByPartyId">The party id of the entity having received the delegated policy, if the entity is an organization</param>
    /// <param name="coveredByUserId">The user id of the entity having received the delegated policy, if the entity is a user</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    Task<List<DelegationChanges>> GetAllAppDelegationChanges(string altinnAppId, int offeredByPartyId, int? coveredByPartyId, int? coveredByUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all the current app delegation change records matching the filter values
    /// </summary>
    /// <param name="offeredByPartyIds">The list of party id of the entity offering the delegated the policy</param>
    /// <param name="altinnAppIds">The list of altinn app ids to look up</param>
    /// <param name="coveredByPartyIds">The list of party id of the entity having received the delegated policy, if the entity is an organization</param>
    /// <param name="coveredByUserIds">The list of user id of the entity having received the delegated policy, if the entity is a user</param>
    /// <param name="cancellationToken">Cancellation token for cancelling the request</param>
    Task<List<DelegationChanges>> GetAllCurrentAppDelegationChanges(List<int> offeredByPartyIds, List<string> altinnAppIds, List<int> coveredByPartyIds = null, List<int> coveredByUserIds = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all the current altinn app delegation change records matching the filter values
    /// </summary>
    /// <param name="altinnAppIds">The list of altinn app IDs to look up delegations of</param>
    /// <param name="fromPartyIds">The list of from parties having delegated resources</param>
    /// <param name="toUuidType">The type of the to uuid recipient of delegated resources</param>
    /// <param name="toUuid">The uuid of the recipient of delegated resources</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    Task<List<DelegationChanges>> GetAllCurrentAppDelegationChanges(List<string> altinnAppIds, List<int> fromPartyIds, string toUuidType, Guid toUuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all the current resource registry delegation change records matching the filter values
    /// </summary>
    /// <param name="offeredByPartyIds">The list of party id of the entity offering the delegated the policy</param>
    /// <param name="resourceRegistryIds">The list of resource registry ids to look up</param>
    /// <param name="coveredByPartyIds">The list of party id of the entity having received the delegated policy, if the entity is an organization</param>
    /// <param name="coveredByUserId">The user id of the user having received the delegated policy</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    Task<List<DelegationChanges>> GetAllCurrentResourceRegistryDelegationChanges(List<int> offeredByPartyIds, List<string> resourceRegistryIds, List<int> coveredByPartyIds = null, int? coveredByUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all the current resource registry delegation change records matching the filter values
    /// </summary>
    /// <param name="resourceRegistryIds">The list of resource registry IDs to look up delegations of</param>
    /// <param name="fromPartyIds">The list of from parties having delegated resources</param>
    /// <param name="toUuidType">The type of the to uuid recipient of delegated resources</param>
    /// <param name="toUuid">The uuid of the recipient of delegated resources</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    Task<List<DelegationChanges>> GetAllCurrentResourceRegistryDelegationChanges(List<string> resourceRegistryIds, List<int> fromPartyIds, string toUuidType, Guid toUuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all the active resource registry delegations a given party have delegated to others
    /// </summary>
    /// <param name="offeredByPartyId">The party id of the entity offering the delegations</param>
    /// <param name="resourceRegistryIds">The resource registry ids of resources to find delegations of</param>
    /// <param name="resourceTypes">The types of resources to find delegations of</param>
    /// <param name="cancellationToken">Cancellation token for cancelling the request</param>
    Task<List<DelegationChanges>> GetOfferedResourceRegistryDelegations(int offeredByPartyId, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all the active delegation given party has offered to others
    /// </summary>
    /// <param name="offeredByPartyIds">a</param>
    /// <param name="cancellationToken">b</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<List<DelegationChanges>> GetOfferedDelegations(List<int> offeredByPartyIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all the active resource registry delegations a list of parties have received from others
    /// </summary>
    /// <param name="coveredByPartyIds">The list of party ids of the entities receiving the delegations</param>
    /// <param name="offeredByPartyIds">The list of party ids of the entities offering the delegations</param>
    /// <param name="resourceRegistryIds">The resource registry ids of resources to find delegations of</param>
    /// <param name="resourceTypes">The types of resources to find delegations of</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    Task<List<DelegationChanges>> GetReceivedResourceRegistryDelegationsForCoveredByPartys(List<int> coveredByPartyIds, List<int> offeredByPartyIds = null, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all the active resource registry delegations a given user have received from others
    /// </summary>
    /// <param name="coveredByUserId">The user id of the entity that received the delegation</param>
    /// <param name="offeredByPartyIds">The party ids of the entities offering the delegations</param>
    /// <param name="resourceRegistryIds">The resource registry ids of resources to find delegations of</param>
    /// <param name="resourceTypes">The types of resources to find delegations of</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    Task<List<DelegationChanges>> GetReceivedResourceRegistryDelegationsForCoveredByUser(int coveredByUserId, List<int> offeredByPartyIds, List<string> resourceRegistryIds = null, List<ResourceType> resourceTypes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the delgations for a given supplier, consumer and resourcetype based on resourceids
    /// </summary>
    /// <param name="resourceIds">the resource ids</param>
    /// <param name="offeredByPartyId">the party id of the entity that offered the delegation</param>
    /// <param name="coveredByPartyId">The party id of the entity that received the delegation</param>
    /// <param name="resourceType">the type of resource</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    Task<List<DelegationChanges>> GetResourceRegistryDelegationChanges(List<string> resourceIds, int offeredByPartyId, int coveredByPartyId, ResourceType resourceType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all the active app or resource delegations (except MaskinportenSchema delegations) for the set of covered userIds or partyIds
    /// </summary>
    /// <param name="coveredByUserIds">The user ids of the users to get received delegation for</param>
    /// <param name="coveredByPartyIds">The party ids of the organizations to get received delegation for</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    Task<List<DelegationChanges>> GetAllDelegationChangesForAuthorizedParties(List<int> coveredByUserIds, List<int> coveredByPartyIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all the active app or resource delegations (except MaskinportenSchema delegations) for the set of covered toPartyUuids
    /// </summary>
    /// <param name="toPartyUuids">The party uuids to get received delegation for</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    Task<List<DelegationChanges>> GetAllDelegationChangesForAuthorizedParties(List<Guid> toPartyUuids, CancellationToken cancellationToken = default);
}
