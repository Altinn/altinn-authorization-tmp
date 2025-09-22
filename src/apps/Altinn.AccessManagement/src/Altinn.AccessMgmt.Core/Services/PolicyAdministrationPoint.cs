using System.Net;
using System.Text.Json;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Enums;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.Authorization.ABAC.Xacml;
using Azure;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using IPolicyAdministrationPoint = Altinn.AccessMgmt.Core.Services.Contracts.IPolicyAdministrationPoint;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc />
public class PolicyAdministrationPoint(AppDbContext dbContext, AuditValues auditValues, IPolicyRetrievalPoint prp, IPolicyFactory policyFactory, IDelegationMetadataRepository delegationRepository, IDelegationChangeEventQueue eventQueue, ILogger<IPolicyAdministrationPoint> logger) : IPolicyAdministrationPoint
{
    private readonly int delegationChangeEventQueueErrorId = 911;

    /// <inheritdoc />
    public async Task<List<Rule>> TryWriteDelegationPolicyRules(List<Rule> rules, CancellationToken cancellationToken)
    {
        List<Rule> result = new List<Rule>();
        Dictionary<string, List<Rule>> delegationDict = DelegationHelper.SortRulesByDelegationPolicyPath(rules, out List<Rule> unsortables);

        foreach (string delegationPolicypath in delegationDict.Keys)
        {
            bool writePolicySuccess = false;

            try
            {
                writePolicySuccess = await WriteDelegationPolicyInternal(delegationPolicypath, delegationDict[delegationPolicypath], cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception occured while processing authorization rules for delegation on delegation policy path: {delegationPolicypath}", delegationPolicypath);
            }

            foreach (Rule rule in delegationDict[delegationPolicypath])
            {
                if (writePolicySuccess)
                {
                    rule.CreatedSuccessfully = true;
                    rule.Type = RuleType.DirectlyDelegated;
                }
                else
                {
                    rule.RuleId = string.Empty;
                }

                result.Add(rule);
            }
        }

        if (unsortables.Count > 0)
        {
            string unsortablesJson = JsonSerializer.Serialize(unsortables);
            logger.LogError("One or more rules could not be processed because of incomplete input:\n{unsortablesJson}", unsortablesJson);
            result.AddRange(unsortables);
        }

        return result;
        
    }

    private async Task<bool> WriteDelegationPolicyInternal(string policyPath, List<Rule> rules, CancellationToken cancellationToken)
    {
        if (!DelegationHelper.TryGetDelegationParamsFromRule(rules[0], out ResourceAttributeMatchType resourceMatchType, out string resourceId, out string org, out string app, out int offeredByPartyId, out Guid? fromUuid, out UuidType fromUuidType, out Guid? toUuid, out UuidType toUuidType, out int? coveredByPartyId, out int? coveredByUserId, out int? delegatedByUserId, out int? delegatedByPartyId, out Guid? performedByUuid, out UuidType performedByUuidType, out DateTime delegatedDateTime)
            || resourceMatchType == ResourceAttributeMatchType.None)
        {
            logger.LogWarning("This should not happen. Incomplete rule model received for delegation to delegation policy at: {policyPath}. Incomplete model should have been returned in unsortable rule set by TryWriteDelegationPolicyRules. DelegationHelper.SortRulesByDelegationPolicyPath might be broken.", policyPath);
            return false;
        }

        if (resourceMatchType == ResourceAttributeMatchType.ResourceRegistry)
        {
            XacmlPolicy resourcePolicy = await prp.GetPolicyAsync(resourceId, cancellationToken);
            if (resourcePolicy == null)
            {
                logger.LogWarning("No valid resource policy found for delegation policy path: {policyPath}", policyPath);
                return false;
            }

            foreach (Rule rule in rules)
            {
                if (!DelegationHelper.PolicyContainsMatchingRule(resourcePolicy, rule))
                {
                    logger.LogWarning("Matching rule not found in resource policy. Action might not exist for Resource, or Resource itself might not exist. Delegation policy path: {policyPath}. Rule: {rule}", policyPath, rule);
                    return false;
                }
            }
        }
        else if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
        {
            XacmlPolicy appPolicy = await prp.GetPolicyAsync(org, app, cancellationToken);
            if (appPolicy == null)
            {
                logger.LogWarning("No valid App policy found for delegation policy path: {policyPath}", policyPath);
                return false;
            }

            foreach (Rule rule in rules)
            {
                if (!DelegationHelper.PolicyContainsMatchingRule(appPolicy, rule))
                {
                    logger.LogWarning("Matching rule not found in app policy. Action might not exist for Resource, or Resource itself might not exist. Delegation policy path: {policyPath}. Rule: {rule}", policyPath, rule);
                    return false;
                }
            }
        }

        var policyClient = policyFactory.Create(policyPath);
        if (!await policyClient.PolicyExistsAsync(cancellationToken))
        {
            // Create a new empty blob for lease locking
            await policyClient.WritePolicyAsync(new MemoryStream(), cancellationToken: cancellationToken);
        }

        string leaseId = await policyClient.TryAcquireBlobLease(cancellationToken);
        if (leaseId != null)
        {
            try
            {
                // ToDo: Read from EF-Service
                DelegationChange currentChange = await delegationRepository.GetCurrentDelegationChange(resourceMatchType, resourceId, offeredByPartyId, coveredByPartyId, coveredByUserId, toUuid, toUuidType, cancellationToken);

                XacmlPolicy existingDelegationPolicy = null;
                if (currentChange != null && currentChange.DelegationChangeType != DelegationChangeType.RevokeLast)
                {
                    existingDelegationPolicy = await prp.GetPolicyVersionAsync(policyPath, currentChange.BlobStorageVersionId, cancellationToken);
                }

                // Build delegation XacmlPolicy either as a new policy or add rules to existing
                XacmlPolicy delegationPolicy;
                if (existingDelegationPolicy != null)
                {
                    delegationPolicy = existingDelegationPolicy;
                    foreach (Rule rule in rules)
                    {
                        if (!DelegationHelper.PolicyContainsMatchingRule(delegationPolicy, rule))
                        {
                            (string coveredBy, string coveredByType) = PolicyHelper.GetCoveredByAndType(coveredByPartyId, coveredByUserId, toUuid, toUuidType);
                            delegationPolicy.Rules.Add(PolicyHelper.BuildDelegationRule(resourceId, offeredByPartyId, coveredBy, coveredByType, rule));
                        }
                    }
                }
                else
                {
                    delegationPolicy = PolicyHelper.BuildDelegationPolicy(resourceId, offeredByPartyId, coveredByPartyId, coveredByUserId, toUuid, toUuidType, rules);
                }

                // Write delegation policy to blob storage
                MemoryStream dataStream = PolicyHelper.GetXmlMemoryStreamFromXacmlPolicy(delegationPolicy);
                Response<BlobContentInfo> blobResponse = await policyClient.WritePolicyConditionallyAsync(dataStream, leaseId, cancellationToken);
                Response httpResponse = blobResponse.GetRawResponse();
                if (httpResponse.Status != (int)HttpStatusCode.Created)
                {
                    logger.LogError("Writing of delegation policy at path: {policyPath} failed. Response Status Code:\n{httpResponse.Status}. Response Reason Phrase:\n{httpResponse.ReasonPhrase}", policyPath, httpResponse.Status, httpResponse.ReasonPhrase);
                    return false;
                }

                // Write delegation change to postgresql
                DelegationChange change = new DelegationChange
                {
                    DelegationChangeType = DelegationChangeType.Grant,
                    ResourceId = resourceId,
                    OfferedByPartyId = offeredByPartyId,
                    FromUuid = fromUuid,
                    FromUuidType = fromUuidType,
                    CoveredByPartyId = coveredByPartyId,
                    CoveredByUserId = coveredByUserId,
                    ToUuid = toUuid,
                    ToUuidType = toUuidType,
                    PerformedByUserId = delegatedByUserId,
                    PerformedByPartyId = delegatedByPartyId,
                    PerformedByUuid = performedByUuid?.ToString(),
                    PerformedByUuidType = performedByUuidType,
                    Created = delegatedDateTime,
                    BlobStoragePolicyPath = policyPath,
                    BlobStorageVersionId = blobResponse.Value.VersionId
                };

                //// ToDo: Wrap in common transaction and write to EF-Services for both old and new delegation models
                change = await delegationRepository.InsertDelegation(resourceMatchType, change, cancellationToken);
                if (change == null || (change.DelegationChangeId <= 0 && change.ResourceRegistryDelegationChangeId <= 0))
                {
                    // Comment:
                    // This means that the current version of the root blob is no longer in sync with changes in authorization postgresql delegation.delegatedpolicy table.
                    // The root blob is in effect orphaned/ignored as the delegation policies are always to be read by version, and will be overritten by the next delegation change.
                    logger.LogError("Writing of delegation change to authorization postgresql database failed for changes to delegation policy at path: {policyPath}", policyPath);
                    return false;
                }

                if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
                {
                    try
                    {
                        await eventQueue.Push(change);
                    }
                    catch (Exception ex)
                    {
                        logger.LogCritical(new EventId(delegationChangeEventQueueErrorId, "DelegationChangeEventQueue.Push.Error"), ex, "AddRules could not push DelegationChangeEvent to DelegationChangeEventQueue. DelegationChangeEvent must be retried for successful sync with SBL Authorization. DelegationChange: {change}", change);
                    }
                }

                return true;
            }
            finally
            {
                await policyClient.ReleaseBlobLease(leaseId, CancellationToken.None);
            }
        }

        LogLeaseLockError(policyPath);
        return false;
    }

    private async Task<bool> WriteDbInternal(DelegationChange change, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        try
        {
            // ToDo: Find or create rettighetshaver or leverandør assignment (api-delegering)


            // ToDo: Insert Assignment Resource


            // ToDo: Insert delegation change


            // Commit transaction if all commands succeed, transaction will auto-rollback
            // when disposed if either commands fails
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

        return true;
    }

    /// <inheritdoc />
    public async Task<List<Rule>> TryDeleteDelegationPolicyRules(List<RequestToDelete> rulesToDelete, CancellationToken cancellationToken)
    {
        List<Rule> result = new List<Rule>();

        foreach (RequestToDelete deleteRequest in rulesToDelete)
        {
            List<Rule> currentRules = await DeleteRulesInPolicy(deleteRequest, cancellationToken);
            if (currentRules != null)
            {
                result.AddRange(currentRules);
            }
        }

        return result;
    }

    private async Task<List<Rule>> DeleteRulesInPolicy(RequestToDelete rulesToDelete, CancellationToken cancellationToken)
    {
        string coveredBy = DelegationHelper.GetCoveredByFromMatch(rulesToDelete.PolicyMatch.CoveredBy, out int? coveredByUserId, out int? coveredByPartyId, out Guid? coveredByUuid, out UuidType coveredByUuidType);

        if (!DelegationHelper.TryGetResourceFromAttributeMatch(rulesToDelete.PolicyMatch.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out string org, out string app, out string _, out string _))
        {
            logger.LogError("The resource cannot be identified.");
            return null;
        }

        string policyPath;
        try
        {
            policyPath = PolicyHelper.GetDelegationPolicyPath(resourceMatchType, resourceId, org, app, rulesToDelete.PolicyMatch.OfferedByPartyId.ToString(), coveredByUserId, coveredByPartyId, coveredByUuid, coveredByUuidType);
        }
        catch (Exception ex)
        {
            string rulesToDeleteString = string.Join(", ", rulesToDelete.RuleIds);
            logger.LogError(ex, "Not possible to build policy path for: {resourceId} CoveredBy: {coveredBy} OfferedBy: {policyToDelete.PolicyMatch.OfferedByPartyId} RuleIds: {rulesToDeleteString}", resourceId, coveredBy, rulesToDelete.PolicyMatch.OfferedByPartyId, rulesToDeleteString);
            return null;
        }

        var policyClient = policyFactory.Create(policyPath);
        if (!await policyClient.PolicyExistsAsync(cancellationToken))
        {
            logger.LogWarning("No blob was found for the expected path: {policyPath} this must be removed without updating the database", policyPath);
            return null;
        }

        List<Rule> currentRules = await ProcessPolicyFile(policyPath, resourceMatchType, resourceId, rulesToDelete, cancellationToken);

        return currentRules;
    }

    private async Task<List<Rule>> ProcessPolicyFile(string policyPath, ResourceAttributeMatchType resourceMatchType, string resourceId, RequestToDelete deleteRequest, CancellationToken cancellationToken = default)
    {
        List<Rule> currentRules = new List<Rule>();
        var policyClient = policyFactory.Create(policyPath);
        string leaseId = await policyClient.TryAcquireBlobLease(cancellationToken);

        if (leaseId == null)
        {
            LogLeaseLockError(policyPath, true);
            return null;
        }

        try
        {
            bool isAllRulesDeleted = false;
            string coveredBy = DelegationHelper.GetCoveredByFromMatch(deleteRequest.PolicyMatch.CoveredBy, out int? coveredByUserId, out int? coveredByPartyId, out Guid? coveredByUuid, out UuidType coveredByUuidType);
            string offeredBy = deleteRequest.PolicyMatch.OfferedByPartyId.ToString();
            DelegationHelper.TryGetPerformerFromAttributeMatches(deleteRequest.PerformedBy, out string performedByUuid, out UuidType performedByUuidType);

            //// ToDo: Read from EF-Services for both old and new delegation models
            DelegationChange currentChange = await delegationRepository.GetCurrentDelegationChange(resourceMatchType, resourceId, deleteRequest.PolicyMatch.OfferedByPartyId, coveredByPartyId, coveredByUserId, coveredByUuid, coveredByUuidType, cancellationToken);

            XacmlPolicy existingDelegationPolicy = null;
            if (currentChange.DelegationChangeType == DelegationChangeType.RevokeLast)
            {
                logger.LogWarning("The policy is already deleted for: {resourceId} CoveredBy: {coveredBy} OfferedBy: {offeredBy}", resourceId, coveredBy, offeredBy);
                return null;
            }

            existingDelegationPolicy = await prp.GetPolicyVersionAsync(currentChange.BlobStoragePolicyPath, currentChange.BlobStorageVersionId, cancellationToken);

            foreach (string ruleId in deleteRequest.RuleIds)
            {
                XacmlRule xacmlRuleToRemove = existingDelegationPolicy.Rules.FirstOrDefault(r => r.RuleId == ruleId);
                if (xacmlRuleToRemove == null)
                {
                    logger.LogWarning("The rule with id: {ruleId} does not exist in policy with path: {policyPath}", ruleId, policyPath);
                    continue;
                }

                existingDelegationPolicy.Rules.Remove(xacmlRuleToRemove);
                Rule currentRule = PolicyHelper.CreateRuleFromPolicyAndRuleMatch(deleteRequest, xacmlRuleToRemove);
                currentRules.Add(currentRule);
            }

            isAllRulesDeleted = existingDelegationPolicy.Rules.Count == 0;

            // if nothing is deleted no update has been done and policy and postgree update can be skipped
            if (currentRules.Count > 0)
            {
                Response<BlobContentInfo> response;
                try
                {
                    // Write delegation policy to blob storage
                    MemoryStream dataStream = PolicyHelper.GetXmlMemoryStreamFromXacmlPolicy(existingDelegationPolicy);
                    response = await policyClient.WritePolicyConditionallyAsync(dataStream, leaseId, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Writing of delegation policy at path: {policyPath} failed. Is delegation blob storage account alive and well?", policyPath);
                    return null;
                }

                // Write delegation change to postgresql
                DelegationChange change = new DelegationChange
                {
                    DelegationChangeType = isAllRulesDeleted ? DelegationChangeType.RevokeLast : DelegationChangeType.Revoke,
                    ResourceId = resourceId,
                    OfferedByPartyId = deleteRequest.PolicyMatch.OfferedByPartyId,
                    FromUuid = deleteRequest.PolicyMatch.FromUuid,
                    FromUuidType = deleteRequest.PolicyMatch.FromUuidType,
                    CoveredByPartyId = coveredByPartyId,
                    CoveredByUserId = coveredByUserId,
                    ToUuid = coveredByUuid,
                    ToUuidType = coveredByUuidType,
                    PerformedByUserId = deleteRequest.DeletedByUserId,
                    PerformedByUuid = performedByUuid,
                    PerformedByUuidType = performedByUuidType,
                    BlobStoragePolicyPath = policyPath,
                    BlobStorageVersionId = response.Value.VersionId
                };

                //// ToDo: Wrap in common transaction and write to EF-Services for both old and new delegation models
                change = await delegationRepository.InsertDelegation(resourceMatchType, change, cancellationToken);
                if (change == null || (change.DelegationChangeId <= 0 && change.ResourceRegistryDelegationChangeId <= 0))
                {
                    // Comment:
                    // This means that the current version of the root blob is no longer in sync with changes in authorization postgresql delegation.delegatedpolicy table.
                    // The root blob is in effect orphaned/ignored as the delegation policies are always to be read by version, and will be overritten by the next delegation change.
                    logger.LogError("Writing of delegation change to authorization postgresql database failed for changes to delegation policy at path: {policyPath}. is authorization postgresql database alive and well?", policyPath);
                    return null;
                }

                if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
                {
                    try
                    {
                        await eventQueue.Push(change);
                    }
                    catch (Exception ex)
                    {
                        logger.LogCritical(new EventId(delegationChangeEventQueueErrorId, "DelegationChangeEventQueue.Push.Error"), ex, "DeleteRules could not push DelegationChangeEvent to DelegationChangeEventQueue. DelegationChangeEvent must be retried for successful sync with SBL Authorization. DelegationChange: {change}", change);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occured while processing rules to delete in policy: {policyPath}", policyPath);
            return null;
        }
        finally
        {
            await policyClient.ReleaseBlobLease(leaseId, CancellationToken.None);
        }

        return currentRules;
    }

    /// <inheritdoc />
    public async Task<List<Rule>> TryDeleteDelegationPolicies(List<RequestToDelete> policiesToDelete, CancellationToken cancellationToken = default)
    {
        List<Rule> result = new List<Rule>();

        foreach (RequestToDelete policyToDelete in policiesToDelete)
        {
            List<Rule> currentRules = await DeleteAllRulesInPolicy(policyToDelete, cancellationToken);
            if (currentRules != null)
            {
                result.AddRange(currentRules);
            }
        }

        return result;
    }

    private async Task<List<Rule>> DeleteAllRulesInPolicy(RequestToDelete policyToDelete, CancellationToken cancellationToken = default)
    {
        string coveredBy = DelegationHelper.GetCoveredByFromMatch(policyToDelete.PolicyMatch.CoveredBy, out int? coveredByUserId, out int? coveredByPartyId, out Guid? coveredByUuid, out UuidType coveredByUuidType);
        DelegationHelper.TryGetPerformerFromAttributeMatches(policyToDelete.PerformedBy, out string performedByUuid, out UuidType performedByUuidType);

        if (!DelegationHelper.TryGetResourceFromAttributeMatch(policyToDelete.PolicyMatch.Resource, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out string org, out string app, out string _, out string _))
        {
            logger.LogError("The resource cannot be identified.");
            return null;
        }

        string policyPath;
        try
        {
            policyPath = PolicyHelper.GetDelegationPolicyPath(resourceMatchType, resourceId, org, app, policyToDelete.PolicyMatch.OfferedByPartyId.ToString(), coveredByUserId, coveredByPartyId, coveredByUuid, coveredByUuidType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Not possible to build policy path for: {resourceId} CoveredBy: {coveredBy} OfferedBy: {policyToDelete.PolicyMatch.OfferedByPartyId}", resourceId, coveredBy, policyToDelete.PolicyMatch.OfferedByPartyId);
            return null;
        }

        var policyClient = policyFactory.Create(policyPath);
        if (!await policyClient.PolicyExistsAsync(cancellationToken))
        {
            logger.LogWarning("No blob was found for the expected path: {policyPath} this must be removed without upading the database", policyPath);
            return null;
        }

        string leaseId = await policyClient.TryAcquireBlobLease(cancellationToken);
        if (leaseId == null)
        {
            logger.LogError("Could not acquire blob lease on delegation policy at path: {policyPath}", policyPath);
            return null;
        }

        try
        {
            // ToDo: Get from EF-service
            DelegationChange currentChange = await delegationRepository.GetCurrentDelegationChange(resourceMatchType, resourceId, policyToDelete.PolicyMatch.OfferedByPartyId, coveredByPartyId, coveredByUserId, coveredByUuid, coveredByUuidType, cancellationToken);

            if (currentChange.DelegationChangeType == DelegationChangeType.RevokeLast)
            {
                logger.LogWarning("The policy is already deleted for: {resourceId} CoveredBy: {coveredBy} OfferedBy: {policyToDelete.PolicyMatch.OfferedByPartyId}", resourceId, coveredBy, policyToDelete.PolicyMatch.OfferedByPartyId);
                return null;
            }

            XacmlPolicy existingDelegationPolicy = await prp.GetPolicyVersionAsync(currentChange.BlobStoragePolicyPath, currentChange.BlobStorageVersionId, cancellationToken);
            List<Rule> currentPolicyRules = new List<Rule>();
            foreach (XacmlRule xacmlRule in existingDelegationPolicy.Rules)
            {
                currentPolicyRules.Add(PolicyHelper.CreateRuleFromPolicyAndRuleMatch(policyToDelete, xacmlRule));
            }

            existingDelegationPolicy.Rules.Clear();

            Response<BlobContentInfo> response;
            try
            {
                MemoryStream dataStream = PolicyHelper.GetXmlMemoryStreamFromXacmlPolicy(existingDelegationPolicy);
                response = await policyClient.WritePolicyConditionallyAsync(dataStream, leaseId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Writing of delegation policy at path: {policyPath} failed. Is delegation blob storage account alive and well?}", policyPath);
                return null;
            }

            DelegationChange change = new DelegationChange
            {
                DelegationChangeType = DelegationChangeType.RevokeLast,
                ResourceId = resourceId,
                OfferedByPartyId = policyToDelete.PolicyMatch.OfferedByPartyId,
                CoveredByPartyId = coveredByPartyId,
                CoveredByUserId = coveredByUserId,
                ToUuid = coveredByUuid,
                ToUuidType = coveredByUuidType,
                FromUuid = policyToDelete.PolicyMatch.FromUuid,
                FromUuidType = policyToDelete.PolicyMatch.FromUuidType,
                PerformedByUserId = policyToDelete.DeletedByUserId,
                PerformedByUuid = performedByUuid,
                PerformedByUuidType = performedByUuidType,
                BlobStoragePolicyPath = policyPath,
                BlobStorageVersionId = response.Value.VersionId
            };

            //// ToDo: Wrap in common transaction and write to EF-Services for both old and new delegation models
            change = await delegationRepository.InsertDelegation(resourceMatchType, change, cancellationToken);
            if (change == null || (change.DelegationChangeId <= 0 && change.ResourceRegistryDelegationChangeId <= 0))
            {
                // Comment:
                // This means that the current version of the root blob is no longer in sync with changes in authorization postgresql delegation.delegatedpolicy table.
                // The root blob is in effect orphaned/ignored as the delegation policies are always to be read by version, and will be overritten by the next delegation change.
                logger.LogError("Writing of delegation change to authorization postgresql database failed for changes to delegation policy at path: {policyPath}. is authorization postgresql database alive and well?", policyPath);
                return null;
            }

            if (resourceMatchType == ResourceAttributeMatchType.AltinnAppId)
            {
                try
                {
                    await eventQueue.Push(change);
                }
                catch (Exception ex)
                {
                    logger.LogCritical(new EventId(delegationChangeEventQueueErrorId, "DelegationChangeEventQueue.Push.Error"), ex, "DeletePolicy could not push DelegationChangeEvent to DelegationChangeEventQueue. DelegationChangeEvent must be retried for successful sync with SBL Authorization. DelegationChange: {change}", change);
                }
            }

            return currentPolicyRules;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occured while processing rules to delete in policy: {policyPath}", policyPath);
            return null;
        }
        finally
        {
            await policyClient.ReleaseBlobLease(leaseId, CancellationToken.None);
        }
    }

    private void LogLeaseLockError(string policyPath, bool logAsError = false)
    {
        if (logAsError)
        {
            logger.LogError("Could not acquire blob lease lock on delegation policy at path: {policyPath}", policyPath);
        }
        else
        {
            logger.LogInformation("Could not acquire blob lease lock on delegation policy at path: {policyPath}", policyPath);
        }
    }
}
