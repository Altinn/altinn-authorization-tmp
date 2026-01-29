using System.ComponentModel.DataAnnotations;
using System.Text;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Helpers.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.AccessList;
using Altinn.AccessManagement.Core.Models.Party;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;
using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils.Helper;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.ABAC.Constants;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.EntityFrameworkCore;
using ResourceType = Altinn.AccessManagement.Core.Models.ResourceRegistry.ResourceType;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc/>
public class ResourceService : IResourceService
{
    public AppDbContext Db { get; }

    public IAuditAccessor AuditAccessor { get; }

    public IPolicyRetrievalPoint PolicyRetrievalPoint { get; }

    public IAMPartyService PartyService { get; }

    public IContextRetrievalService ContextRetrievalService { get; }

    public IAccessListsAuthorizationClient AccessListsAuthorizationClient { get; }

    public IPolicyInformationPoint PolicyInformationPoint { get; }

    public IConnectionService ConnectionService { get; set; }

    public ResourceService(AppDbContext appDbContext, IAuditAccessor auditAccessor, IPolicyRetrievalPoint policyRetrievalPoint, IAMPartyService partyService, IContextRetrievalService contextRetrievalService, IAccessListsAuthorizationClient accessListsAuthorizationClient, IPolicyInformationPoint policyInformationPoint, IConnectionService connectionService)
    {
        Db = appDbContext;
        AuditAccessor = auditAccessor;
        PolicyRetrievalPoint = policyRetrievalPoint;
        PartyService = partyService;
        ContextRetrievalService = contextRetrievalService;
        AccessListsAuthorizationClient = accessListsAuthorizationClient;
        PolicyInformationPoint = policyInformationPoint;
        ConnectionService = connectionService;
    }

    public async ValueTask<Resource> GetResource(Guid id, CancellationToken cancellationToken = default)
    {
        return await Db.Resources.AsNoTracking().SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async ValueTask<Result<ResourceCheckDto>> DelegationCheck(Guid authenticatedUserUuid, int authenticatedUserId, int authenticationLevel, Guid party, string resourceId, CancellationToken cancellationToken = default)
    {
        // Fetch policy for the resource
        XacmlPolicy policy = await GetPolicy(resourceId, cancellationToken);

        ResourceDto resource = await FetchResource(resourceId, cancellationToken);

        // Decompose policy into tasks
        IEnumerable<ActionAccess> actionAcces = await DecomposePolicy(resourceId, policy, cancellationToken);

        var access = await ConnectionService.GetConnectionInfo(party, authenticatedUserUuid, resource.Id, cancellationToken);

        // Check AccessList usage
        
        // Fetch users roles for the resource/party

        // Fetch users packages for the resource/party

        // Fetch users right for the resource/party

        // build reult with reason based on roles, packages, resource rights and users access

        ResourceCheckDto resourceCheckDto = new ResourceCheckDto
        {
            Resource = resource,
            Actions = actionAcces.Select(aa => new ActionDto
            {
                ActionKey = aa.ActionKey,
                ActionName = aa.ActionKey, // TODO: Map to human readable name
                Result = true, // TODO: Calculate based on rights, roles, packages and access
                Reasons = aa.AccessorUrns.Select(urn => new ActionDto.Reason
                {
                    Description = $"Access granted based on accessor urn: {urn}",
                    ReasonKey = "Test"
                })
            })
        };

        // return result
        return resourceCheckDto;
    }

    private Action<ConnectionOptions> ConfigureConnections { get; } = options =>
    {
        options.AllowedWriteFromEntityTypes = [EntityTypeConstants.Organization];
        options.AllowedWriteToEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
        options.AllowedReadFromEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
        options.AllowedReadToEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
        options.FilterFromEntityTypes = [];
        options.FilterToEntityTypes = [];
    };

    private async Task<ResourceDto> FetchResource(string resourceId, CancellationToken cancellationToken)
    {
        Resource resource = await Db.Resources.AsNoTracking().SingleOrDefaultAsync(r => r.RefId == resourceId, cancellationToken);
        Provider provider = await Db.Providers.AsNoTracking().SingleOrDefaultAsync(p => p.Id == resource.ProviderId, cancellationToken);
        ProviderTypeConstants.TryGetById(provider.TypeId, out var providerType);
        PersistenceEF.Models.ResourceType resourceType = await Db.ResourceTypes.SingleOrDefaultAsync(rt => rt.Id == resource.TypeId, cancellationToken);

        ProviderDto providerDto = new ProviderDto
        {
            Id = provider.Id,
            Code = provider.Code,
            LogoUrl = provider.LogoUrl,
            Name = provider.Name,
            RefId = resource.RefId,
            TypeId = provider.TypeId,
            Type = new ProviderTypeDto { Id = providerType.Id, Name = providerType.Entity.Name }
        };

        ResourceDto resourceDto = new ResourceDto 
        {
            Id = resource.Id,
            Name = resource.Name,
            Description = resource.Description,
            Provider = providerDto,
            ProviderId = provider.Id,
            RefId = resource.RefId,
            TypeId = resource.TypeId,
            Type = new ResourceTypeDto { Id = resourceType.Id, Name = resourceType.Name }
        };

        return resourceDto;
    }

    private IEnumerable<string> RemoveNonUserRules(IEnumerable<string> ruleSubjects)
    {
        List<string> result = [];
        foreach (string subject in ruleSubjects)
        {
            if (subject.StartsWith(AltinnXacmlConstants.MatchAttributeIdentifiers.RoleAttribute))
            {
                result.Add(subject);
            }
            else if (subject.StartsWith(AltinnXacmlConstants.MatchAttributeIdentifiers.ExternalCcrRoleAttribute))
            {
                result.Add(subject);
            }
            else if (subject.StartsWith(AltinnXacmlConstants.MatchAttributeIdentifiers.ExternalCraRoleAttribute))
            {
                result.Add(subject);
            }
            else if (subject.StartsWith(AltinnXacmlConstants.MatchAttributeIdentifiers.AccessPackageAttribute))
            {
                result.Add(subject);
            }
            else if (subject.StartsWith(AltinnXacmlConstants.MatchAttributeIdentifiers.AccessPackagePersonAttribute))
            {
                result.Add(subject);
            }
        }

        return result;
    }

    private async Task<IEnumerable<ActionAccess>> DecomposePolicy(string resourceId, XacmlPolicy policy, CancellationToken cancellationToken)
    {
        Dictionary<string, List<string>> rules = new Dictionary<string, List<string>>();
        
        foreach (XacmlRule rule in policy.Rules)
        {
            IEnumerable<string> keys = CalculateActionKey(rule);
            IEnumerable<string> ruleSubjects = GetFirstAccessorValuesFromPolicy(rule, XacmlConstants.MatchAttributeCategory.Subject);
            ruleSubjects = RemoveNonUserRules(ruleSubjects);

            foreach (string key in keys) 
            {
                if (!rules.ContainsKey(key))
                {
                    List<string> value = [.. ruleSubjects];
                    rules.Add(key, value);
                }
                else
                {
                    rules[key].AddRange(ruleSubjects);
                }
            }            
        }

        List<ActionAccess> result = [];

        foreach (KeyValuePair<string, List<string>> action in rules)
        {
            ActionAccess current = new ActionAccess();
            current.ActionKey = action.Key;
            current.AccessorUrns = action.Value;
            result.Add(current);            
        }

        return result;
    }

    private IEnumerable<string> CalculateActionKey(XacmlRule rule)
    {
        List<string> result = [];

        //Use policy to calculate the rest of the key
        var resources = GetAllAccessorValuesFromPolicy(rule, XacmlConstants.MatchAttributeCategory.Resource).ToList();
        var actions = GetAllAccessorValuesFromPolicy(rule, XacmlConstants.MatchAttributeCategory.Action);
        List<string> resourceKeys = new List<string>();
        List<string> actionKeys = new List<string>();

        foreach (var resource in resources)
        {
            var org = resource.FirstOrDefault(r => r.Id.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute));
            var app = resource.FirstOrDefault(r => r.Id.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute));

            if (org != null && app != null)
            {
                string resourceAppId = $"app_{org.Value}_{app.Value}";
                resource.Remove(org);
                resource.Remove(app);
                resource.Add(new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute, Value = resourceAppId });
            }

            StringBuilder resourceKey = new();

            resource.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.InvariantCultureIgnoreCase));
            foreach (var item in resource)
            {
                resourceKey.Append(item.Id);
                resourceKey.Append(":");
                resourceKey.Append(item.Value);
            }

            resourceKeys.Add(resourceKey.ToString());
        }

        foreach (var action in actions)
        {
            StringBuilder actionKey = new();

            action.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.InvariantCultureIgnoreCase));
            foreach (var item in action)
            {
                actionKey.Append(item.Id);
                actionKey.Append(":");
                actionKey.Append(item.Value);
            }

            actionKeys.Add(actionKey.ToString());
        }

        foreach (var resource in resourceKeys)
        {
            foreach (var action in actionKeys)
            {
                result.Add(resource + ":" + action);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets a nested list of AttributeMatche models for all XacmlMatch instances matching the specified attribute category. 
    /// </summary>
    /// <param name="rule">The xacml rule to process</param>
    /// <param name="category">The attribute category to match</param>
    /// <returns>Nested list of PolicyAttributeMatch models</returns>
    public static IEnumerable<string> GetFirstAccessorValuesFromPolicy(XacmlRule rule, string category)
    {
        List<string> result = [];

        foreach (XacmlAnyOf anyOf in rule.Target.AnyOf)
        {
            foreach (XacmlAllOf allOf in anyOf.AllOf)
            {
                List<string> anyOfAttributeMatches = new();
                foreach (XacmlMatch xacmlMatch in allOf.Matches)
                {
                    if (xacmlMatch.AttributeDesignator.Category.Equals(category))
                    {
                        anyOfAttributeMatches.Add(xacmlMatch.AttributeDesignator.AttributeId.OriginalString + ":" + xacmlMatch.AttributeValue.Value);
                    }
                }

                if (anyOfAttributeMatches.Count() == 1)
                {
                    result.Add(anyOfAttributeMatches[0]);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Gets a nested list of AttributeMatche models for all XacmlMatch instances matching the specified attribute category. 
    /// </summary>
    /// <param name="rule">The xacml rule to process</param>
    /// <param name="category">The attribute category to match</param>
    /// <returns>Nested list of PolicyAttributeMatch models</returns>
    public static List<List<AttributeMatch>> GetAllAccessorValuesFromPolicy(XacmlRule rule, string category)
    {
        List<List<AttributeMatch>> result = [];

        foreach (XacmlAnyOf anyOf in rule.Target.AnyOf)
        {
            foreach (XacmlAllOf allOf in anyOf.AllOf)
            {
                List<AttributeMatch> anyOfAttributeMatches = new();
                foreach (XacmlMatch xacmlMatch in allOf.Matches)
                {
                    if (xacmlMatch.AttributeDesignator.Category.Equals(category))
                    {
                        anyOfAttributeMatches.Add(new AttributeMatch { Id = xacmlMatch.AttributeDesignator.AttributeId.OriginalString, Value = xacmlMatch.AttributeValue.Value });
                    }
                }

                if (anyOfAttributeMatches.Any())
                {
                    result.Add(anyOfAttributeMatches);
                }
            }
        }

        return result;
    }

    /*
    private static void EnrichRightsDictionaryWithRightsFromPolicy(XacmlPolicy policy, RightSourceType policySourceType, List<AttributeMatch> subjectMatches, int minimumAuthenticationLevel = 0, int delegationOfferedByPartyId = 0, bool returnAllPolicyRights = false, bool getDelegableRights = false)
    {
        foreach (XacmlRule rule in policy.Rules)
        {
            XacmlPolicy singleRulePolicy = new XacmlPolicy(new Uri($"{policy.PolicyId}_{rule.RuleId}"), policy.RuleCombiningAlgId, policy.Target);
            singleRulePolicy.Rules.Add(rule);

            List<List<PolicyAttributeMatch>> ruleSubjects = PolicyHelper.GetRulePolicyAttributeMatchesForCategory(rule, XacmlConstants.MatchAttributeCategory.Subject);
            ICollection<Right> ruleRights = PolicyHelper.GetRightsFromXacmlRules(rule.SingleToList());
            foreach (Right ruleRight in ruleRights)
            {
                ICollection<XacmlContextAttributes> contextAttributes = PolicyHelper.GetContextAttributes(subjectMatches, ruleRight.Resource, ruleRight.Action.SingleToList());
                XacmlContextRequest authRequest = new XacmlContextRequest(false, false, contextAttributes);

                // If getting rights for delegation, the right source is a delegation policy and the right does no longer exist in the app/resource policy: it should NOT be added as a delegable right
                if (getDelegableRights && policySourceType == RightSourceType.DelegationPolicy && !rights.ContainsKey(ruleRight.RightKey))
                {
                    continue;
                }

                if (!rights.TryGetValue(ruleRight.RightKey, out Right right))
                {
                    rights.Add(ruleRight.RightKey, ruleRight);
                    right = ruleRight;
                }

                // If getting rights for delegation, the xacml decision is to be used for indicating if the user can delegate the right. Otherwise the decision indicate whether the user actually have the right.
                if (getDelegableRights)
                {
                    right.CanDelegate = (right.CanDelegate.HasValue && right.CanDelegate.Value) || decisionResult.Decision.Equals(XacmlContextDecision.Permit);
                }
                else
                {
                    right.HasPermit = (right.HasPermit.HasValue && right.HasPermit.Value) || decisionResult.Decision.Equals(XacmlContextDecision.Permit);
                }

                if (decisionResult.Decision.Equals(XacmlContextDecision.Permit) || returnAllPolicyRights)
                {
                    right.RightSources.Add(
                        new RightSource
                        {
                            PolicyId = policy.PolicyId.OriginalString,
                            PolicyVersion = policy.Version,
                            RuleId = rule.RuleId,
                            RightSourceType = policySourceType,
                            HasPermit = getDelegableRights ? null : decisionResult.Decision.Equals(XacmlContextDecision.Permit),
                            CanDelegate = getDelegableRights ? decisionResult.Decision.Equals(XacmlContextDecision.Permit) : null,
                            MinimumAuthenticationLevel = minimumAuthenticationLevel,
                            OfferedByPartyId = delegationOfferedByPartyId,
                            UserSubjects = subjectMatches,
                            PolicySubjects = ruleSubjects
                        });
                }
            }
        }
    }
    */

    private async Task<DelegationCheckResponse> RightsDelegationCheck(int authenticatedUserId, int authenticatedUserAuthlevel, string resourceId, Guid fromPartyUuid, CancellationToken cancellationToken = default)
    {
        (DelegationCheckResponse result, ServiceResource resource, MinimalParty fromParty) = await ValidateRightDelegationCheckRequest(fromPartyUuid, resourceId, cancellationToken);
        if (!result.IsValid)
        {
            return result;
        }

        RightsQuery rightsQuery;
        //DelegationCheckHelper.TrySplitResiurceIdIntoOrgApp (resourceId, out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string org, out string app);
        
        rightsQuery = RightsHelper.GetRightsQuery(authenticatedUserId, fromParty.PartyId, resource);


        List<Right> allDelegableRights = await PolicyInformationPoint.GetRights(rightsQuery, getDelegableRights: true, returnAllPolicyRights: true, cancellationToken: cancellationToken);
        if (allDelegableRights == null || allDelegableRights.Count == 0)
        {
            result.Errors.Add("right[0].Resource", $"No delegable rights could be found for the resource: {resource}");
            return result;
        }

        // Build result model with status
        foreach (Right right in allDelegableRights)
        {
            if (!DelegationCheckHelper.CheckIfRuleIsAnEndUserRule(right))
            {
                continue;
            }

            AccessListAuthorizationResult accessListAuthorizationResult = AccessListAuthorizationResult.NotApplicable;

            if (DelegationCheckHelper.IsAccessListModeEnabledAndApplicable(right, resource, fromParty))
            {
                AccessListAuthorizationRequest accessListAuthorizationRequest = new AccessListAuthorizationRequest
                {
                    Subject = PartyUrn.OrganizationIdentifier.Create(OrganizationNumber.CreateUnchecked(fromParty.OrganizationId)),
                    Resource = ResourceIdUrn.ResourceId.Create(ResourceIdentifier.CreateUnchecked(resource.Identifier)),
                    Action = ActionUrn.ActionId.Create(ActionIdentifier.CreateUnchecked(right.Action.Value))
                };

                AccessListAuthorizationResponse accessListAuthorizationResponse = await AccessListsAuthorizationClient.AuthorizePartyForAccessList(accessListAuthorizationRequest, cancellationToken);
                accessListAuthorizationResult = accessListAuthorizationResponse.Result;
            }

            RightDelegationCheckResult rightDelegationStatus = new()
            {
                RightKey = right.RightKey,
                Resource = right.Resource,
                Action = right.Action,
                Details = RightsHelper.AnalyzeDelegationAccessReason(right, accessListAuthorizationResult)
            };

            rightDelegationStatus.Status = (right.CanDelegate.HasValue && right.CanDelegate.Value) ? DelegableStatus.Delegable : DelegableStatus.NotDelegable;

            result.RightDelegationCheckResults.Add(rightDelegationStatus);
        }

        return result;
    }

    private async Task<(DelegationCheckResponse Result, ServiceResource Resource, MinimalParty FromParty)> ValidateRightDelegationCheckRequest(Guid fromPartyUuid, string resourceId, CancellationToken cancellationToken)
    {
        DelegationCheckResponse result = new DelegationCheckResponse { From = new AttributeMatch { Id = AltinnXacmlConstants.MatchAttributeIdentifiers.PartyUuidAttribute, Value = fromPartyUuid.ToString() }.SingleToList(), RightDelegationCheckResults = new() };

        DelegationCheckHelper.TrySplitResiurceIdIntoOrgApp(resourceId, out ResourceAttributeMatchType resourceMatchType, out string resourceRegistryId, out string org, out string app);

        if (resourceMatchType == ResourceAttributeMatchType.None)
        {
            result.Errors.Add("right[0].Resource", $"The specified resource is not recognized. The operation only support requests for a single resource from either the Altinn Resource Registry identified by using the {AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute} attribute id, Altinn Apps identified by using {AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute} and {AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute}, or Altinn 2 services identified by using {AltinnXacmlConstants.MatchAttributeIdentifiers.ServiceCodeAttribute}");
            return (result, null, null);
        }

        // Verify resource is valid
        ServiceResource resource = await ContextRetrievalService.GetResourceFromResourceList(resourceRegistryId, org, app);
        if (resource == null || !resource.Delegable)
        {
            result.Errors.Add("right[0].Resource", $"The resource does not exist or is not available for delegation");
            return (result, resource, null);
        }

        if (resource.ResourceType == ResourceType.MaskinportenSchema)
        {
            result.Errors.Add("right[0].Resource", $"This operation does not support MaskinportenSchema resources. Please use the MaskinportenSchema DelegationCheck API. Invalid resource: {resourceRegistryId}. Invalid resource type: {resource.ResourceType}");
            return (result, resource, null);
        }

        // Verify and get From reportee party of the delegation
        MinimalParty fromParty = await PartyService.GetByUid(fromPartyUuid, cancellationToken);
        
        if (fromParty == null)
        {
            // This shouldn't really happen, as to get here the request must have been authorized for the From reportee, but the register integration could fail.
            result.Errors.Add("From", $"Could not identify the From party. Please try again.");
            return (result, resource, null);
        }

        return (result, resource, fromParty);
    }

    private async ValueTask<ResourceCheckDto> ConvertOldFormatToNew()
    {
        throw new NotImplementedException();
    }
    
    private async Task<XacmlPolicy> GetPolicy(string resourceId, CancellationToken cancellationToken = default)
    {
        XacmlPolicy policy = null;
        bool isApp = false;
        if (string.IsNullOrEmpty(resourceId))
        {
            throw new ValidationException($"ResourceId cannot be null or empty");
        }

        if (resourceId.StartsWith("app_"))
        {
            isApp = true;
        }

        if (isApp)
        {
            // Extract org and app from the resourceId
            string[] parts = resourceId.Split('_', 3);
            policy = await PolicyRetrievalPoint.GetPolicyAsync(parts[1], parts[2], cancellationToken);
        }
        else
        {
            policy = await PolicyRetrievalPoint.GetPolicyAsync(resourceId, cancellationToken);            
        }

        if (policy == null)
        {
            throw new ValidationException($"No valid policy found for the specified resource");
        }

        return policy;
    }
}
