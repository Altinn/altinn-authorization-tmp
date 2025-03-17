using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Internal.Controllers
{
    /// <summary>
    /// Delegations
    /// </summary>
    [Route("/accessmanagement/api/v1/delegations")]
    [ApiController]
    public class DelegationController(
        IConnectionRepository connectionRepository,
        IConnectionPackageRepository connectionPackageRepository,
        IConnectionResourceRepository connectionResourceRepository,
        IDelegationRepository delegationRepository,
        IDelegationPackageRepository delegationPackageRepository,
        IDelegationResourceRepository delegationResourceRepository,
        IAssignmentRepository assignmentRepository,
        IEntityRepository entityRepository
        ) : ControllerBase
    {
        private readonly IConnectionRepository connectionRepository = connectionRepository;
        private readonly IConnectionPackageRepository connectionPackageRepository = connectionPackageRepository;
        private readonly IConnectionResourceRepository connectionResourceRepository = connectionResourceRepository;
        private readonly IDelegationRepository delegationRepository = delegationRepository;
        private readonly IDelegationPackageRepository delegationPackageRepository = delegationPackageRepository;
        private readonly IDelegationResourceRepository delegationResourceRepository = delegationResourceRepository;
        private readonly IAssignmentRepository assignmentRepository = assignmentRepository;
        private readonly IEntityRepository entityRepository = entityRepository;

        /// <summary>
        /// Create a new assignment
        /// </summary>
        [Route("{id}")]
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ExtDelegation>> Get(Guid id)
        {
            var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            /*
            [X] User is to or TS for from
            */

            var delegation = await delegationRepository.GetExtended(id);
            if (delegation == null)
            {
                return NotFound();
            }

            var userEntity = await entityRepository.Get(userId);

            var filter = connectionRepository.CreateFilterBuilder();
            filter.Equal(t => t.FromId, delegation.FacilitatorId);
            filter.Equal(t => t.ToId, userEntity.Id);
            var connections = await connectionRepository.GetExtended(filter);

            string roleUrn = "urn:altinn:role:hovedadministrator"; // Or something else?
            if (connections.Count(t => t.Role.Urn == roleUrn) == 0)
            {
                return Unauthorized(string.Format("User '{0}' is missing role '{1}' on '{2}'", userId, roleUrn, delegation.FacilitatorId.ToString()));
            }

            return Ok(delegation);
        }

        /// <summary>
        /// Add package to delegation
        /// </summary>
        [Route("")]
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreateDelegationRequestDto request)
        {
            var fromAssignment = await assignmentRepository.Get(request.FromAssignmentId);
            var toAssignment = await assignmentRepository.Get(request.ToAssignmentId);

            if (fromAssignment == null || toAssignment == null)
            {
                // Assignments must exist
                return BadRequest();
            }

            if (!fromAssignment.ToId.Equals(toAssignment.FromId))
            {
                // Assignments must have a link
                return BadRequest();
            }

            var filter = delegationRepository.CreateFilterBuilder();
            filter.Equal(t => t.FromId, fromAssignment.Id);
            filter.Equal(t => t.ToId, toAssignment.Id);
            filter.Equal(t => t.FacilitatorId, fromAssignment.ToId);
            var existsRes = (await delegationRepository.Get(filter)).FirstOrDefault();
            if (existsRes != null)
            {
                // Allready exists
                return Ok(); // 302 Found?
            }

            var res = await delegationRepository.Create(new AccessMgmt.Core.Models.Delegation()
            {
                Id = Guid.NewGuid(),
                FromId = request.FromAssignmentId,
                ToId = request.ToAssignmentId,
                FacilitatorId = Guid.Empty
            });
            if (res > 0)
            {
                // Created
                return Created();
            }

            return Problem();
        }

        /// <summary>
        /// Add package to delegation
        /// </summary>
        [Route("/system")]
        [HttpPost]
        public async Task<ActionResult> CreateDelegationForSystemAgent([FromBody] CreateSystemDelegationRequestDto request)
        {
            var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            //var user = (await entityRepository.Get(userId)) ?? throw new Exception(string.Format("Party not found '{0}'", userId));

            return Problem();
        }

        /// <summary>
        /// Remove package from delegation
        /// </summary>
        [Route("{id}")]
        [HttpDelete]
        public async Task<ActionResult> Delete(Guid id)
        {
            var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var userEntity = await entityRepository.Get(userId);
            var assignment = await assignmentRepository.Get(id);

            if (userEntity == null || assignment == null)
            {
                return BadRequest();
            }

            /*
            [X] User must have role:TS on Facilitator
            */

            var filter = connectionRepository.CreateFilterBuilder();
            filter.Equal(t => t.FromId, id);
            filter.Equal(t => t.ToId, userEntity.Id);
            var connections = await connectionRepository.GetExtended(filter);

            string roleUrn = "urn:altinn:role:tilgangsstyrer"; // Or something else?
            if (connections.Count(t => t.Role.Urn == roleUrn) == 0)
            {
                return Unauthorized(string.Format("User '{0}' is missing role '{1}' on '{2}'", userId, roleUrn, assignment.FromId.ToString()));
            }

            try
            {
                var res = await delegationRepository.Delete(id);
                if (res > 0)
                {
                    return NoContent();
                }

                throw new Exception("Unable to delete delegation");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// Add package to delegation
        /// </summary>
        [Route("{id}/package/{packageId}")]
        [HttpPost]
        public async Task<ActionResult> AddPackage(Guid id, Guid packageId)
        {
            /*
            
            - User must have role:TS on Facilitator
            - Package must exist in /connection/{assignment_id}/packages
            - Package must be delegable

            */

            try
            {
                var res = await delegationPackageRepository.CreateCross(id, packageId);
                if (res > 0)
                {
                    return Created();
                }

                throw new Exception("Unable to add package to delegation");
            }
            catch (Exception ex) 
            {
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// Remove package from delegation
        /// </summary>
        [Route("{id}/package/{packageId}")]
        [HttpDelete]
        public async Task<ActionResult> RemovePackage(Guid id, Guid packageId)
        {
            /*

           - User must have role:TS on Facilitator
           - Package should exist in /connection/{delegation_id}/packages

           */

            try
            {
                var res = await delegationPackageRepository.DeleteCross(id, packageId);
                if (res > 0)
                {
                    return NoContent();
                }

                throw new Exception("Unable to remove package from delegation");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}

/// <summary>
/// RequestDto for CreateDelegation
/// </summary>
public class CreateDelegationRequestDto
{
    /// <summary>
    /// Assignment identifier (From)
    /// </summary>
    public Guid FromAssignmentId { get; set; }

    /// <summary>
    /// Assignment identifier (To)
    /// </summary>
    public Guid ToAssignmentId { get; set; }
}

/// <summary>
/// RequestDto to create Delegation and required assignments for System
/// </summary>
public class CreateSystemDelegationRequestDto
{
    /// <summary>
    /// Client party uuid
    /// </summary>
    public Guid ClientPartyId { get; set; }

    /// <summary>
    /// Facilitator party uuid
    /// </summary>
    public Guid FacilitatorPartyId { get; set; }

    /// <summary>
    /// Client role (From -> Facilitator)
    /// e.g REGN/REVI
    /// </summary>
    public string ClientRole { get; set; } = string.Empty;

    /// <summary>
    /// Agent party uuid
    /// </summary>
    public Guid AgentPartyId { get; set; }

    /// <summary>
    /// Agent name (need to create new party)
    /// System displayName
    /// </summary>
    public Guid AgentName { get; set; }

    /// <summary>
    /// Agent role (Facilitator -> Agent)
    /// e.g Agent
    /// </summary>
    public string AgentRole { get; set; } = string.Empty;

    /// <summary>
    /// Packages to be delegated to Agent
    /// </summary>
    public string[] Packages { get; set; } = [];
}

/*
// Do we need a RequestDto?  
public class AddDelegationPackageRequestDto
{
    public Guid DelegationId { get; set; }
    public Guid PackageId { get; set; }
}
*/
