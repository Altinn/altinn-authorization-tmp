using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection.Models;
using Altinn.Authorization.ABAC;
using Altinn.Authorization.ABAC.Constants;
using Altinn.Authorization.ABAC.Utils;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Altinn.Authorization.Api.Contracts.Authorization;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessMgmt.Core.Services;

/// <summary>
/// Service for making authorization decisions.
/// Orchestrates context enrichment, policy evaluation, and delegation resolution.
/// </summary>
public class AuthorizationDecisionService : IAuthorizationDecisionService
{
    private readonly IAuthorizationContextService _contextService;
    private readonly IPolicyRetrievalPoint _prp;
    private readonly IOedRoleAssignmentService _oedRoleService;
    private readonly ILogger<AuthorizationDecisionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationDecisionService"/> class.
    /// </summary>
    public AuthorizationDecisionService(
        IAuthorizationContextService contextService,
        IPolicyRetrievalPoint prp,
        IOedRoleAssignmentService oedRoleService,
        ILogger<AuthorizationDecisionService> logger)
    {
        _contextService = contextService;
        _prp = prp;
        _oedRoleService = oedRoleService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<AuthorizationResponseDto> AuthorizeAsync(AuthorizationRequestDto request, CancellationToken cancellationToken = default)
    {
        XacmlJsonRequest internalRequest = MapToInternal(request.Request);

        if (internalRequest.MultiRequests?.RequestReference is { Count: >= 2 })
        {
            return await AuthorizeMultiRequest(internalRequest, cancellationToken);
        }

        XacmlJsonResponse response = await AuthorizeSingleJsonRequest(internalRequest, cancellationToken);
        return MapToResponse(response);
    }

    private async Task<AuthorizationResponseDto> AuthorizeMultiRequest(XacmlJsonRequest decisionRequest, CancellationToken cancellationToken)
    {
        var sortedResources = new SortedList<string, XacmlJsonCategory>();
        foreach (var resource in decisionRequest.Resource)
        {
            sortedResources[resource.Id] = resource;
        }

        var multiResponse = new XacmlJsonResponse { Response = [] };

        foreach (var requestReference in decisionRequest.MultiRequests.RequestReference)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var partRequest = new XacmlJsonRequest();
            foreach (string refId in requestReference.ReferenceId)
            {
                if (sortedResources.TryGetValue(refId, out var resourceCategory))
                {
                    partRequest.Resource ??= [];
                    partRequest.Resource.Add(resourceCategory);
                }

                partRequest.AccessSubject = AddMatchingCategories(decisionRequest.AccessSubject, refId, partRequest.AccessSubject);
                partRequest.Action = AddMatchingCategories(decisionRequest.Action, refId, partRequest.Action);
            }

            var partResponse = await AuthorizeSingleJsonRequest(partRequest, cancellationToken);
            multiResponse.Response.Add(partResponse.Response[0]);
        }

        return MapToResponse(multiResponse);
    }

    private static List<XacmlJsonCategory> AddMatchingCategories(List<XacmlJsonCategory> source, string refId, List<XacmlJsonCategory> target)
    {
        if (source == null)
        {
            return target;
        }

        var matching = source.Where(c => c.Id.Equals(refId)).ToList();
        if (matching.Count > 0)
        {
            target ??= [];
            target.AddRange(matching);
        }

        return target;
    }

    private async Task<XacmlJsonResponse> AuthorizeSingleJsonRequest(XacmlJsonRequest jsonRequest, CancellationToken cancellationToken)
    {
        XacmlContextRequest request = XacmlJsonXmlConverter.ConvertRequest(jsonRequest);

        XacmlContextResponse response = await AuthorizeSingle(request, cancellationToken);

        return XacmlJsonXmlConverter.ConvertResponse(response);
    }

    private async Task<XacmlContextResponse> AuthorizeSingle(XacmlContextRequest decisionRequest, CancellationToken cancellationToken)
    {
        // 1. Resolve resource party and subject entities
        var resourceAttributes = ExtractResourceAttributes(decisionRequest);
        var subjectAttributes = ExtractSubjectAttributes(decisionRequest);

        Entity resourcePartyEntity = await ResolveResourceParty(resourceAttributes, cancellationToken);
        Entity subjectEntity = await ResolveSubject(subjectAttributes, cancellationToken);

        // 2. Enrich resource party ID on the request if resolved
        if (resourcePartyEntity != null && string.IsNullOrEmpty(resourceAttributes.ResourcePartyValue) && resourcePartyEntity.PartyId.HasValue)
        {
            AddAttributeToCategory(
                decisionRequest.GetResourceAttributes(),
                XacmlRequestAttribute.PartyAttribute,
                resourcePartyEntity.PartyId.Value.ToString());
            resourceAttributes.ResourcePartyValue = resourcePartyEntity.PartyId.Value.ToString();
        }

        // 3. Get policy for the resource
        XacmlPolicy policy = await _prp.GetPolicyAsync(decisionRequest, cancellationToken);
        if (policy == null)
        {
            return CreateIndeterminateResponse("Policy not found for resource");
        }

        // 4. Enrich subject with roles, access packages, and delegations from ConnectionQuery
        if (resourcePartyEntity != null && subjectEntity != null)
        {
            await EnrichSubjectFromConnections(decisionRequest, policy, resourcePartyEntity, subjectEntity, cancellationToken);
        }

        // 5. Enrich with OED roles if policy requires it (external dependency — kept as-is)
        await EnrichOedRoles(decisionRequest, policy, resourcePartyEntity, subjectEntity, subjectAttributes, cancellationToken);

        // 6. Evaluate against policy using PDP
        var pdp = new PolicyDecisionPoint();
        XacmlContextResponse rolesResponse = pdp.Authorize(decisionRequest, policy);
        XacmlContextResult roleResult = rolesResponse.Results.First();

        // 7. If role-based decision is NotApplicable, try delegations
        XacmlContextResponse finalResponse = rolesResponse;
        if (roleResult.Decision == XacmlContextDecision.NotApplicable && resourcePartyEntity != null && subjectEntity != null)
        {
            try
            {
                var delegationResponse = await AuthorizeUsingDelegations(decisionRequest, policy, resourcePartyEntity, subjectEntity, pdp, cancellationToken);
                if (delegationResponse != null)
                {
                    var delegationResult = delegationResponse.Results.First();
                    if (delegationResult.Decision == XacmlContextDecision.Permit)
                    {
                        finalResponse = delegationResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authorization using delegations failed");
            }
        }

        return finalResponse;
    }

    private async Task EnrichSubjectFromConnections(
        XacmlContextRequest decisionRequest,
        XacmlPolicy policy,
        Entity resourcePartyEntity,
        Entity subjectEntity,
        CancellationToken cancellationToken)
    {
        var connections = await _contextService.GetConnections(
            resourcePartyEntity.Id,
            [subjectEntity.Id],
            cancellationToken);

        if (connections == null || connections.Count == 0)
        {
            return;
        }

        var subjectContextAttributes = decisionRequest.GetSubjectAttributes();

        // Extract roles and add as XACML attributes
        IDictionary<string, ICollection<string>> policySubjectAttributes = policy.GetAttributeDictionaryByCategory(XacmlConstants.MatchAttributeCategory.Subject);

        if (policySubjectAttributes.ContainsKey(AltinnXacmlConstants.MatchAttributeIdentifiers.RoleAttribute))
        {
            var roleValues = connections
                .Where(c => c.Role != null && !string.IsNullOrEmpty(c.Role.Code))
                .Select(c => c.Role.Code)
                .Distinct()
                .ToList();

            if (roleValues.Count > 0)
            {
                var roleAttribute = new XacmlAttribute(new Uri(XacmlRequestAttribute.RoleAttribute), false);
                foreach (string roleCode in roleValues)
                {
                    roleAttribute.AttributeValues.Add(new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), roleCode));
                }

                subjectContextAttributes.Attributes.Add(roleAttribute);
            }
        }

        // Extract access packages and add as XACML attributes
        if (policySubjectAttributes.ContainsKey(AltinnXacmlConstants.MatchAttributeIdentifiers.AccessPackageAttribute))
        {
            var packageUrns = connections
                .SelectMany(c => c.Packages ?? [])
                .Where(p => !string.IsNullOrEmpty(p.Urn))
                .Select(p => p.Urn)
                .Distinct()
                .ToList();

            foreach (string urn in packageUrns)
            {
                // Package URNs are in the format "urn:altinn:accesspackage:package-name"
                // The XACML attribute id is the prefix and the value is the suffix
                int lastColon = urn.LastIndexOf(':');
                if (lastColon > 0)
                {
                    string attributeId = urn[..lastColon];
                    string attributeValue = urn[(lastColon + 1)..];
                    subjectContextAttributes.Attributes.Add(CreateStringAttribute(attributeId, attributeValue));
                }
            }
        }
    }

    private async Task EnrichOedRoles(
        XacmlContextRequest decisionRequest,
        XacmlPolicy policy,
        Entity resourcePartyEntity,
        Entity subjectEntity,
        SubjectAttributeValues subjectAttributes,
        CancellationToken cancellationToken)
    {
        IDictionary<string, ICollection<string>> policySubjectAttributes = policy.GetAttributeDictionaryByCategory(XacmlConstants.MatchAttributeCategory.Subject);
        const string OedRoleAttribute = "urn:digitaltdodsbo:rolecode";
        if (!policySubjectAttributes.ContainsKey(OedRoleAttribute))
        {
            return;
        }

        string subjectSsn = subjectAttributes.PersonId;
        if (string.IsNullOrEmpty(subjectSsn) && subjectEntity != null)
        {
            subjectSsn = subjectEntity.PersonIdentifier;
        }

        string resourceSsn = resourcePartyEntity?.PersonIdentifier;

        if (string.IsNullOrWhiteSpace(subjectSsn) || string.IsNullOrWhiteSpace(resourceSsn))
        {
            return;
        }

        try
        {
            var oedRoleCodes = await _oedRoleService.GetOedRoleCodes(resourceSsn, subjectSsn, cancellationToken);
            if (oedRoleCodes?.Count > 0)
            {
                var subjectContextAttributes = decisionRequest.GetSubjectAttributes();
                var oedAttribute = new XacmlAttribute(new Uri(OedRoleAttribute), false);
                foreach (string code in oedRoleCodes)
                {
                    oedAttribute.AttributeValues.Add(new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), code));
                }

                subjectContextAttributes.Attributes.Add(oedAttribute);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get OED role assignments");
        }
    }

    private async Task<XacmlContextResponse> AuthorizeUsingDelegations(
        XacmlContextRequest decisionRequest,
        XacmlPolicy resourcePolicy,
        Entity resourcePartyEntity,
        Entity subjectEntity,
        PolicyDecisionPoint pdp,
        CancellationToken cancellationToken)
    {
        var connections = await _contextService.GetConnections(
            resourcePartyEntity.Id,
            [subjectEntity.Id],
            cancellationToken);

        var delegations = connections
            .Where(c => c.Reason == ConnectionReason.Delegation && c.Resources?.Count > 0)
            .ToList();

        if (delegations.Count == 0)
        {
            return null;
        }

        // TODO: Evaluate delegation policies from blob storage once the delegation connection records
        // include blob storage path and version. For now, return null to fall through to role-based decision.
        return null;
    }

    #region Entity Resolution

    private async Task<Entity> ResolveResourceParty(ResourceAttributeValues attrs, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(attrs.OrganizationNumber))
        {
            return await _contextService.ResolveEntityByOrgNo(attrs.OrganizationNumber, cancellationToken);
        }

        if (!string.IsNullOrEmpty(attrs.PersonId))
        {
            return await _contextService.ResolveEntityByPersonId(attrs.PersonId, cancellationToken);
        }

        if (attrs.PartyUuid != Guid.Empty)
        {
            return await _contextService.ResolveEntityByUuid(attrs.PartyUuid, cancellationToken);
        }

        if (!string.IsNullOrEmpty(attrs.ResourcePartyValue) && int.TryParse(attrs.ResourcePartyValue, out int partyId))
        {
            return await _contextService.ResolveEntityByPartyId(partyId, cancellationToken);
        }

        return null;
    }

    private async Task<Entity> ResolveSubject(SubjectAttributeValues attrs, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(attrs.PersonId))
        {
            return await _contextService.ResolveEntityByPersonId(attrs.PersonId, cancellationToken);
        }

        if (attrs.UserId > 0)
        {
            return await _contextService.ResolveEntityByUserId(attrs.UserId, cancellationToken);
        }

        if (!string.IsNullOrEmpty(attrs.OrganizationNumber))
        {
            return await _contextService.ResolveEntityByOrgNo(attrs.OrganizationNumber, cancellationToken);
        }

        if (attrs.PartyUuid != Guid.Empty)
        {
            return await _contextService.ResolveEntityByUuid(attrs.PartyUuid, cancellationToken);
        }

        if (attrs.SystemUserUuid != Guid.Empty)
        {
            return await _contextService.ResolveEntityByUuid(attrs.SystemUserUuid, cancellationToken);
        }

        return null;
    }

    #endregion

    #region Attribute Extraction

    private static ResourceAttributeValues ExtractResourceAttributes(XacmlContextRequest request)
    {
        var result = new ResourceAttributeValues();
        var resourceAttrs = request.GetResourceAttributes();
        if (resourceAttrs == null)
        {
            return result;
        }

        foreach (var attr in resourceAttrs.Attributes)
        {
            string id = attr.AttributeId.OriginalString;
            string value = attr.AttributeValues.FirstOrDefault()?.Value;
            if (value == null)
            {
                continue;
            }

            switch (id)
            {
                case XacmlRequestAttribute.OrgAttribute: result.OrgValue = value; break;
                case XacmlRequestAttribute.AppAttribute: result.AppValue = value; break;
                case XacmlRequestAttribute.PartyAttribute: result.ResourcePartyValue = value; break;
                case XacmlRequestAttribute.OrganizationNumberAttribute: result.OrganizationNumber = value; break;
                case "urn:altinn:person:identifier-no": result.PersonId = value; break;
                case XacmlRequestAttribute.ResourceRegistryAttribute:
                    if (value.StartsWith("app_"))
                    {
                        var parts = value.Split('_');
                        result.OrgValue = parts[1];
                        result.AppValue = parts[2];
                    }
                    else
                    {
                        result.ResourceRegistryId = value;
                    }

                    break;
                case "urn:altinn:party:uuid":
                    if (Guid.TryParse(value, out var uuid))
                    {
                        result.PartyUuid = uuid;
                    }

                    break;
            }
        }

        return result;
    }

    private static SubjectAttributeValues ExtractSubjectAttributes(XacmlContextRequest request)
    {
        var result = new SubjectAttributeValues();
        var subjectAttrs = request.GetSubjectAttributes();
        if (subjectAttrs == null)
        {
            return result;
        }

        foreach (var attr in subjectAttrs.Attributes)
        {
            string id = attr.AttributeId.OriginalString;
            string value = attr.AttributeValues.FirstOrDefault()?.Value;
            if (value == null)
            {
                continue;
            }

            switch (id)
            {
                case XacmlRequestAttribute.UserAttribute:
                    if (int.TryParse(value, out int userId))
                    {
                        result.UserId = userId;
                    }

                    break;
                case "urn:altinn:person:identifier-no": result.PersonId = value; break;
                case XacmlRequestAttribute.OrganizationNumberAttribute: result.OrganizationNumber = value; break;
                case "urn:altinn:organization:identifier-no": result.OrganizationNumber = value; break;
                case "urn:altinn:party:uuid":
                    if (Guid.TryParse(value, out var pUuid))
                    {
                        result.PartyUuid = pUuid;
                    }

                    break;
                case "urn:altinn:systemuser:uuid":
                    if (Guid.TryParse(value, out var sUuid))
                    {
                        result.SystemUserUuid = sUuid;
                    }

                    break;
            }
        }

        return result;
    }

    #endregion

    #region Mapping Helpers

    private static XacmlJsonRequest MapToInternal(AuthorizationXacmlRequestDto dto)
    {
        if (dto == null)
        {
            return new XacmlJsonRequest();
        }

        return new XacmlJsonRequest
        {
            ReturnPolicyIdList = dto.ReturnPolicyIdList,
            CombinedDecision = dto.CombinedDecision,
            XPathVersion = dto.XPathVersion,
            Category = MapCategories(dto.Category),
            Resource = MapCategories(dto.Resource),
            Action = MapCategories(dto.Action),
            AccessSubject = MapCategories(dto.AccessSubject),
            RecipientSubject = MapCategories(dto.RecipientSubject),
            IntermediarySubject = MapCategories(dto.IntermediarySubject),
            RequestingMachine = MapCategories(dto.RequestingMachine),
            MultiRequests = dto.MultiRequests == null ? null : new XacmlJsonMultiRequests
            {
                RequestReference = dto.MultiRequests.RequestReference?.Select(r => new XacmlJsonRequestReference
                {
                    ReferenceId = r.ReferenceId,
                }).ToList(),
            },
        };
    }

    private static List<XacmlJsonCategory> MapCategories(List<AuthorizationXacmlCategoryDto> categories)
    {
        return categories?.Select(c => new XacmlJsonCategory
        {
            CategoryId = c.CategoryId,
            Id = c.Id,
            Content = c.Content,
            Attribute = c.Attribute?.Select(a => new XacmlJsonAttribute
            {
                AttributeId = a.AttributeId,
                Value = a.Value,
                Issuer = a.Issuer,
                DataType = a.DataType,
                IncludeInResult = a.IncludeInResult,
            }).ToList(),
        }).ToList();
    }

    private static AuthorizationResponseDto MapToResponse(XacmlJsonResponse response)
    {
        return new AuthorizationResponseDto
        {
            Response = response.Response?.Select(r => new AuthorizationXacmlResultDto
            {
                Decision = r.Decision,
                Status = r.Status == null ? null : new AuthorizationXacmlStatusDto
                {
                    StatusMessage = r.Status.StatusMessage,
                    StatusCode = r.Status.StatusCode == null ? null : MapStatusCode(r.Status.StatusCode),
                },
                Obligations = r.Obligations?.Select(MapObligationOrAdvice).ToList(),
                AssociateAdvice = r.AssociateAdvice?.Select(MapObligationOrAdvice).ToList(),
                Category = r.Category?.Select(c => new AuthorizationXacmlCategoryDto
                {
                    CategoryId = c.CategoryId,
                    Id = c.Id,
                    Content = c.Content,
                    Attribute = c.Attribute?.Select(a => new AuthorizationXacmlAttributeDto
                    {
                        AttributeId = a.AttributeId,
                        Value = a.Value,
                        Issuer = a.Issuer,
                        DataType = a.DataType,
                        IncludeInResult = a.IncludeInResult,
                    }).ToList(),
                }).ToList(),
            }).ToList(),
        };
    }

    private static AuthorizationXacmlStatusCodeDto MapStatusCode(XacmlJsonStatusCode code)
    {
        if (code == null)
        {
            return null;
        }

        return new AuthorizationXacmlStatusCodeDto
        {
            Value = code.Value,
            StatusCode = MapStatusCode(code.StatusCode),
        };
    }

    private static AuthorizationXacmlObligationOrAdviceDto MapObligationOrAdvice(XacmlJsonObligationOrAdvice oa)
    {
        return new AuthorizationXacmlObligationOrAdviceDto
        {
            Id = oa.Id,
            AttributeAssignment = oa.AttributeAssignment?.Select(aa => new AuthorizationXacmlAttributeAssignmentDto
            {
                AttributeId = aa.AttributeId,
                Value = aa.Value,
                Category = aa.Category,
                DataType = aa.DataType,
                Issuer = aa.Issuer,
            }).ToList(),
        };
    }

    #endregion

    #region XACML Helpers

    private static XacmlAttribute CreateStringAttribute(string attributeId, string value)
    {
        var attribute = new XacmlAttribute(new Uri(attributeId), false);
        attribute.AttributeValues.Add(new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), value));
        return attribute;
    }

    private static void AddAttributeToCategory(XacmlContextAttributes category, string attributeId, string value)
    {
        if (category == null)
        {
            return;
        }

        var attribute = new XacmlAttribute(new Uri(attributeId), true);
        attribute.AttributeValues.Add(new XacmlAttributeValue(new Uri(XacmlConstants.DataTypes.XMLString), value));
        category.Attributes.Add(attribute);
    }

    private static XacmlContextResponse CreateIndeterminateResponse(string message)
    {
        return new XacmlContextResponse(new XacmlContextResult(XacmlContextDecision.Indeterminate)
        {
            Status = new XacmlContextStatus(XacmlContextStatusCode.ProcessingError) { StatusMessage = message },
        });
    }

    #endregion

    #region Internal Value Models

    private sealed class ResourceAttributeValues
    {
        public string OrgValue { get; set; }
        public string AppValue { get; set; }
        public string ResourcePartyValue { get; set; }
        public string ResourceRegistryId { get; set; }
        public string OrganizationNumber { get; set; }
        public string PersonId { get; set; }
        public Guid PartyUuid { get; set; }
    }

    private sealed class SubjectAttributeValues
    {
        public int UserId { get; set; }
        public string PersonId { get; set; }
        public string OrganizationNumber { get; set; }
        public Guid PartyUuid { get; set; }
        public Guid SystemUserUuid { get; set; }
    }

    #endregion
}
