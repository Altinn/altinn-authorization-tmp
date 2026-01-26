using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Enums;
using Altinn.AccessManagement.Persistence;
using Altinn.AccessManagement.Persistence.Extensions;

namespace Altinn.AccessMgmt.Core.Services.Legacy;

public sealed class DelegationMetadataRouter(DelegationMetadataEF newEf, DelegationMetadataRepo legacyRepo, ILegacyRoutingPolicy policy) : IDelegationMetadataRepository
{
    private async Task<T> Route<T>(
        string methodName,
        Func<DelegationMetadataRepo, Task<T>> legacyCall,
        Func<DelegationMetadataEF, Task<T>> newCall,
        CancellationToken ct)
    {
        var useLegacy = await policy.UseLegacyAsync(methodName, "DelegationMetadata", "Legacy", ct);
        return useLegacy ? await legacyCall(legacyRepo) : await newCall(newEf);
    }

    public Task<DelegationChange> InsertDelegation(
        ResourceAttributeMatchType resourceMatchType,
        DelegationChange delegationChange,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(InsertDelegation),
            legacy => legacy.InsertDelegation(resourceMatchType, delegationChange, cancellationToken),
            ef => ef.InsertDelegation(resourceMatchType, delegationChange, cancellationToken),
            cancellationToken);

    public Task<List<InstanceDelegationChange>> GetAllLatestInstanceDelegationChanges(
        InstanceDelegationSource source,
        string resourceID,
        string instanceID,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetAllLatestInstanceDelegationChanges),
            legacy => legacy.GetAllLatestInstanceDelegationChanges(source, resourceID, instanceID, cancellationToken),
            ef => ef.GetAllLatestInstanceDelegationChanges(source, resourceID, instanceID, cancellationToken),
            cancellationToken);

    public Task<List<InstanceDelegationChange>> GetAllCurrentReceivedInstanceDelegations(
        List<Guid> toUuid,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetAllCurrentReceivedInstanceDelegations),
            legacy => legacy.GetAllCurrentReceivedInstanceDelegations(toUuid, cancellationToken),
            ef => ef.GetAllCurrentReceivedInstanceDelegations(toUuid, cancellationToken),
            cancellationToken);

    public Task<InstanceDelegationChange> GetLastInstanceDelegationChange(
        InstanceDelegationChangeRequest request,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetLastInstanceDelegationChange),
            legacy => legacy.GetLastInstanceDelegationChange(request, cancellationToken),
            ef => ef.GetLastInstanceDelegationChange(request, cancellationToken),
            cancellationToken);

    public Task<InstanceDelegationChange> InsertInstanceDelegation(
        InstanceDelegationChange instanceDelegationChange,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(InsertInstanceDelegation),
            legacy => legacy.InsertInstanceDelegation(instanceDelegationChange, cancellationToken),
            ef => ef.InsertInstanceDelegation(instanceDelegationChange, cancellationToken),
            cancellationToken);

    public Task<bool> InsertMultipleInstanceDelegations(
        List<PolicyWriteOutput> policyWriteOutputs,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(InsertMultipleInstanceDelegations),
            legacy => legacy.InsertMultipleInstanceDelegations(policyWriteOutputs, cancellationToken),
            ef => ef.InsertMultipleInstanceDelegations(policyWriteOutputs, cancellationToken),
            cancellationToken);

    public Task<IEnumerable<InstanceDelegationChange>> GetActiveInstanceDelegations(
        List<string> resourceIds,
        Guid from,
        List<Guid> to,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetActiveInstanceDelegations),
            legacy => legacy.GetActiveInstanceDelegations(resourceIds, from, to, cancellationToken),
            ef => ef.GetActiveInstanceDelegations(resourceIds, from, to, cancellationToken),
            cancellationToken);

    public Task<DelegationChange> GetCurrentDelegationChange(
        ResourceAttributeMatchType resourceMatchType,
        string resourceId,
        int offeredByPartyId,
        int? coveredByPartyId,
        int? coveredByUserId,
        Guid? toUuid,
        UuidType toUuidType,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetCurrentDelegationChange),
            legacy => legacy.GetCurrentDelegationChange(resourceMatchType, resourceId, offeredByPartyId, coveredByPartyId, coveredByUserId, toUuid, toUuidType, cancellationToken),
            ef => ef.GetCurrentDelegationChange(resourceMatchType, resourceId, offeredByPartyId, coveredByPartyId, coveredByUserId, toUuid, toUuidType, cancellationToken),
            cancellationToken);

    public Task<List<DelegationChange>> GetAllCurrentAppDelegationChanges(
        List<int> offeredByPartyIds,
        List<string> altinnAppIds,
        List<int> coveredByPartyIds = null,
        List<int> coveredByUserIds = null,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetAllCurrentAppDelegationChanges), // NB: deler flagg
            legacy => legacy.GetAllCurrentAppDelegationChanges(offeredByPartyIds, altinnAppIds, coveredByPartyIds, coveredByUserIds, cancellationToken),
            ef => ef.GetAllCurrentAppDelegationChanges(offeredByPartyIds, altinnAppIds, coveredByPartyIds, coveredByUserIds, cancellationToken),
            cancellationToken);

    public Task<List<DelegationChange>> GetAllCurrentAppDelegationChanges(
        List<string> altinnAppIds,
        List<int> fromPartyIds,
        UuidType toUuidType,
        Guid toUuid,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetAllCurrentAppDelegationChanges), // NB: deler flagg
            legacy => legacy.GetAllCurrentAppDelegationChanges(altinnAppIds, fromPartyIds, toUuidType, toUuid, cancellationToken),
            ef => ef.GetAllCurrentAppDelegationChanges(altinnAppIds, fromPartyIds, toUuidType, toUuid, cancellationToken),
            cancellationToken);

    public Task<List<DelegationChange>> GetAllCurrentResourceRegistryDelegationChanges(
        List<int> offeredByPartyIds,
        List<string> resourceRegistryIds,
        List<int> coveredByPartyIds = null,
        int? coveredByUserId = null,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetAllCurrentResourceRegistryDelegationChanges), // NB: deler flagg
            legacy => legacy.GetAllCurrentResourceRegistryDelegationChanges(offeredByPartyIds, resourceRegistryIds, coveredByPartyIds, coveredByUserId, cancellationToken),
            ef => ef.GetAllCurrentResourceRegistryDelegationChanges(offeredByPartyIds, resourceRegistryIds, coveredByPartyIds, coveredByUserId, cancellationToken),
            cancellationToken);

    public Task<List<DelegationChange>> GetAllCurrentResourceRegistryDelegationChanges(
        List<string> resourceRegistryIds,
        List<int> fromPartyIds,
        UuidType toUuidType,
        Guid toUuid,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetAllCurrentResourceRegistryDelegationChanges), // NB: deler flagg
            legacy => legacy.GetAllCurrentResourceRegistryDelegationChanges(resourceRegistryIds, fromPartyIds, toUuidType, toUuid, cancellationToken),
            ef => ef.GetAllCurrentResourceRegistryDelegationChanges(resourceRegistryIds, fromPartyIds, toUuidType, toUuid, cancellationToken),
            cancellationToken);

    public Task<List<DelegationChange>> GetOfferedResourceRegistryDelegations(
        int offeredByPartyId,
        List<string> resourceRegistryIds = null,
        List<AccessManagement.Core.Models.ResourceRegistry.ResourceType> resourceTypes = null,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetOfferedResourceRegistryDelegations),
            legacy => legacy.GetOfferedResourceRegistryDelegations(offeredByPartyId, resourceRegistryIds, resourceTypes, cancellationToken),
            ef => ef.GetOfferedResourceRegistryDelegations(offeredByPartyId, resourceRegistryIds, resourceTypes, cancellationToken),
            cancellationToken);

    public Task<List<DelegationChange>> GetOfferedDelegations(
        List<int> offeredByPartyIds,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetOfferedDelegations),
            legacy => legacy.GetOfferedDelegations(offeredByPartyIds, cancellationToken),
            ef => ef.GetOfferedDelegations(offeredByPartyIds, cancellationToken),
            cancellationToken);

    public Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByPartys(
        List<int> coveredByPartyIds,
        List<int> offeredByPartyIds = null,
        List<string> resourceRegistryIds = null,
        List<AccessManagement.Core.Models.ResourceRegistry.ResourceType> resourceTypes = null,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetReceivedResourceRegistryDelegationsForCoveredByPartys),
            legacy => legacy.GetReceivedResourceRegistryDelegationsForCoveredByPartys(coveredByPartyIds, offeredByPartyIds, resourceRegistryIds, resourceTypes, cancellationToken),
            ef => ef.GetReceivedResourceRegistryDelegationsForCoveredByPartys(coveredByPartyIds, offeredByPartyIds, resourceRegistryIds, resourceTypes, cancellationToken),
            cancellationToken);

    public Task<List<DelegationChange>> GetReceivedResourceRegistryDelegationsForCoveredByUser(
        int coveredByUserId,
        List<int> offeredByPartyIds,
        List<string> resourceRegistryIds = null,
        List<AccessManagement.Core.Models.ResourceRegistry.ResourceType> resourceTypes = null,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetReceivedResourceRegistryDelegationsForCoveredByUser),
            legacy => legacy.GetReceivedResourceRegistryDelegationsForCoveredByUser(coveredByUserId, offeredByPartyIds, resourceRegistryIds, resourceTypes, cancellationToken),
            ef => ef.GetReceivedResourceRegistryDelegationsForCoveredByUser(coveredByUserId, offeredByPartyIds, resourceRegistryIds, resourceTypes, cancellationToken),
            cancellationToken);

    public Task<List<DelegationChange>> GetResourceRegistryDelegationChanges(
        List<string> resourceIds,
        int offeredByPartyId,
        int coveredByPartyId,
        AccessManagement.Core.Models.ResourceRegistry.ResourceType resourceType,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetResourceRegistryDelegationChanges),
            legacy => legacy.GetResourceRegistryDelegationChanges(resourceIds, offeredByPartyId, coveredByPartyId, resourceType, cancellationToken),
            ef => ef.GetResourceRegistryDelegationChanges(resourceIds, offeredByPartyId, coveredByPartyId, resourceType, cancellationToken),
            cancellationToken);

    public Task<List<DelegationChange>> GetAllDelegationChangesForAuthorizedParties(
        List<int> coveredByUserIds,
        List<int> coveredByPartyIds,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetAllDelegationChangesForAuthorizedParties), // NB: deler flagg
            legacy => legacy.GetAllDelegationChangesForAuthorizedParties(coveredByUserIds, coveredByPartyIds, cancellationToken),
            ef => ef.GetAllDelegationChangesForAuthorizedParties(coveredByUserIds, coveredByPartyIds, cancellationToken),
            cancellationToken);

    public Task<List<DelegationChange>> GetAllDelegationChangesForAuthorizedParties(
        List<Guid> toPartyUuids,
        CancellationToken cancellationToken = default)
        => Route(
            nameof(GetAllDelegationChangesForAuthorizedParties), // NB: deler flagg
            legacy => legacy.GetAllDelegationChangesForAuthorizedParties(toPartyUuids, cancellationToken),
            ef => ef.GetAllDelegationChangesForAuthorizedParties(toPartyUuids, cancellationToken),
            cancellationToken);
}
