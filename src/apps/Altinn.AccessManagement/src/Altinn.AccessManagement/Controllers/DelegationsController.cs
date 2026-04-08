using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessMgmt.Core.Audit;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Altinn.Platform.Storage.Interface.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// Controller responsible for all operations for managing delegations of Altinn Apps
    /// </summary>
    [ApiController]
    public class DelegationsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IPolicyInformationPoint _pip;
        private readonly ISingleRightsService _rights;
        private readonly IEntityService _entityService;
        private readonly IResourceService _resourceService;
        private readonly IConnectionService _connectionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegationsController"/> class.
        /// </summary>
        /// <param name="logger">the logger.</param>
        /// <param name="policyInformationPoint">The policy information point</param>
        /// <param name="rights">Singlerights service to enrich and call PolicyAdministrationpoint for storing the changed rights</param>
        /// <param name="entityService">The entity service for retrieving entity information for parties involved in delegations</param>
        /// <param name="resourceService">The resource service for retrieving resource information for delegations</param>
        /// <param name="connectionService">The connection service for creating instance delegations</param>
        public DelegationsController(
            ILogger<DelegationsController> logger,
            IPolicyInformationPoint policyInformationPoint,
            ISingleRightsService rights,
            IEntityService entityService,
            IResourceService resourceService,
            IConnectionService connectionService)
        {
            _logger = logger;
            _pip = policyInformationPoint;
            _rights = rights;
            _entityService = entityService;
            _resourceService = resourceService;
            _connectionService = connectionService;
        }

        /// <summary>
        /// Endpoint for adding one or more rules for the given app/offeredby/coveredby. This updates or creates a new delegated policy of type "DirectlyDelegated". DelegatedByUserId is included to store history information in 3.0.
        /// </summary>
        /// <param name="rules">All rules to be delegated</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <response code="201" cref="List{PolicyRule}">Created</response>
        /// <response code="206" cref="List{PolicyRule}">Partial Content</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [AuditStaticDb(System = AuditDefaults.Altinn2AddRulesApi)]
        [Route("accessmanagement/api/v1/delegations/addrules")]
        public async Task<ActionResult> Post([FromBody] List<Rule> rules, CancellationToken cancellationToken)
        {
            if (rules == null || rules.Count < 1)
            {
                return BadRequest("Missing rules in body");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model");
            }

            List<Rule> delegationResults = await _rights.EnrichAndTryWriteDelegationPolicyRules(rules, ignoreExistingPolicy: false, cancellationToken: cancellationToken);

            if (delegationResults.All(r => r.CreatedSuccessfully))
            {
                return Created("Created", delegationResults);
            }

            if (delegationResults.Any(r => r.CreatedSuccessfully))
            {
                return StatusCode(206, delegationResults);
            }

            string rulesJson = JsonSerializer.Serialize(rules);
            _logger.LogInformation("Delegation could not be completed. None of the rules could be processed, indicating invalid or incomplete input:\n{rulesJson}", rulesJson);
            return BadRequest("Delegation could not be completed");
        }

        /// <summary>
        /// Endpoint for retrieving delegated rules between parties
        /// </summary>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [Route("accessmanagement/api/v1/delegations/getrules")]
        public async Task<ActionResult<List<Rule>>> GetRules([FromBody] RuleQuery ruleQuery, CancellationToken cancellationToken, [FromQuery] bool onlyDirectDelegations = false)
        {
            List<int> coveredByPartyIds = new List<int>();
            List<int> coveredByUserIds = new List<int>();
            List<int> offeredByPartyIds = new List<int>();
            List<string> resourceIds = new List<string>();

            if (ruleQuery.KeyRolePartyIds.Any(id => id != 0))
            {
                coveredByPartyIds.AddRange(ruleQuery.KeyRolePartyIds);
            }

            if (ruleQuery.ParentPartyId != 0)
            {
                offeredByPartyIds.Add(ruleQuery.ParentPartyId);
            }

            foreach (List<AttributeMatch> resource in ruleQuery.Resources)
            {
                if (DelegationHelper.TryGetResourceFromAttributeMatch(resource, out ResourceAttributeMatchType resourceMatchType, out string resourceId, out _, out _, out _, out _))
                {
                    resourceIds.Add(resourceId);
                }
            }

            if (DelegationHelper.TryGetPartyIdFromAttributeMatch(ruleQuery.CoveredBy, out int partyId))
            {
                coveredByPartyIds.Add(partyId);
            }
            else if (DelegationHelper.TryGetUserIdFromAttributeMatch(ruleQuery.CoveredBy, out int userId))
            {
                coveredByUserIds.Add(userId);
            }

            if (ruleQuery.OfferedByPartyId != 0)
            {
                offeredByPartyIds.Add(ruleQuery.OfferedByPartyId);
            }

            if (offeredByPartyIds.Count == 0)
            {
                return StatusCode(400, $"Unable to get the rules: Missing offeredbyPartyId value.");
            }

            if (coveredByPartyIds.Count == 0 && coveredByUserIds.Count == 0)
            {
                return StatusCode(400, $"Unable to get the rules: Missing offeredby and coveredby values.");
            }

            List<Rule> rulesList = await _pip.GetRulesAsync(resourceIds, offeredByPartyIds, coveredByPartyIds, coveredByUserIds, cancellationToken);
            DelegationHelper.SetRuleType(rulesList, ruleQuery.OfferedByPartyId, ruleQuery.KeyRolePartyIds, ruleQuery.CoveredBy, ruleQuery.ParentPartyId);
            return Ok(rulesList);
        }

        /// <summary>
        /// Endpoint for deleting delegated rules between parties
        /// </summary>
        /// <response code="200" cref="List{PolicyRule}">Deleted</response>
        /// <response code="206" cref="List{PolicyRule}">Partial Content</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [AuditStaticDb(System = AuditDefaults.Altinn2AddRulesApi)]
        [Route("accessmanagement/api/v1/delegations/deleterules")]
        public async Task<ActionResult> DeleteRule([FromBody] RequestToDeleteRuleList rulesToDelete, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Rule> deletionResults = await _rights.EnrichAndTryDeleteDelegationPolicyRules(rulesToDelete, cancellationToken);
            int ruleCountToDelete = DelegationHelper.GetRulesCountToDeleteFromRequestToDelete(rulesToDelete);
            int deletionResultsCount = deletionResults.Count;

            if (deletionResultsCount == ruleCountToDelete)
            {
                return StatusCode(200, deletionResults);
            }

            string rulesToDeleteSerialized = JsonSerializer.Serialize(rulesToDelete);
            if (deletionResultsCount > 0)
            {
                string deletionResultsSerialized = JsonSerializer.Serialize(deletionResults);
                _logger.LogInformation("Partial deletion completed deleted {deletionResultsCount} of {ruleCountToDelete}.\n{rulesToDeleteSerialized}\n{deletionResultsSerialized}", deletionResultsCount, ruleCountToDelete, rulesToDeleteSerialized, deletionResultsSerialized);
                return StatusCode(206, deletionResults);
            }

            _logger.LogInformation("Deletion could not be completed. None of the rules could be processed, indicating invalid or incomplete input:\n{rulesToDeleteSerialized}", rulesToDeleteSerialized);
            return StatusCode(400, $"Unable to complete deletion");
        }

        /// <summary>
        /// Endpoint for deleting an entire delegated policy between parties
        /// </summary>
        /// <response code="200" cref="List{PolicyRule}">Deleted</response>
        /// <response code="206" cref="List{PolicyRule}">Partial Content</response>
        /// <response code="400">Bad Request</response>
        /// <response code="500">Internal Server Error</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [AuditStaticDb(System = AuditDefaults.Altinn2AddRulesApi)]
        [Route("accessmanagement/api/v1/delegations/deletepolicy")]
        public async Task<ActionResult> DeletePolicy([FromBody] RequestToDeletePolicyList policiesToDelete, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Rule> deletionResults = await _rights.EnrichAndTryDeleteDelegationPolicies(policiesToDelete, cancellationToken);
            int countPolicies = DelegationHelper.GetPolicyCount(deletionResults);
            int policiesToDeleteCount = policiesToDelete.Count;

            if (countPolicies == policiesToDeleteCount)
            {
                return StatusCode(200, deletionResults);
            }

            string policiesToDeleteSerialized = JsonSerializer.Serialize(policiesToDelete);
            if (countPolicies > 0)
            {
                string deletionResultsSerialized = JsonSerializer.Serialize(deletionResults);
                _logger.LogInformation("Partial deletion completed deleted {countPolicies} of {policiesToDeleteCount}.\n{deletionResultsSerialized}", countPolicies, policiesToDeleteCount, deletionResultsSerialized);
                return StatusCode(206, deletionResults);
            }

            _logger.LogInformation("Deletion could not be completed. None of the rules could be processed, indicating invalid or incomplete input:\n{policiesToDeleteSerialized}", policiesToDeleteSerialized);
            return StatusCode(400, $"Unable to complete deletion");
        }

        /// <summary>
        /// Endpoint for adding instance delegations for the given instance/offeredby/coveredby.
        /// This method creates a delegation that grants specific rights on a resource instance from one party to another.
        /// </summary>
        /// <param name="from">The UUID of the party offering the delegation (the resource owner)</param>
        /// <param name="to">The UUID of the party receiving the delegation</param>
        /// <param name="by">The UUID of the party performing the delegation</param>
        /// <param name="resource">The resource identifier for which the delegation is being created</param>
        /// <param name="instance">The specific instance identifier of the resource</param>
        /// <param name="delegatedDateTime">The date and time when the delegation is effective, provided in the request body</param>
        /// <param name="rightKeys">The list of right keys to be delegated, provided in the request body</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>A 201 Created response on success, or a problem details response on failure</returns>
        /// <response code="201">Successfully created the instance delegation</response>
        /// <response code="400">Bad Request - Invalid input parameters or resource/instance not found</response>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.ALTINNII_AUTHORIZATION)]
        [AuditStaticDb(System = AuditDefaults.Altinn2AddInstanceDelegationApi)]
        [Route("accessmanagement/api/v1/delegations/addaltinn2instanceright")]
        public async Task<IActionResult> AddAltinn2InstanceRights(
        [Required][FromQuery(Name = "from")] Guid from,
        [Required][FromQuery(Name = "to")] Guid to,
        [Required][FromQuery(Name = "by")] Guid by,
        [Required][FromQuery(Name = "resource")] string resource,
        [Required][FromQuery(Name = "instance")] string instance,
        [Required][FromQuery(Name = "delegatedDateTime")] DateTime delegatedDateTime,
        [Required][FromBody] RightKeyListDto rightKeys,
        CancellationToken cancellationToken = default)
        {
            var fromEntity = await _entityService.GetEntity(from, cancellationToken);
            var toEntity = await _entityService.GetEntity(to, cancellationToken);
            var byEntity = await _entityService.GetEntity(by, cancellationToken);
            var resourceObj = await _resourceService.GetResource(resource, cancellationToken);

            //var result = await _connectionService.AddInstance(fromEntity, toEntity, resourceObj, instance, rightKeys, byEntity, null, cancellationToken);

            //var instanceRules = await GenerateInstanceRules(from, to, resource, instanceId, ruleKeys, performedBy, cancellationToken);

            //var instanceRight = new InstanceRight
            //{
            //    FromUuid = from.Id,
            //    FromType = DelegationHelper.GetUuidTypeFromEntityType(from.TypeId),
            //    ToUuid = to.Id,
            //    ToType = DelegationHelper.GetUuidTypeFromEntityType(to.TypeId),
            //    PerformedBy = performedBy.Id.ToString(),
            //    PerformedByType = DelegationHelper.GetUuidTypeFromEntityType(performedBy.TypeId),
            //    ResourceId = resource.RefId,
            //    InstanceId = instanceId,
            //    InstanceDelegationMode = InstanceDelegationMode.Normal,
            //    InstanceDelegationSource = InstanceDelegationSource.User,
            //    InstanceRules = instanceRules
            //};

            //InstanceRight result = await _pap.TryWriteInstanceDelegationPolicyRules(instanceRight, cancellationToken);

            //if (!result.All(r => r.CreatedSuccessfully))
            //{
                //return Problems.DelegationPolicyRuleWriteFailed;
            //}

            //if (result.IsProblem)
            //{
            //    if (result.Problem.Equals(Core.Errors.Problems.InvalidResource))
            //    {
            //        ProblemDetails problem = result.Problem.ToProblemDetails();
            //        problem.Extensions["resource"] = resource;
            //        problem.Extensions["instance"] = instance;
            //        return problem.ToActionResult();
            //    }

            //    return result.Problem.ToActionResult();
            //}

            return Created();
        }
    }
}
