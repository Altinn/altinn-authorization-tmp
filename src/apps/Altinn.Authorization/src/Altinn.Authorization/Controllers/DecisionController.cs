using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Altinn.Authorization.ABAC;
using Altinn.Authorization.ABAC.Utils;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Altinn.Platform.Authorization.Constants;
using Altinn.Platform.Authorization.ModelBinding;
using Altinn.Platform.Authorization.Models;
using Altinn.Platform.Authorization.Models.AccessManagement;
using Altinn.Platform.Authorization.Models.External;
using Altinn.Platform.Authorization.Repositories.Interface;
using Altinn.Platform.Authorization.Services.Interface;
using Altinn.Platform.Authorization.Services.Interfaces;
using Altinn.Platform.Storage.Interface.Models;
using AutoMapper;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Altinn.Platform.Authorization.Controllers
{
    /// <summary>
    /// This is the controller responsible for Policy Enformcent Point endpoint.
    /// It returns a Xacml Context Response based on a Context Request
    /// </summary>
    [ApiController]
    public class DecisionController : ControllerBase
    {
        private readonly PolicyDecisionPoint _pdp;
        private readonly IPolicyRetrievalPoint _prp;
        private readonly IContextHandler _contextHandler;
        private readonly IDelegationContextHandler _delegationContextHandler;
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IEventLog _eventLog;
        private readonly IFeatureManager _featureManager;
        private readonly IAccessManagementWrapper _accessManagement;
        private readonly IMapper _mapper;

        private readonly SortedDictionary<string, AuthInfo> _appInstanceInfo = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="DecisionController"/> class.
        /// </summary>
        /// <param name="accessManagement">Service for making request the to Access Management API (PIP)</param>
        /// <param name="contextHandler">The Context handler</param>
        /// <param name="delegationContextHandler">The delegation context handler</param>
        /// <param name="policyRetrievalPoint">The policy Retrieval point</param>
        /// <param name="delegationRepository">The delegation repository</param>
        /// <param name="logger">the logger</param>
        /// <param name="memoryCache">memory cache</param>
        /// <param name="eventLog">the authorization event logger</param>
        /// <param name="featureManager">the feature manager</param>
        /// <param name="mapper">The model mapper</param>
        public DecisionController(IAccessManagementWrapper accessManagement, IContextHandler contextHandler, IDelegationContextHandler delegationContextHandler, IPolicyRetrievalPoint policyRetrievalPoint, IDelegationMetadataRepository delegationRepository, ILogger<DecisionController> logger, IMemoryCache memoryCache, IEventLog eventLog, IFeatureManager featureManager, IMapper mapper)
        {
            _pdp = new PolicyDecisionPoint();
            _prp = policyRetrievalPoint;
            _contextHandler = contextHandler;
            _delegationContextHandler = delegationContextHandler;
            _logger = logger;
            _memoryCache = memoryCache;
            _eventLog = eventLog;
            _featureManager = featureManager;
            _accessManagement = accessManagement;
            _mapper = mapper;
        }

        /// <summary>
        /// Decision Point endpoint to authorize Xacml Context Requests
        /// </summary>
        /// <param name="model">A Generic model</param>
        [HttpPost]
        [Route("authorization/api/v1/[controller]")]
        public async Task<ActionResult> Post([FromBody] XacmlRequestApiModel model)
        {
            try
            {
                if (Request.ContentType.Contains("application/json"))
                {
                    return await AuthorizeJsonRequest(model);
                }
                else
                {
                    return await AuthorizeXmlRequest(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "// DecisionController // Decision // Unexpected Exception");

                XacmlContextResult result = new XacmlContextResult(XacmlContextDecision.Indeterminate)
                {
                    Status = new XacmlContextStatus(XacmlContextStatusCode.SyntaxError)
                };

                XacmlContextResponse xacmlContextResponse = new XacmlContextResponse(result);

                if (Request.ContentType.Contains("application/json"))
                {
                    XacmlJsonResponse jsonResult = XacmlJsonXmlConverter.ConvertResponse(xacmlContextResponse);
                    return Ok(jsonResult);
                }
                else
                {
                    return CreateResponse(xacmlContextResponse);
                }
            }
        }

        /// <summary>
        /// External endpoint for autorization 
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.AUTHORIZESCOPEACCESS)]
        [Route("authorization/api/v1/authorize")]
        public async Task<ActionResult<XacmlJsonResponseExternal>> AuthorizeExternal([FromBody] XacmlJsonRequestRootExternal authorizationRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                XacmlJsonRequestRoot jsonRequest = _mapper.Map<XacmlJsonRequestRoot>(authorizationRequest);
                XacmlJsonResponse xacmlResponse = await Authorize(jsonRequest.Request, true, cancellationToken);
                return _mapper.Map<XacmlJsonResponseExternal>(xacmlResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "// DecisionController // External Decision // Unexpected Exception");

                XacmlContextStatus status = new XacmlContextStatus(XacmlContextStatusCode.SyntaxError);
                if (ex is ArgumentException)
                {
                    status = new XacmlContextStatus(XacmlContextStatusCode.ProcessingError);
                    status.StatusMessage = ex.Message;
                }

                XacmlContextResult result = new XacmlContextResult(XacmlContextDecision.Indeterminate)
                {
                    Status = status
                };

                XacmlContextResponse xacmlContextResponse = new XacmlContextResponse(result);
                XacmlJsonResponse jsonResult = XacmlJsonXmlConverter.ConvertResponse(xacmlContextResponse);
                return _mapper.Map<XacmlJsonResponseExternal>(jsonResult);
            }
        }

        private async Task<XacmlJsonResponse> Authorize(XacmlJsonRequest decisionRequest, bool isExternalRequest = false, CancellationToken cancellationToken = default)
        {
            bool logEvent = true;
            if (decisionRequest.MultiRequests == null || decisionRequest.MultiRequests.RequestReference == null
                || decisionRequest.MultiRequests.RequestReference.Count < 2)
            {
                XacmlContextRequest request = XacmlJsonXmlConverter.ConvertRequest(decisionRequest);
                XacmlContextResponse xmlResponse = await Authorize(request, isExternalRequest, logEvent, cancellationToken);
                return XacmlJsonXmlConverter.ConvertResponse(xmlResponse);
            }
            else
            {
                logEvent = false;
                XacmlJsonResponse multiResponse = new XacmlJsonResponse();
                foreach (XacmlJsonRequestReference xacmlJsonRequestReference in decisionRequest.MultiRequests.RequestReference)
                {
                    XacmlJsonRequest jsonMultiRequestPart = new XacmlJsonRequest();

                    foreach (string refer in xacmlJsonRequestReference.ReferenceId)
                    {
                        IEnumerable<XacmlJsonCategory> resourceCategoriesPart = decisionRequest.Resource.Where(i => i.Id.Equals(refer));

                        if (resourceCategoriesPart != null && resourceCategoriesPart.Count() > 0)
                        {
                            if (jsonMultiRequestPart.Resource == null)
                            {
                                jsonMultiRequestPart.Resource = new List<XacmlJsonCategory>();
                            }

                            jsonMultiRequestPart.Resource.AddRange(resourceCategoriesPart);
                        }

                        IEnumerable<XacmlJsonCategory> subjectCategoriesPart = decisionRequest.AccessSubject.Where(i => i.Id.Equals(refer));

                        if (subjectCategoriesPart != null && subjectCategoriesPart.Count() > 0)
                        {
                            if (jsonMultiRequestPart.AccessSubject == null)
                            {
                                jsonMultiRequestPart.AccessSubject = new List<XacmlJsonCategory>();
                            }

                            jsonMultiRequestPart.AccessSubject.AddRange(subjectCategoriesPart);
                        }

                        IEnumerable<XacmlJsonCategory> actionCategoriesPart = decisionRequest.Action.Where(i => i.Id.Equals(refer));

                        if (actionCategoriesPart != null && actionCategoriesPart.Count() > 0)
                        {
                            if (jsonMultiRequestPart.Action == null)
                            {
                                jsonMultiRequestPart.Action = new List<XacmlJsonCategory>();
                            }

                            jsonMultiRequestPart.Action.AddRange(actionCategoriesPart);
                        }
                    }

                    XacmlContextResponse partResponse = await Authorize(XacmlJsonXmlConverter.ConvertRequest(jsonMultiRequestPart), isExternalRequest, logEvent, cancellationToken);
                    XacmlJsonResponse xacmlJsonResponsePart = XacmlJsonXmlConverter.ConvertResponse(partResponse);

                    if (multiResponse.Response == null)
                    {
                        multiResponse.Response = new List<XacmlJsonResult>();
                    }

                    multiResponse.Response.Add(xacmlJsonResponsePart.Response.First());
                }

                return multiResponse;
            }
        }

        private async Task<ActionResult> AuthorizeXmlRequest(XacmlRequestApiModel model)
        {
            XacmlContextRequest request;
            using (XmlReader reader = XmlReader.Create(new StringReader(model.BodyContent)))
            {
                request = XacmlParser.ReadContextRequest(reader);
            }

            XacmlContextResponse xacmlContextResponse = await Authorize(request, false, true);

            return CreateResponse(xacmlContextResponse);
        }

        private async Task<ActionResult> AuthorizeJsonRequest(XacmlRequestApiModel model)
        {
            XacmlJsonRequestRoot jsonRequest = JsonConvert.DeserializeObject<XacmlJsonRequestRoot>(model.BodyContent);

            XacmlJsonResponse jsonResponse = await Authorize(jsonRequest.Request);

            return Ok(jsonResponse);
        }

        private ActionResult CreateResponse(XacmlContextResponse xacmlContextResponse)
        {
            StringBuilder builder = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(builder))
            {
                XacmlSerializer.WriteContextResponse(writer, xacmlContextResponse);
            }

            string xml = builder.ToString();

            return Content(xml);
        }

        private async Task<XacmlContextResponse> Authorize(XacmlContextRequest decisionRequest, bool isExernalRequest, bool logEvent = true, CancellationToken cancellationToken = default)
        {
            decisionRequest = await this._contextHandler.Enrich(decisionRequest, isExernalRequest, _appInstanceInfo);

            ////_logger.LogInformation($"// DecisionController // Authorize // Roles // Enriched request: {JsonConvert.SerializeObject(decisionRequest)}.");
            XacmlPolicy policy = await _prp.GetPolicyAsync(decisionRequest);

            XacmlContextResponse rolesContextResponse = _pdp.Authorize(decisionRequest, policy);
            ////_logger.LogInformation($"// DecisionController // Authorize // Roles // XACML ContextResponse: {JsonConvert.SerializeObject(rolesContextResponse)}.");

            XacmlContextResult roleResult = rolesContextResponse.Results.First();
            if (roleResult.Decision.Equals(XacmlContextDecision.NotApplicable))
            {
                try
                {
                    XacmlContextResponse delegationContextResponse = await AuthorizeUsingDelegations(decisionRequest, policy, cancellationToken);
                    XacmlContextResult delegationResult = delegationContextResponse.Results.First();
                    if (delegationResult.Decision.Equals(XacmlContextDecision.Permit))
                    {
                        if (logEvent)
                        {
                            await _eventLog.CreateAuthorizationEvent(_featureManager, decisionRequest, HttpContext, delegationContextResponse, cancellationToken);
                        }

                        return delegationContextResponse;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "// DecisionController // Authorize // Delegation // Unexpected Exception");
                }
            }

            if (logEvent)
            {
                await _eventLog.CreateAuthorizationEvent(_featureManager, decisionRequest, HttpContext, rolesContextResponse, cancellationToken);
            }
            
            return rolesContextResponse;
        }

        private async Task<XacmlContextResponse> ProcessDelegationResult(XacmlContextRequest decisionRequest, XacmlPolicy resourcePolicy, IEnumerable<DelegationChangeExternal> delegations, CancellationToken cancellationToken = default)
        {
            if (delegations?.Any() ?? false)
            {
                List<int> keyrolePartyIds = await _delegationContextHandler.GetKeyRolePartyIds(_delegationContextHandler.GetSubjectUserId(decisionRequest), cancellationToken);
                _delegationContextHandler.Enrich(decisionRequest, keyrolePartyIds);
            }

            var delegationContextResponse = await MakeContextDecisionUsingDelegations(decisionRequest, delegations, resourcePolicy, cancellationToken);
            if (delegationContextResponse.Results.Any(r => r.Decision == XacmlContextDecision.Permit))
            {
                return delegationContextResponse;
            }

            return new XacmlContextResponse(new XacmlContextResult(XacmlContextDecision.NotApplicable)
            {
                Status = new XacmlContextStatus(XacmlContextStatusCode.Success)
            });
        }

        private static string CreateCacheKey(params string[] cacheKeys) =>
            string.Join("-", cacheKeys.Where(c => c != null && (c != string.Empty || !c.EndsWith(':'))));

        private static bool IsTypeApp(XacmlResourceAttributes resourceAttributes) =>
            !string.IsNullOrEmpty(resourceAttributes.OrgValue) && !string.IsNullOrEmpty(resourceAttributes.AppValue);

        private static bool IsTypeResource(XacmlResourceAttributes resourceAttributes) =>
            !string.IsNullOrEmpty(resourceAttributes.ResourceRegistryId);

        private bool IsIncompleteRequestForDelegation(XacmlResourceAttributes resourceAttributes, XacmlContextRequest decisionRequest) =>
            resourceAttributes == null ||
            (_delegationContextHandler.GetSubjectAttributeMatch(decisionRequest, [XacmlRequestAttribute.UserAttribute, XacmlRequestAttribute.PartyAttribute, XacmlRequestAttribute.SystemUserIdAttribute]) == null) ||
            !int.TryParse(resourceAttributes.ResourcePartyValue, out var _) ||
            !(IsTypeApp(resourceAttributes) || IsTypeResource(resourceAttributes));

        private Action<DelegationChangeInput> WithDefaultGetAllDelegationChangesInput(XacmlResourceAttributes resourceAttributes, XacmlContextRequest decisionRequest) => (input) =>
        {
            input.Subject = _delegationContextHandler.GetSubjectAttributeMatch(decisionRequest, [XacmlRequestAttribute.UserAttribute, XacmlRequestAttribute.PartyAttribute, XacmlRequestAttribute.SystemUserIdAttribute]);
            input.Party = new(AltinnXacmlConstants.MatchAttributeIdentifiers.PartyAttribute, resourceAttributes.ResourcePartyValue);
        };

        private async Task<XacmlContextResponse> AuthorizeUsingDelegations(XacmlContextRequest decisionRequest, XacmlPolicy resourcePolicy, CancellationToken cancellationToken = default)
        {
            var resourceAttributes = _delegationContextHandler.GetResourceAttributes(decisionRequest);
            if (IsIncompleteRequestForDelegation(resourceAttributes, decisionRequest))
            {
                string request = JsonConvert.SerializeObject(decisionRequest);
                _logger.LogWarning("// DecisionController // Authorize // Delegations // Incomplete request: {request}", request);
                return new XacmlContextResponse(new XacmlContextResult(XacmlContextDecision.Indeterminate)
                {
                    Status = new XacmlContextStatus(XacmlContextStatusCode.Success)
                });
            }

            if (IsTypeApp(resourceAttributes))
            {
                var delegations = await GetAllCachedDelegationChanges(WithDefaultGetAllDelegationChangesInput(resourceAttributes, decisionRequest), input => input.Resource = new List<AttributeMatch>()
                    {
                        new(AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute, resourceAttributes.OrgValue),
                        new(AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute, resourceAttributes.AppValue),
                    });

                return await ProcessDelegationResult(decisionRequest, resourcePolicy, delegations, cancellationToken);
            }

            if (IsTypeResource(resourceAttributes))
            {
                var delegations = await GetAllCachedDelegationChanges(WithDefaultGetAllDelegationChangesInput(resourceAttributes, decisionRequest), input => input.Resource = new List<AttributeMatch>()
                {
                    new(AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistry, resourceAttributes.ResourceRegistryId)
                });

                return await ProcessDelegationResult(decisionRequest, resourcePolicy, delegations, cancellationToken);
            }

            return new XacmlContextResponse(new XacmlContextResult(XacmlContextDecision.NotApplicable)
            {
                Status = new XacmlContextStatus(XacmlContextStatusCode.Success)
            });
        }

        private async Task<IEnumerable<DelegationChangeExternal>> GetAllCachedDelegationChanges(params Action<DelegationChangeInput>[] actions)
        {
            var delegation = new DelegationChangeInput();
            foreach (var action in actions)
            {
                action(delegation);
            }

            var cacheKey = CreateCacheKey(
                $"s:{delegation.Subject.Id}:{delegation.Subject.Value}",
                $"p:{delegation.Party.Value}",
                $"a:{delegation.Resource.FirstOrDefault(r => r.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.OrgAttribute)}/{delegation.Resource.FirstOrDefault(r => r.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute)}",
                $"r:{delegation.Resource.FirstOrDefault(r => r.Id == AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistry)}");

            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<DelegationChangeExternal> result))
            {
                result = await _accessManagement.GetAllDelegationChanges(actions);
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.High)
                    .SetAbsoluteExpiration(new TimeSpan(0, 0, 5, 0));

                _memoryCache.Set(cacheKey, result, cacheEntryOptions);
            }

            return result;
        }

        private async Task<XacmlContextResponse> MakeContextDecisionUsingDelegations(XacmlContextRequest decisionRequest, IEnumerable<DelegationChangeExternal> delegations, XacmlPolicy appPolicy, CancellationToken cancellationToken = default)
        {
            XacmlContextResponse delegationContextResponse = new XacmlContextResponse(new XacmlContextResult(XacmlContextDecision.NotApplicable)
            {
                Status = new XacmlContextStatus(XacmlContextStatusCode.Success)
            });

            foreach (DelegationChangeExternal delegation in delegations.Where(d => d.DelegationChangeType != DelegationChangeType.RevokeLast))
            {
                XacmlPolicy delegationPolicy = await _prp.GetPolicyVersionAsync(delegation.BlobStoragePolicyPath, delegation.BlobStorageVersionId, cancellationToken);
                foreach (XacmlObligationExpression obligationExpression in appPolicy.ObligationExpressions)
                {
                    delegationPolicy.ObligationExpressions.Add(obligationExpression);
                }

                delegationContextResponse = _pdp.Authorize(decisionRequest, delegationPolicy);

                string response = JsonConvert.SerializeObject(delegationContextResponse);
                _logger.LogInformation("// DecisionController // Authorize // Delegations // XACML ContextResponse\n{response}", response);

                if (delegationContextResponse.Results.Any(r => r.Decision == XacmlContextDecision.Permit))
                {
                    return delegationContextResponse;
                }
            }

            return delegationContextResponse;
        }
    }
}
